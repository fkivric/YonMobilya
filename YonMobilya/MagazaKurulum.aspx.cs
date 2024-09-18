using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using YonMobilya.Class;
using static YonMobilya.Class.Siniflar;

namespace YonMobilya
{
    public partial class MagazaKurulum : System.Web.UI.Page
    {
        public static string ConnectionString = "Server=192.168.4.24;Database=VDB_YON01;User Id=sa;Password=MagicUser2023!;";
        public static string ConnectionString2 = "Server=192.168.4.24;Database=MDE_GENEL;User Id=sa;Password=MagicUser2023!;";
        SqlConnection sql = new SqlConnection(ConnectionString);
        SqlConnection sql2 = new SqlConnection(ConnectionString);
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var loginRes = (List<LoginObj>)Session["Login"];
                if (loginRes != null)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "showLoading", "showLoading();", true);
                    TeslimatListesi();
                    ScriptManager.RegisterStartupScript(this, GetType(), "hideLoading", "hideLoading();", true);
                }
                else
                {
                    Response.Redirect("NewLogin.aspx");
                }
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            }
        }

        void TeslimatListesi()
        {
            string magaza = "";
            var loginRes = (List<LoginObj>)Session["Login"];
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
            string q = String.Format(@"select PRDEDATE,outd.DIVVAL,outd.DIVNAME as ISTEYEN,ind.DIVNAME as VEREN,sum(PRDEQUAN) as ADET,sum(PRLPRICE) as TUTAR
            from PRODEMAND 
            left outer join PRODUCTS on PROID = PRDEPROID
            left outer join DEFSTORAGE outs on outs.DSTORID = PRDEDSTORIDOUT
            left outer join DEFSTORAGE ins on ins.DSTORID = PRDEDSTORIDIN
            left outer join DIVISON outd on outd.DIVVAL = ins.DSTORDIVISON
            left outer join DIVISON ind on ind.DIVVAL = outs.DSTORDIVISON
            outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = PROID and PRLDPRID = 740) as pesinfiyat
            where PRDESTS = 0 and PRDEKIND= 1
            AND PRDEDIVISON in ({0})
            group by outd.DIVVAL,outd.DIVNAME,PRDEDATE,ind.DIVNAME", magaza);
            SqlDataAdapter da = new SqlDataAdapter(q, ConnectionString);
            DataTable dt = new DataTable();
            da.Fill(dt);
            GridView1.DataSource = dt;
            GridView1.DataBind();
            toplamadet.InnerText = dt.Rows.Count.ToString();
            double ciro = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                ciro = ciro + double.Parse(dt.Rows[0]["TUTAR"].ToString());
            }
            toplamciro.InnerText = ciro.ToString("C", new CultureInfo("tr-TR"));
            hakedis.InnerText = (ciro * 0.08).ToString("C", new CultureInfo("tr-TR"));

            Page.ClientScript.RegisterStartupScript(
                this.GetType(),
                "HideSpinner",
                "window.onload = function() { document.getElementById('loadingSpinner').style.display = 'none'; };",
                true
            );

        }

        protected void GridView1_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.Cells.Count > 1)
            {
                e.Row.Cells[2].Visible = false;
            }
        }

        private static int musterirow = 0;
        public static string CURVAL = "";
        protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "Page")
            {
                // Butonun hangi satırda olduğunu belirleyin
                int index = Convert.ToInt32(e.CommandArgument);
                GridViewRow row = GridView1.Rows[index];

                var CURID = GridView1.Rows[index].Cells[2].Text;
                var PRDEDATE = Convert.ToDateTime(GridView1.Rows[index].Cells[1].Text).ToString("yyyy-MM-dd");
                CURVAL = GridView1.Rows[index].Cells[3].Text;
                var CURNAME = System.Web.HttpUtility.HtmlDecode(GridView1.Rows[index].Cells[5].Text);
                // SQL Sorgusu ile veriyi al
                var SALID = GridView1.Rows[index].Cells[1].Text;

                string w = String.Format(@"select CURCHCITY,CURCHCOUNTY,CURCHADR1 + ' ' + CURCHADR2 as CURCHADR from CURRENTSCHILD where CURCHID = {0}", CURID);
                var adr = DbQuery.Query(w, ConnectionString);
                string City = "";
                string County = "";
                string Adrs = "";
                if (adr.Rows.Count > 0)
                {
                    City = adr.Rows[0]["CURCHCITY"].ToString();
                    County = adr.Rows[0]["CURCHCOUNTY"].ToString();
                    Adrs = adr.Rows[0]["CURCHADR"].ToString();
                }
                // JavaScript kodunu oluştur
                string script = $@"
            <script type='text/javascript'>
                $(document).ready(function () {{
                    $('#customerCURNAME').val('{CURVAL}');
                    $('#customerCURVAL').val('{CURNAME}');
                    $('#customerCity').val('{City}');
                    $('#customerCounty').val('{County}');
                    $('#customerAdres').val('{Adrs}');
                    showModal();
                }});
            </script>";

                // Scripti sayfaya ekle
                ClientScript.RegisterStartupScript(
                    this.GetType(),
                    "showModal",
                    script,
                    false
                );
                customerDATE.Value = DateTime.Now.ToString("yyyy-MM-dd");
                string query = String.Format(@"select * from PRODEMAND 
                left outer join PRODUCTS on PROID = PRDEPROID
                left outer join DIVISON on PRDEDIVISON = DIVVAL
                outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = PROID and PRLDPRID = 740) as pesinfiyat
                where  PRDESTS = 0 and PRDEKIND= 1 
                AND not exists (select * from MDE_GENEL.dbo.MB_Islemler where MB_SALID = 0 AND MB_ORDCHID = PRDEID)
                and DIVVAL = '{0}' and PRDEDATE = '{1}'
                order by 1 desc", CURID, PRDEDATE);
                SqlDataAdapter da = new SqlDataAdapter(query, sql);
                DataTable dt = new DataTable();
                da.Fill(dt);
                musterirow = dt.Rows.Count;
                Musteri.DataSource = dt;
                Musteri.DataBind();
                ScriptManager.RegisterStartupScript(this, GetType(), "hideLoading", "hideLoading();", true);
            }
        }

        protected void Kapat_Click(object sender, EventArgs e)
        {

        }

        protected void Onayla_Click(object sender, EventArgs e)
        {

        }
    }
}