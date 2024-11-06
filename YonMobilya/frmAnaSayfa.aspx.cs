using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.DynamicData;
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
        CultureInfo culture = new CultureInfo("tr-TR");
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var loginRes = (List<LoginObj>)Session["Login"];
                if (loginRes != null)
                {
                    LoadEventCounts();
                    BindGrid();
                    //Convert.ToDateTime(dataTable.Rows[0]["Tarih"].ToString()).ToString("dd MMMM yyyy", new CultureInfo("tr-TR")) + " Tarihli Güncelleme";
                    AyIsmi.InnerText = DateTime.Now.ToString("MMMM",new CultureInfo("tr-TR")) + " - "+ DateTime.Now.ToString("yyyy", new CultureInfo("tr-TR")) + " Proje Durumu";
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
                string q = String.Format(@"select count(adet) as Toplamadet, sum(tutar) as Toplamtutar from (
                select distinct Convert(varchar(50),MB_SALID) as adet, sum(PRLPRICE) as tutar from MDE_GENEL..MB_Islemler
				inner join CUSDELIVER T on MB_SALID = CDRSALID and CDRORDCHID = MB_ORDCHID
                outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = MB_PROID and PRLDPRID = 740) as pesinfiyat
                where MB_Tamamlandi = 0 and MB_SALID != 0 and MB_SUPCURVAL = 'T003387' and CDRSHIPVAL = 'ANTMOB' 
				and not exists (select top 1 * from CUSDELIVER i WITH (NOLOCK) where i.CDRBASECANID = t.CDRID and T.CDRSALID = i.CDRSALID and i.CDRORDCHID = T.CDRORDCHID)
                group by MB_SALID
                union
                select outd.DIVNAME as ISTEYEN,sum(PRLPRICE) as TUTAR
                            from PRODEMAND 
                            left outer join PRODUCTS on PROID = PRDEPROID
                            left outer join DEFSTORAGE outs on outs.DSTORID = PRDEDSTORIDOUT
                            left outer join DEFSTORAGE ins on ins.DSTORID = PRDEDSTORIDIN
                            left outer join DIVISON outd on outd.DIVVAL = ins.DSTORDIVISON
                            left outer join DIVISON ind on ind.DIVVAL = outs.DSTORDIVISON
			                inner join MDE_GENEL.dbo.MB_Islemler on MB_ORDCHID = PRDEID
                            outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = PROID and PRLDPRID = 740) as pesinfiyat
                            where PRDESTS = 0 and PRDEKIND= 1 and MB_Tamamlandi = 0 
                            and MB_SUPCURVAL = '{0}'
                            group by outd.DIVVAL,outd.DIVNAME,MB_PlanTarih,ind.DIVNAME) sonuc", loginRes[0].CURVAL);
                string qq = String.Format(@"select count(adet) as Toplamadet, sum(tutar) as Toplamtutar from (
                select distinct Convert(varchar(50),MB_SALID) as adet, sum(PRLPRICE) as tutar from MDE_GENEL..MB_Islemler
                outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = MB_PROID and PRLDPRID = 740) as pesinfiyat
                where MB_Tamamlandi = 1 and MB_SALID != 0 and MB_SUPCURVAL = '{0}'
                group by MB_SALID
                union
                select outd.DIVNAME as ISTEYEN,sum(PRLPRICE) as TUTAR
                            from PRODEMAND 
                            left outer join PRODUCTS on PROID = PRDEPROID
                            left outer join DEFSTORAGE outs on outs.DSTORID = PRDEDSTORIDOUT
                            left outer join DEFSTORAGE ins on ins.DSTORID = PRDEDSTORIDIN
                            left outer join DIVISON outd on outd.DIVVAL = ins.DSTORDIVISON
                            left outer join DIVISON ind on ind.DIVVAL = outs.DSTORDIVISON
			                inner join MDE_GENEL.dbo.MB_Islemler on MB_ORDCHID = PRDEID
                            outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = PROID and PRLDPRID = 740) as pesinfiyat
                            where PRDESTS = 0 and PRDEKIND= 1 and MB_Tamamlandi = 1 
                            and MB_SUPCURVAL = '{0}'
                            group by outd.DIVVAL,outd.DIVNAME,MB_PlanTarih,ind.DIVNAME) sonuc", loginRes[0].CURVAL);
                //string q = String.Format(@"select CDRSALID,CDRCURID,CURVAL,CURNAME,ORDDATE,sum(ORDCHBALANCEQUAN) as ORDCHBALANCEQUAN,Convert(numeric(18,2),sum(ORDCHBALANCEQUAN*PRLPRICE)) as PRLPRICE,CURCHCOUNTY
                //FROM  CUSDELIVER
                //inner join MDE_GENEL.dbo.MB_Islemler on MB_SALID = CDRSALID and CDRORDCHID = MB_ORDCHID
                //left outer join CURRENTS on CURID = CDRCURID
                //left outer join CURRENTSCHILD on CURCHID = CURID
                //left outer join ORDERS on ORDSALID = CDRSALID
                //left outer join ORDERSCHILD on ORDCHORDID = ORDID and ORDCHID = MB_ORDCHID and CDRORDCHID = ORDCHID
                //left outer join DEFSTORAGE WITH (NOLOCK) ON CDRSTORID = DSTORID
                //left outer join DIVISON TESLIM WITH (NOLOCK) ON TESLIM.DIVVAL = DSTORVAL AND ORDCHCOMPANY = TESLIM.DIVCOMPANY
                //outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = MB_PROID and PRLDPRID = 740) as pesinfiyat
                //where MB_Tamamlandi = 0 and MB_SUPCURVAL = '{0}'  --and ORDCHBALANCEQUAN > 0
                //group by CDRSALID,CDRCURID,CURVAL,CURNAME,ORDDATE,CURCHCOUNTY
                //order by ORDDATE,CURCHCOUNTY,CURNAME", loginRes[0].CURVAL);
                //string qq = String.Format(@"select CDRSALID,CDRCURID,CURVAL,CURNAME,ORDDATE,sum(ORDCHBALANCEQUAN) as ORDCHBALANCEQUAN,Convert(numeric(18,2),sum(ORDCHBALANCEQUAN*PRLPRICE)) as PRLPRICE,CURCHCOUNTY
                //FROM  CUSDELIVER
                //inner join MDE_GENEL.dbo.MB_Islemler on MB_SALID = CDRSALID and CDRORDCHID = MB_ORDCHID
                //left outer join CURRENTS on CURID = CDRCURID
                //left outer join CURRENTSCHILD on CURCHID = CURID
                //left outer join ORDERS on ORDSALID = CDRSALID
                //left outer join ORDERSCHILD on ORDCHORDID = ORDID and ORDCHID = MB_ORDCHID and CDRORDCHID = ORDCHID
                //left outer join DEFSTORAGE WITH (NOLOCK) ON CDRSTORID = DSTORID
                //left outer join DIVISON TESLIM WITH (NOLOCK) ON TESLIM.DIVVAL = DSTORVAL AND ORDCHCOMPANY = TESLIM.DIVCOMPANY
                //outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = MB_PROID and PRLDPRID = 740) as pesinfiyat
                //where MB_Tamamlandi = 1 and MB_SUPCURVAL = '{0}'  --and ORDCHBALANCEQUAN > 0
                //group by CDRSALID,CDRCURID,CURVAL,CURNAME,ORDDATE,CURCHCOUNTY
                //order by ORDDATE,CURCHCOUNTY,CURNAME", loginRes[0].CURVAL);
                var dt = DbQuery.Query(q, ConnectionString);
                var dt2 = DbQuery.Query(qq, ConnectionString);

                if (dt != null)
                {
                    bekleyenadet.InnerText = dt.Rows[0]["Toplamadet"].ToString();
                    double ciro = 0;
                    ciro = double.Parse(dt.Rows[0]["Toplamtutar"].ToString());
                    bekleyenciro.InnerText = ciro.ToString("C", new CultureInfo("tr-TR"));
                    bekleyenhakedis.InnerText = (ciro * 0.08).ToString("C", new CultureInfo("tr-TR"));
                    //bekleyenadet.InnerText = dt.Rows.Count.ToString();
                    //double ciro = 0;
                    //for (int i = 0; i < dt.Rows.Count; i++)
                    //{
                    //    ciro = ciro + double.Parse(dt.Rows[0]["PRLPRICE"].ToString());
                    //}
                    //bekleyenciro.InnerText = ciro.ToString("C", new CultureInfo("tr-TR"));
                    //bekleyenhakedis.InnerText = (ciro * 0.08).ToString("C", new CultureInfo("tr-TR"));
                }
                else
                {
                    bekleyenadet.InnerText = "0";
                    bekleyenciro.InnerText = "0";
                    bekleyenhakedis.InnerText = "0";


                }
                if (dt2 != null)
                {
                    tamamlananadet.InnerText = dt2.Rows[0]["Toplamadet"].ToString();
                    double ciro2 = 0;
                    ciro2 = double.Parse(dt2.Rows[0]["Toplamtutar"].ToString());
                    tamamalananciro.InnerText = ciro2.ToString("C", new CultureInfo("tr-TR"));
                    tamamlananhakedis.InnerText = (ciro2 * 0.08).ToString("C", new CultureInfo("tr-TR"));
                    //tamamlananadet.InnerText = dt2.Rows.Count.ToString();
                    //double ciro2 = 0;
                    //for (int i = 0; i < dt2.Rows.Count; i++)
                    //{
                    //    ciro2 = ciro2 + double.Parse(dt2.Rows[0]["PRLPRICE"].ToString());
                    //}
                    //tamamalananciro.InnerText = ciro2.ToString("C", new CultureInfo("tr-TR"));
                    //tamamlananhakedis.InnerText = (ciro2 * 0.08).ToString("C", new CultureInfo("tr-TR"));
                }
                else
                {
                    tamamlananadet.InnerText = "0";
                    tamamalananciro.InnerText = "0";
                    tamamlananhakedis.InnerText = "0";
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
				CURID,
                MB_Tamamlandi
            FROM MDE_GENEL.dbo.MB_Islemler
            LEFT OUTER JOIN SALES ON SALID = MB_SALID
            LEFT OUTER JOIN CURRENTS ON CURID = SALCURID
            where MB_SALID != 0 --and MB_Tamamlandi = 0";

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
                    Islem.COMPLATE = bool.Parse(reader["MB_Tamamlandi"].ToString());
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