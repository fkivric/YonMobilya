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
    public partial class Satis : System.Web.UI.Page
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
            else
            {

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
            string q = String.Format(@"select SALID,SALCURID,CUSCUR.CURVAL,CUSCUR.CURNAME,SALDATE,sum(ORDCHBALANCEQUAN) as ORDCHBALANCEQUAN,Convert(numeric(18,2),sum(ORDCHBALANCEQUAN*PRLPRICE)) as PRLPRICE,TESLIM.DIVNAME  as TESLIMDIVNAME,SATIS.DIVNAME as SATISDIVNAME,DSHIPNAME,CURCHCOUNTY
            FROM  CUSDELIVER
            LEFT OUTER JOIN ORDERSCHILD WITH (NOLOCK) ON ORDCHID = CDRORDCHID
            LEFT OUTER JOIN SALES WITH (NOLOCK) ON SALID=CDRSALID
            LEFT OUTER JOIN ORDERS WITH (NOLOCK) ON ORDID = ORDCHORDID
            LEFT OUTER JOIN DEEDS WITH (NOLOCK) ON DEEDID=CDRDEEDID
            LEFT OUTER JOIN PRODUCTSBEHAVE B WITH (NOLOCK) ON B.PROBHDEEDID= DEEDID AND B.PROBHORDCHID=CDRORDCHID
            LEFT OUTER JOIN PRODUCTS WITH (NOLOCK) ON ORDCHPROID = PROID
            LEFT OUTER JOIN PROSUPPLIER WITH (NOLOCK) ON PROSUPPROID=PROID
            LEFT OUTER JOIN CURRENTS SUPCUR WITH (NOLOCK) ON SUPCUR.CURID=PROSUPCURID
            LEFT OUTER JOIN DEFSTORAGE WITH (NOLOCK) ON CDRSTORID = DSTORID
            LEFT OUTER JOIN DIVISON TESLIM WITH (NOLOCK) ON TESLIM.DIVVAL = DSTORVAL AND ORDCHCOMPANY = TESLIM.DIVCOMPANY
            LEFT OUTER JOIN CURRENTS CUSCUR WITH (NOLOCK) ON CUSCUR.CURID = CDRCURID 
            LEFT OUTER JOIN CURRENTSCHILD WITH (NOLOCK) ON CURCHID = CUSCUR.CURID 
            LEFT OUTER JOIN CUSIDENTITY WITH (NOLOCK) ON CUSIDCURID = CUSCUR.CURID 
            LEFT OUTER JOIN CURSHIPADR WITH (NOLOCK) ON CSAID = CDRCSAID
            LEFT OUTER JOIN PRODUCTSUNITED WITH (NOLOCK) ON PROUID = PRODUCTS.PROPROUID
            LEFT OUTER JOIN DEFSHIPMENT WITH (NOLOCK) ON DSHIPVAL = CDRSHIPVAL
            LEFT OUTER JOIN SSH WITH (NOLOCK) ON SSHCDRID = CDRID
            LEFT OUTER JOIN SSHFAULTS WITH (NOLOCK) ON SSHSFAULVAL = SFAULVAL
            LEFT OUTER JOIN SSHSTATUS WITH (NOLOCK) ON SSTID = SSH.SSHSTATUS 
            LEFT OUTER JOIN DELIVERMAP WITH (NOLOCK) ON DELIVERMAP.DLMCDRID = CDRID
            LEFT OUTER JOIN DIVISON SATIS WITH (NOLOCK) ON SATIS.DIVVAL = CDRSALEDIV AND ORDCHCOMPANY = SATIS.DIVCOMPANY
            LEFT OUTER JOIN PROPARTADR WITH (NOLOCK) ON CDRPROPADRID = PROPADRID
            LEFT OUTER JOIN SALESMEN WITH (NOLOCK) ON SMENID=ORDCHSMENID 
            LEFT OUTER JOIN DIVISONUNITED WITH (NOLOCK) ON DIVUNIVAL = TESLIM.DIVUNITE
            LEFT OUTER JOIN PRODUCTSCHILD V1 WITH (NOLOCK) ON V1.PROCHPROID= ORDCHPROID AND V1.PROCHORDCHID = CDRORDCHID AND V1.PROCHKIND = 1 
            LEFT OUTER JOIN PRODUCTSCHILD V2 WITH (NOLOCK) ON V2.PROCHID= ORDCHPROCHAINID 
            LEFT OUTER JOIN (SELECT MAX(SPDID) AS SPDID, SPDCDRID, MAX(SPDENDDATETIME) AS SPDENDDATETIME, MAX(SPDSTARTDATETIME) AS SPDSTARTDATETIME, (CASE WHEN MAX(SPDSTARTDATE) IS NOT NULL AND MAX(SPDENDDATE) IS NULL THEN 1 ELSE 0 END) AS KURULUMSTS FROM SUPDELIVER GROUP BY SPDCDRID) K ON K.SPDCDRID = CDRID  
            outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = PROID and PRLDPRID = 740) as pesinfiyat
             WHERE CUSDELIVER.CDRSTS = 1 
             AND CUSDELIVER.CDRRNDSTS = 1 
             AND CUSDELIVER.CDRKIND <> 1 
             AND CUSDELIVER.CDRSALID > 0 
             AND ORDERSCHILD.ORDCHBALANCEQUAN > 0 
             AND ORDERSCHILD.ORDCHBEYOND = 0
             AND PROUVAL = '111'
             AND SALDIVISON in ({0})
             AND DSHIPSOCODE = '{1}'
             AND not exists (select * from MDE_GENEL.dbo.MB_Islemler where MB_SALID = SALID AND MB_ORDCHID = CDRORDCHID)
			 group by SALID,SALCURID,CUSCUR.CURVAL,CUSCUR.CURNAME,SALDATE,TESLIM.DIVNAME,SATIS.DIVNAME,DSHIPNAME,CURCHCOUNTY", magaza, loginRes[0].SOCODE);
            SqlDataAdapter da = new SqlDataAdapter(q, ConnectionString);
            DataTable dt = new DataTable();
            da.Fill(dt);
            GridView1.DataSource = dt;
            GridView1.DataBind();
            toplamadet.InnerText = dt.Rows.Count.ToString();
            double ciro = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                ciro = ciro + double.Parse(dt.Rows[0]["PRLPRICE"].ToString());
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
        protected void Musteri_SelectedIndexChanged(object sender, EventArgs e)
        {
            //        var CURVAL = GridView1.SelectedRow.Cells[4].Text;
            //        string query = @"select PROVAL,PRONAME,ORDCHBALANCEQUAN,Convert(numeric(18,2),ORDCHBALANCEQUAN*PRLPRICE) as PRLPRICE
            //,TESLIM.DIVNAME
            //from CUSDELIVER
            //left outer join CURRENTS on CURID = CDRCURID
            //left outer join ORDERSCHILD on ORDCHID = CDRORDCHID
            //left outer join PRODUCTS on PROID = ORDCHPROID
            //left outer join DEFSTORAGE WITH (NOLOCK) ON CDRSTORID = DSTORID
            //left outer join DIVISON TESLIM WITH (NOLOCK) ON TESLIM.DIVVAL = DSTORVAL AND ORDCHCOMPANY = TESLIM.DIVCOMPANY
            //outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = PROID and PRLDPRID = 740) as pesinfiyat
            //where CURVAL = '@CURVAL'";

            //        using (SqlConnection conn = new SqlConnection(ConnectionString))
            //        {
            //            using (SqlCommand cmd = new SqlCommand(query, conn))
            //            {
            //                cmd.Parameters.AddWithValue("@CURVAL", CURVAL); // Id değerini buraya ekleyin

            //                conn.Open();
            //                SqlDataReader reader = cmd.ExecuteReader();

            //                if (reader.Read())
            //                {
            //                    string kod = reader["PROVAL"].ToString();
            //                    string adı = reader["PRONAME"].ToString();
            //                    string adet = reader["ORDCHBALANCEQUAN"].ToString();
            //                    string fiyat = reader["PRLPRICE"].ToString();
            //                    string depo = reader["DIVNAME"].ToString();

            //                    // Modal içeriğini güncellemek için script çalıştırın
            //                    ClientScript.RegisterStartupScript(
            //                        this.GetType(),
            //                        "ShowModal",
            //                        $"$('#inputCURVAL').val('{kod}'); $('#inputCURNAME').val('{adı}'); showModal();",
            //                        true
            //                    );
            //                }
            //            }
            //        }
        }

        private void BindGridViewData()
        {
            // SQL sorgusu ve veri kaynağınızı burada tanımlayın
            string query = String.Format(@"select PROVAL,PRONAME,ORDCHBALANCEQUAN,Convert(numeric(18,2),ORDCHBALANCEQUAN*PRLPRICE) as PRLPRICE
			 ,TESLIM.DIVNAME
			 from CUSDELIVER
			 left outer join CURRENTS on CURID = CDRCURID
			 left outer join ORDERSCHILD on ORDCHID = CDRORDCHID
			 left outer join PRODUCTS on PROID = ORDCHPROID
			 left outer join DEFSTORAGE WITH (NOLOCK) ON CDRSTORID = DSTORID
			 left outer join DIVISON TESLIM WITH (NOLOCK) ON TESLIM.DIVVAL = DSTORVAL AND ORDCHCOMPANY = TESLIM.DIVCOMPANY
			 outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = PROID and PRLDPRID = 740) as pesinfiyat
			 where CURVAL = '@CURVAL'", CURVAL);

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // GridView'a veri bağlayın
                    Musteri.DataSource = dt;
                    Musteri.DataBind();
                }
            }
        }
        public static string CURVAL = "";
        protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "showLoading", "showLoading();", true);
            if (e.CommandName != "Page")
            {
                // Butonun hangi satırda olduğunu belirleyin
                int index = Convert.ToInt32(e.CommandArgument);
                GridViewRow row = GridView1.Rows[index];

                var CURID = GridView1.Rows[index].Cells[2].Text;
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
                string query = String.Format(@"select distinct CDRSALID,ORDCHID,PROID, PROVAL,PRONAME,Convert(int,ORDCHBALANCEQUAN) as ORDCHBALANCEQUAN,Convert(numeric(18,2),ORDCHBALANCEQUAN*PRLPRICE) as PRLPRICE
			     ,TESLIM.DIVNAME
                from CUSDELIVER
                left outer join CURRENTS on CURID = CDRCURID
                left outer join ORDERSCHILD on ORDCHID = CDRORDCHID
                left outer join PRODUCTS on PROID = ORDCHPROID
                left outer join WAVEPRODUCTS on WPROID = PROID and WPROUNIQ = 4 and WPROVAL in ('DD','EE','YY')
                left outer join DEFSTORAGE WITH (NOLOCK) ON CDRSTORID = DSTORID
                left outer join DIVISON TESLIM WITH (NOLOCK) ON TESLIM.DIVVAL = DSTORVAL AND ORDCHCOMPANY = TESLIM.DIVCOMPANY
                outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = PROID and PRLDPRID = 740) as pesinfiyat
                where CDRSALID =  '{0}' and ORDCHBALANCEQUAN >= ORDCHQUAN and WPROVAL in ('DD','EE','YY')
                AND not exists (select * from MDE_GENEL.dbo.MB_Islemler where MB_SALID = CDRSALID AND MB_ORDCHID = CDRORDCHID)
                order by 1 desc", SALID);
                SqlDataAdapter da = new SqlDataAdapter(query, sql);
                DataTable dt = new DataTable();
                da.Fill(dt);
                musterirow = dt.Rows.Count;
                Musteri.DataSource = dt;
                Musteri.DataBind();
                ScriptManager.RegisterStartupScript(this, GetType(), "hideLoading", "hideLoading();", true);
            }
        }
        protected void btnCustomerInfo_Click(object sender, EventArgs e)
        {

        }

        protected void Onayla_Click(object sender, EventArgs e)
        {
            var loginRes = (List<LoginObj>)Session["Login"];
            if (loginRes != null)
            {
                foreach (GridViewRow row in Musteri.Rows)
                {
                    CheckBox chkSelect = (CheckBox)row.FindControl("chkSelect");
                    if (chkSelect != null && chkSelect.Checked)
                    {
                        // Seçili olan satırı işleme kodu buraya yazılır.
                        string SALID = Musteri.Rows[row.RowIndex].Cells[1].Text.ToString();
                        var CURID = DbQuery.GetValue($"select CURVAL from SALES left outer join MDE_GENEL.dbo.MB_Teslimatci on MB_DIVVAL = SALDIVISON left outer join CURRENTS on CURID = MB_CURID where SALID = {SALID}");
                        if (CURID != "SIFIR")
                        {
                                var ORDCHID = Musteri.Rows[row.RowIndex].Cells[2].Text;
                                var PROID = Musteri.Rows[row.RowIndex].Cells[3].Text;
                                string cellValue = Musteri.Rows[row.RowIndex].Cells[7].Text;

                                // 'TL' para birimi işaretini çıkar
                                string cleanedValue = cellValue.Replace(" TL", "").Replace(",", "");

                                // Yerel ayar kullanarak string'i double'a dönüştür
                                double sellAmount;
                                bool isParsed = double.TryParse(cleanedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out sellAmount);
                                if (double.TryParse(cleanedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out sellAmount))
                                {
                                    var SellAmount = sellAmount;
                                }
                                else
                                {
                                    // Dönüştürme başarısız
                                    Console.WriteLine("Değeri double'a dönüştürme başarısız.");
                                }
                                var PlanTarih = customerDATE.Value.ToString();
                                Dictionary<string, string> val = new Dictionary<string, string>();
                                val.Add("@MB_CURVAL", CURID);
                                val.Add("@MB_SALID", SALID);
                                val.Add("@MB_ORDCHID", ORDCHID);
                                val.Add("@MB_PROID", PROID);
                                val.Add("@MB_SellAmount", sellAmount.ToString());
                                val.Add("@MB_PlanTarih", PlanTarih);
                                val.Add("@MB_Ekleyen", loginRes[0].SOCODE);
                                val.Add("@ReturnDesc", "");
                                var sonuc = DbQuery.Insert2("MB_Islemler_New", val);
                            
                        }
                        else
                        {
                            WebMsgBox.Show("Mağaza Teslimatcısı Tanımlanmamış");
                        }
                    }
                }

                //var SALID = Musteri.Rows[0].Cells[0].Text;
                //var CURID = DbQuery.GetValue($"select CURVAL from SALES left outer join MDE_GENEL.dbo.MB_Teslimatci on MB_DIVVAL = SALDIVISON left outer join CURRENTS on CURID = MB_CURID where SALID = {SALID}");
                
            }
            else
            {
                Response.Redirect("NewLogin.aspx");
            }
            TeslimatListesi();

        }

        protected void GridView1_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.Cells.Count > 1)
            {
                e.Row.Cells[1].Visible = false;
                e.Row.Cells[2].Visible = false;
                e.Row.Cells[3].Visible = false;
            }
        }
        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Footer)
            {
                //// Toplamları hesapla
                //int totalAdet = 0;
                //for (int i = 0; i < GridView1.Rows.Count; i++)
                //{
                //    GridViewRow row = GridView1.Rows[i];
                //    var ss = row.Cells[4].Text;
                //    totalAdet += int.Parse(row.Cells[5].Text.ToString());
                //}
                //foreach (GridViewRow row in grid.Rows)
                //{
                //    totalAdet += Convert.ToInt32(DataBinder.Eval(row.DataItem, "adet"));
                //}

                // Toplamı Footer'daki Label'a ata
                //Label lblTotalAdet = (Label)e.Row.FindControl("lblTotalAdet");
                //lblTotalAdet.Text = totalAdet.ToString();
                //if (lblTotalAdet.Text == "0")
                //{
                //}
            }
        }

        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            //Yeni sayfa numarasını ayarla
            GridView1.PageIndex = e.NewPageIndex;

            // GridView'i yeniden bağla
            TeslimatListesi();
        }
        private static int musterirow = 0;
        protected void Musteri_RowCreated(object sender, GridViewRowEventArgs e)
        {
            e.Row.Cells[1].Visible = false;
            e.Row.Cells[2].Visible = false;
            e.Row.Cells[3].Visible = false;
            e.Row.Cells[4].Visible = false;
        }
        protected void chkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chkSelectAll = (CheckBox)Musteri.HeaderRow.FindControl("chkSelectAll");
            foreach (GridViewRow row in Musteri.Rows)
            {
                CheckBox chkSelect = (CheckBox)row.FindControl("chkSelect");
                if (chkSelect != null)
                {
                    chkSelect.Checked = chkSelectAll.Checked;
                }
            }
        }

        protected void Kapat_Click(object sender, EventArgs e)
        {
            ClientScript.RegisterStartupScript(
                this.GetType(),
                "HideModal",
                "HideModal();",
                false
            );
        }
    }
}