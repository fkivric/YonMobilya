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
    public partial class frmAnaSayfa : System.Web.UI.Page
    {
        admin admin = new admin();
        public static string ConnectionString = "Server=192.168.4.24;Database=VDB_YON01;User Id=sa;Password=MagicUser2023!;";
        SqlConnection sql = new SqlConnection(ConnectionString);
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var loginRes = (List<LoginObj>)Session["Login"];
                if (loginRes != null)
                {
                    LoadEventCounts();
                    BindGrid();
                }
                else
                {
                    Response.Redirect("NewLogin.aspx");
                }
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            }
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
                string q = String.Format(@"select CDRSALID,CDRCURID,CURVAL,CURNAME,ORDDATE,sum(ORDCHBALANCEQUAN) as ORDCHBALANCEQUAN,Convert(numeric(18,2),sum(ORDCHBALANCEQUAN*PRLPRICE)) as PRLPRICE,CURCHCOUNTY
                FROM  CUSDELIVER
                inner join MDE_GENEL.dbo.MB_Islemler on MB_SALID = CDRSALID and CDRORDCHID = MB_ORDCHID
                left outer join CURRENTS on CURID = CDRCURID
                left outer join CURRENTSCHILD on CURCHID = CURID
                left outer join ORDERS on ORDSALID = CDRSALID
                left outer join ORDERSCHILD on ORDCHORDID = ORDID and ORDCHID = MB_ORDCHID and CDRORDCHID = ORDCHID
                left outer join DEFSTORAGE WITH (NOLOCK) ON CDRSTORID = DSTORID
                left outer join DIVISON TESLIM WITH (NOLOCK) ON TESLIM.DIVVAL = DSTORVAL AND ORDCHCOMPANY = TESLIM.DIVCOMPANY
                outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = ORDCHPROID and PRLDPRID = 740) as pesinfiyat
                where MB_Tamamlandi = 0 and MB_SUPCURVAL = '{0}'  and ORDCHBALANCEQUAN > 0
                group by CDRSALID,CDRCURID,CURVAL,CURNAME,ORDDATE,CURCHCOUNTY
                order by ORDDATE,CURCHCOUNTY,CURNAME", loginRes[0].CURVAL);
                var dt = DbQuery.Query(q, ConnectionString);
                if (dt != null)
                {
                    toplamadet.InnerText = dt.Rows.Count.ToString();
                    double ciro = 0;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        ciro = ciro + double.Parse(dt.Rows[0]["PRLPRICE"].ToString());
                    }
                    toplamciro.InnerText = ciro.ToString("C", new CultureInfo("tr-TR"));
                    hakedis.InnerText = (ciro * 0.08).ToString("C", new CultureInfo("tr-TR"));
                }
                else
                {
                    toplamadet.InnerText = "0";
                    toplamciro.InnerText = "0";
                    hakedis.InnerText = "0";
                }
            }
        }
        private void LoadEventCounts()
        {
            string query = @"
            SELECT DISTINCT 
                MB_PlanTarih AS PlanTarih, 
                MB_SALID AS ID, 
                CURNAME,
				CURID 
            FROM MDE_GENEL.dbo.MB_Islemler
            LEFT OUTER JOIN SALES ON SALID = MB_SALID
            LEFT OUTER JOIN CURRENTS ON CURID = SALCURID
            where MB_SALID != 0";

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
                    int CURID = int.Parse(reader["CURID"].ToString());

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