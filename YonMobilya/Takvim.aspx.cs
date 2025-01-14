﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using YonMobilya.Class;
using static YonMobilya.Class.Siniflar;

namespace YonMobilya
{
    public partial class Takvim : System.Web.UI.Page
    {
        public static string ConnectionString = "Server=192.168.4.24;Database=VDB_YON01;User Id=sa;Password=MagicUser2023!;";
        SqlConnection sql = new SqlConnection(ConnectionString);
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var loginRes = (List<LoginObj>)Session["Login"];
                if (loginRes != null)
                {
                    // MSSQL'den verileri çekin
                    //Dictionary<string, string> data = GetCalendarDataFromDatabase();
                    //var calendarData = GetCalendarDataFromDatabase(); // Veritabanından veriyi alıyoruz
                    //// Verileri JavaScript'e JSON olarak aktarın
                    //JavaScriptSerializer js = new JavaScriptSerializer();
                    //string jsonData = js.Serialize(data);

                    //// JSON veriyi sayfaya yazdırıyoruz
                    //ClientScript.RegisterStartupScript(this.GetType(), "CalendarData", "var calendarData = " + jsonData + ";", true);


                    //var calendarData2 = GetCalendarDataFromDatabase3();

                    //// JSON formatına çevirip JavaScript'e yolluyoruz
                    //string jsonData2 = Newtonsoft.Json.JsonConvert.SerializeObject(calendarData2);
                    //ClientScript.RegisterStartupScript(this.GetType(), "eventCounts", $"var eventCounts = {jsonData2};", true);
                    LoadEventCounts();
                    //BindGrid();
                }
                else
                {
                    Response.Redirect("NewLogin.aspx");
                }
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            }
        }

        private Dictionary<string, string> GetCalendarDataFromDatabase()
        {
            // MSSQL'den verilerinizi çekeceğiniz kısım
            Dictionary<string, string> calendarData = new Dictionary<string, string>();

            string query = "select CONVERT(varchar, MB_PlanTarih, 23) as EventDate, count(MB_ID) as EventID from MDE_GENEL.dbo.MB_Islemler group by MB_PlanTarih"; // Tarih ve ID kolonlarını seçiyoruz.

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string formattedDate = reader["EventDate"].ToString();  // Tarih yyyy-mm-dd formatında geliyor
                    int eventID = Convert.ToInt32(reader["EventID"]);

                    calendarData[formattedDate] = reader["EventDate"].ToString();
                }
            }

            return calendarData;
        }
        private Dictionary<string, int> GetCalendarDataFromDatabase2()
        {
            // MSSQL'den verilerinizi çekeceğiniz kısım
            Dictionary<string, int> calendarData = new Dictionary<string, int>();

            string query = "SELECT CONVERT(varchar, MB_PlanTarih, 23) AS PlanTarih, COUNT(MB_ID) AS IslemAdet FROM MDE_GENEL.dbo.MB_Islemler GROUP BY MB_PlanTarih"; // Tarih ve ID kolonlarını seçiyoruz.

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string formattedDate = reader["PlanTarih"].ToString();  // Tarih yyyy-mm-dd formatında geliyor
                    int eventID = Convert.ToInt32(reader["IslemAdet"]);

                    calendarData[formattedDate] = eventID;
                }
            }

            return calendarData;
        }

        private Dictionary<string, List<int>> GetCalendarDataFromDatabase3()
        {
            Dictionary<string, List<int>> calendarData = new Dictionary<string, List<int>>();

            string query = @"select distinct MB_PlanTarih as PlanTarih,MB_SALID as ID ,CURNAME as CURNAME from MDE_GENEL.dbo.MB_Islemler
            left outer join SALES on SALID = MB_SALID
            left outer join CURRENTS on CURID = SALCURID"; // Tarih ve ID kolonlarını seçiyoruz.

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string date = reader["PlanTarih"].ToString(); // yyyy-mm-dd formatında tarih
                    int id = Convert.ToInt32(reader["ID"]);

                    if (!calendarData.ContainsKey(date))
                    {
                        calendarData[date] = new List<int>();
                    }

                    calendarData[date].Add(id); // Aynı gün için birden fazla ID ekliyoruz
                }
            }

            return calendarData;
        }

        private void BindGrid()
        {
            var loginRes = (List<LoginObj>)Session["Login"];
            if (loginRes != null)
            {
                string magaza = "";
                if (loginRes[0].DIVVAL.Length >= 3)
                {
                    var magazalar = DbQuery.Query($"select * from MDE_GENEL.dbo.FK_fn_Split('{loginRes[0].DIVVAL.ToString()}',',')", ConnectionString);
                    for (int i = 0; i < magazalar.Rows.Count; i++)
                    {
                        if (i == 0)
                        {
                            magaza = "'" + magazalar.Rows[i][0].ToString() + "'";
                        }
                        else
                        {
                            magaza = magaza + ",'" + magazalar.Rows[i][0].ToString() + "'";
                        }
                    }
                }
                else
                {
                    magaza = loginRes[0].DIVVAL.ToString();
                }
                string q = String.Format(@"select count(adet) as Toplamadet, sum(tutar) as Toplamtutar from (
                select distinct Convert(varchar(50),MB_SALID) as adet, sum(PRLPRICE) as tutar from MDE_GENEL..MB_Islemler
                outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = MB_PROID and PRLDPRID = 740) as pesinfiyat
                where MB_Tamamlandi = 0 and MB_SALID != 0 and MB_SUPCURVAL = '{0}'
                group by MB_SALID
                --union
                --select outd.DIVNAME as ISTEYEN,sum(PRLPRICE) as TUTAR
                --            from PRODEMAND 
                --            left outer join PRODUCTS on PROID = PRDEPROID
                --            left outer join DEFSTORAGE outs on outs.DSTORID = PRDEDSTORIDOUT
                --            left outer join DEFSTORAGE ins on ins.DSTORID = PRDEDSTORIDIN
                --            left outer join DIVISON outd on outd.DIVVAL = ins.DSTORDIVISON
                --            left outer join DIVISON ind on ind.DIVVAL = outs.DSTORDIVISON
			    --            inner join MDE_GENEL.dbo.MB_Islemler on MB_ORDCHID = PRDEID
                --            outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = PROID and PRLDPRID = 740) as pesinfiyat
                --            where PRDESTS = 0 and PRDEKIND= 1 and MB_Tamamlandi = 0 
                --            and MB_SUPCURVAL = '{0}'
                --            group by outd.DIVVAL,outd.DIVNAME,MB_PlanTarih,ind.DIVNAME
                ) sonuc", loginRes[0].CURVAL);
                //var dt = DbQuery.Query(q, ConnectionString);
                //if (dt != null)
                //{
                //    toplamadet.InnerText = dt.Rows[0]["Toplamadet"].ToString();
                //    double ciro = 0;
                //    ciro = double.Parse(dt.Rows[0]["Toplamtutar"].ToString());
                //    toplamciro.InnerText = ciro.ToString("C", new CultureInfo("tr-TR"));
                //    hakedis.InnerText = (ciro * 0.08).ToString("C", new CultureInfo("tr-TR"));
                //}
                //else
                //{
                //    toplamadet.InnerText = "0";
                //    toplamciro.InnerText = "0";
                //    hakedis.InnerText = "0";
                //}
            }
        }
        private void LoadEventCounts()
        {
            var loginRes = (List<LoginObj>)Session["Login"];
            string query = String.Format(@"
            SELECT DISTINCT 
                MB_PlanTarih AS PlanTarih, 
                MB_SALID AS ID, 
                CURNAME,
                CURID
            FROM MDE_GENEL.dbo.MB_Islemler
            LEFT OUTER JOIN SALES ON SALID = MB_SALID
            LEFT OUTER JOIN CURRENTS ON CURID = SALCURID
			LEFT OUTER JOIN ORDERSCHILD on ORDCHORDID = (select ORDID from ORDERS where ORDSALID = SALID)
            left outer join SOCIAL on SOCODE = 'TT-' + MB_Ekleyen
			where MB_SALID != 0 and MB_Tamamlandi = 0 and SOCODE = '{0}'", loginRes[0].SOCODE); 

            // Sonuçları tutacak bir sözlük
            Dictionary<string, List<Islemler>> eventCounts = new Dictionary<string, List<Islemler>>();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var Islem = new Islemler();
                    string planTarih = Convert.ToDateTime(reader["PlanTarih"]).ToString("yyyy-MM-dd");

                    Islem.ID = reader["ID"].ToString();
                    Islem.CURNAME = reader["CURNAME"].ToString();
                    Islem.CURID = int.Parse(reader["CURID"].ToString());
                    int id = Convert.ToInt32(reader["ID"]);
                    string curName = reader["CURNAME"].ToString();

                    // Eğer bu tarih zaten varsa, listeye yeni bir değer ekliyoruz
                    if (!eventCounts.ContainsKey(planTarih))
                    {
                        eventCounts[planTarih] = new List<Islemler>();
                    }
                    // Tarih için ID ve CURNAME değerini ekliyoruz
                    eventCounts[planTarih].Add(Islem);
                }
                reader.Close();
            }

            // Sözlüğü JSON formatına çevir
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            string jsonEventCounts = jsSerializer.Serialize(eventCounts);

            // Bu JSON verisini JavaScript'e aktar
            ClientScript.RegisterStartupScript(this.GetType(), "eventCounts", "var eventCounts = " + jsonEventCounts + ";", true);            
        }
        protected void UpdateCalendar()
        {
            LoadEventCounts();
        }
    }
}