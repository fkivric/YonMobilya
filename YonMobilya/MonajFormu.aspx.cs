using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using YonMobilya.Class;
using static YonMobilya.Class.Siniflar;
using static YonMobilya.Montaj;

namespace YonMobilya
{
    public partial class MonajFormu : System.Web.UI.Page
    {
        public static string ConnectionString = "Server=192.168.4.24;Database=VDB_YON01;User Id=sa;Password=MagicUser2023!;";
        public static string ConnectionString2 = "Server=192.168.4.24;Database=MDE_GENEL;User Id=sa;Password=MagicUser2023!;";
        SqlConnection sql = new SqlConnection(ConnectionString);
        SqlConnection sql2 = new SqlConnection(ConnectionString2);
        public static string CURID = "";
        public static string SALID = "";
        public static string ORDCHID = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var loginRes = (List<LoginObj>)Session["Login"];
                if (loginRes != null)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "showLoading", "showLoading();", true);
                    string[] salidArray = Request.QueryString["salid"].Split(',');
                    

                    SALID = Request.QueryString["salid"];
                    CURID = Request.QueryString["curid"]; //DbQuery.GetValue($"select SALCURID from SALES where SALID = {SALID}");
                    if (CURID != "0" && CURID != "")
                    {
                        tarih.InnerText = DateTime.Parse(DbQuery.GetValue($"select distinct MB_PlanTarih from MDE_GENEL.dbo.MB_Islemler\r\nwhere MB_ORDCHID in ({SALID})")).ToString("dd/MM/yyyy");
                        magaza.InnerText = DbQuery.GetValue($"select distinct DIVNAME from ORDERSCHILD\r\nleft outer join ORDERS on ORDID = ORDCHORDID\r\nleft outer join DIVISON on DIVVAL = ORDDIVISON\r\nwhere ORDCHID in ({SALID})");

                        kurlumcu.InnerText = DbQuery.GetValue($"select distinct OFFCURNAME from MDE_GENEL.dbo.MB_Islemler\r\nleft outer join OFFICALCUR on OFFCURID = MB_Ekleyen\r\nwhere MB_ORDCHID in ({SALID})");


                        using (SqlConnection conn = new SqlConnection(ConnectionString))
                        {
                            conn.Open();
                            string query = $"select CURCHTITLE as MusteriAdi,CURCHADR1 + ' ' + CURCHADR2 + ' ' + CURCHCOUNTY + ' ' + CURCHCITY as Adres,CURCHGSM1 as Tel from CURRENTSCHILD where CURCHID = {CURID}";
                            // Tablo ve şartları belirtin
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                SqlDataReader reader = cmd.ExecuteReader();
                                if (reader.Read())
                                {
                                    musteri.InnerText = reader["MusteriAdi"].ToString();
                                    adres.InnerText = reader["Adres"].ToString();
                                    tel.InnerText = reader["Tel"].ToString();
                                    musteriimza.InnerText = reader["MusteriAdi"].ToString();

                                }
                            }
                        }
                        using (SqlConnection conn = new SqlConnection(ConnectionString))
                        {
                            conn.Open();
                            // Ürün verileri çekme ve tabloya ekleme
                            string query = $"select PROVAL as Kodu,PRONAME as UrunAdi,ORDCHQUAN as Adet,ORDCHNTNOTES as Aciklama from ORDERSCHILD\r\nleft outer join PRODUCTS on PROID = ORDCHPROID\r\nleft outer join ORDERSCHILDNOTES on ORDCHNTID = ORDCHID\r\nwhere ORDCHID in ({SALID})";
                            using (SqlCommand cmd2 = new SqlCommand(query, conn))
                            {
                                SqlDataReader reader = cmd2.ExecuteReader();
                                ProductRepeater.DataSource = reader;
                                ProductRepeater.DataBind();
                            }
                        }
                    }
                    else
                    {
                        tarih.InnerText = DateTime.Parse(DbQuery.GetValue($"select distinct MB_PlanTarih from MDE_GENEL.dbo.MB_Islemler\r\nwhere MB_ORDCHID in ({SALID})")).ToString("dd/MM/yyyy");
                        magaza.InnerText = DbQuery.GetValue($"select distinct DIVNAME from PRODEMAND\r\nleft outer join DIVISON on DIVVAL = PRDEDIVISON\r\nwhere PRDEID in ({SALID})");

                        kurlumcu.InnerText = DbQuery.GetValue($"select distinct OFFCURNAME from MDE_GENEL.dbo.MB_Islemler\r\nleft outer join OFFICALCUR on OFFCURID = MB_Ekleyen\r\nwhere MB_ORDCHID in ({SALID})");



                        using (SqlConnection conn = new SqlConnection(ConnectionString))
                        {
                            conn.Open();
                            string query = $"select distinct DIVNAME as MusteriAdi,DIVADR1 + ' ' + DIVADR2 + '/' + DCITYNAME as Adres,DIVPHN1 as Tel from PRODEMAND\r\nleft outer join DIVISON on DIVVAL = PRDEDIVISON\r\nleft outer join DEFCITY on DIVCITY = DCITYVAL\r\nwhere PRDEID in ({SALID})";
                            // Tablo ve şartları belirtin
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                SqlDataReader reader = cmd.ExecuteReader();
                                if (reader.Read())
                                {
                                    musteri.InnerText = reader["MusteriAdi"].ToString();
                                    adres.InnerText = reader["Adres"].ToString();
                                    tel.InnerText = reader["Tel"].ToString();
                                    musteriimza.InnerText = reader["MusteriAdi"].ToString();

                                }
                            }
                        }

                        using (SqlConnection conn = new SqlConnection(ConnectionString))
                        {
                            conn.Open();
                            // Ürün verileri çekme ve tabloya ekleme
                            string query = $@"select PROVAL as Kodu,PRONAME as UrunAdi,Convert(int,PRDEQUAN) as Adet,PRDENOTES as Aciklama
				            FROM MDE_GENEL.dbo.MB_Islemler
				            left outer join PRODEMAND on PRDEID = MB_ORDCHID
				            left outer join PRODUCTS on PROID = MB_PROID
				            where PRDEKIND= 1 and PRDESTS = 0
				            and PRDEID in ({SALID})
			                and MB_Tamamlandi = 0
                            order by 1 desc";
                            using (SqlCommand cmd2 = new SqlCommand(query, conn))
                            {
                                SqlDataReader reader = cmd2.ExecuteReader();
                                ProductRepeater.DataSource = reader;
                                ProductRepeater.DataBind();
                            }
                        }
                    }
                    // Yazdırma işlemi için JavaScript ekle
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "PrintPage", "window.print();", true);
                }
                else
                {
                    Response.Redirect("NewLogin.aspx");
                }
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "hideLoading", "hideLoading();", true);
            }

        }

    }
}