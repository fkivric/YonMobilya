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
                    TeslimatListesi();
                    BekleyenListesi();
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
            AND not exists (select * from MDE_GENEL.dbo.MB_Islemler where MB_ORDCHID = PRDEID)
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
                ciro = ciro + double.Parse(dt.Rows[i]["TUTAR"].ToString());
            }
            toplamciro.InnerText = ciro.ToString("C", new CultureInfo("tr-TR"));
            hakedis.InnerText = (ciro * 0.08).ToString("C", new CultureInfo("tr-TR"));

            //Page.ClientScript.RegisterStartupScript(
            //    this.GetType(),
            //    "HideSpinner",
            //    "window.onload = function() { document.getElementById('loadingSpinner').style.display = 'none'; };",
            //    true
            //);

        }
        void BekleyenListesi()
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
            string q = String.Format(@"select MB_PlanTarih,outd.DIVVAL,outd.DIVNAME as ISTEYEN,ind.DIVNAME as VEREN,sum(PRDEQUAN) as ADET,sum(PRLPRICE) as TUTAR,MB_Ekleyen
            from PRODEMAND 
            left outer join PRODUCTS on PROID = PRDEPROID
            left outer join DEFSTORAGE outs on outs.DSTORID = PRDEDSTORIDOUT
            left outer join DEFSTORAGE ins on ins.DSTORID = PRDEDSTORIDIN
            left outer join DIVISON outd on outd.DIVVAL = ins.DSTORDIVISON
            left outer join DIVISON ind on ind.DIVVAL = outs.DSTORDIVISON
			inner join MDE_GENEL.dbo.MB_Islemler on MB_ORDCHID = PRDEID
            outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = PROID and PRLDPRID = 740) as pesinfiyat
            where PRDESTS = 0 and PRDEKIND= 1
            AND PRDEDIVISON in ({0})
            group by outd.DIVVAL,outd.DIVNAME,MB_PlanTarih,ind.DIVNAME,MB_Ekleyen", magaza);
            SqlDataAdapter da = new SqlDataAdapter(q, ConnectionString);
            DataTable dt = new DataTable();
            da.Fill(dt);
            GridView2.DataSource = dt;
            GridView2.DataBind();
            tamamlananadet.InnerText = dt.Rows.Count.ToString();
            double ciro = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                ciro = ciro + double.Parse(dt.Rows[i]["TUTAR"].ToString());
            }
            tamamalananciro.InnerText = ciro.ToString("C", new CultureInfo("tr-TR"));
            tamamlananhakedis.InnerText = (ciro * 0.08).ToString("C", new CultureInfo("tr-TR"));

            //Page.ClientScript.RegisterStartupScript(
            //    this.GetType(),
            //    "HideSpinner",
            //    "window.onload = function() { document.getElementById('loadingSpinner').style.display = 'none'; };",
            //    true
            //);

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
            var loginRes = (List<LoginObj>)Session["Login"];
            ScriptManager.RegisterStartupScript(this, GetType(), "showLoading", "showLoading();", true);
            if (e.CommandName != "Page")
            {
                // Butonun hangi satırda olduğunu belirleyin
                int index = Convert.ToInt32(e.CommandArgument);
                GridViewRow row = GridView1.Rows[index];

                CURVAL = GridView1.Rows[index].Cells[2].Text;
                var PRDEDATE = Convert.ToDateTime(GridView1.Rows[index].Cells[1].Text).ToString("yyyy-MM-dd");
                var CURNAME = GridView1.Rows[index].Cells[3].Text;
                //var CURNAME = System.Web.HttpUtility.HtmlDecode(GridView1.Rows[index].Cells[5].Text);
                // SQL Sorgusu ile veriyi al
                var SALID = GridView1.Rows[index].Cells[1].Text;

                string w = String.Format(@"select DCITYNAME as CURCHCITY,DIVADR2 as CURCHCOUNTY,upper(replace(replace(DIVADR1,DIVNAME,''),'()','')) as CURCHADR from DIVISON 
                left outer join DEFCITY on DCITYVAL = DIVCITY
                where DIVVAL = '{0}'", CURVAL);
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
                    $('#customerCURNAME').val('{CURNAME}');
                    $('#customerCURVAL').val('{CURVAL}');
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
                NewOrOld.InnerText = "1";
                customerDATE.Value = DateTime.Now.ToString("yyyy-MM-dd");
                string query = String.Format(@"select PRDEID,PROID,PROVAL,PRONAME,PRDEQUAN,PRLPRICE,
                0 as isSelected from PRODEMAND 
                left outer join PRODUCTS on PROID = PRDEPROID
                left outer join DIVISON on PRDEDIVISON = DIVVAL
                outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = PROID and PRLDPRID = 740) as pesinfiyat
                where  PRDESTS = 0 and PRDEKIND= 1 
                AND not exists (select * from MDE_GENEL.dbo.MB_Islemler where MB_SALID = 0 AND MB_ORDCHID = PRDEID)
                and DIVVAL = '{0}' and PRDEDATE = '{1}'
                order by 1 desc", CURVAL, PRDEDATE);
                SqlDataAdapter da = new SqlDataAdapter(query, sql);
                DataTable dt = new DataTable();
                da.Fill(dt);
                musterirow = dt.Rows.Count;
                Musteri.DataSource = dt;
                Musteri.DataBind(); 
                
                Montajci.DataSource = null;
                Montajci.Items.Clear(); // Eklenen bu satır, mevcut öğeleri temizler
                string q = string.Format(@"select 0 as OFFCURID, 'Genel Kurulumcu...' as OFFCURNAME
			    union
                select OFFCURID,OFFCURNAME from OFFICALCUR
                left outer join CURRENTS on OFFCURCURID = CURID
			    left outer join SOCIAL on SOCURID = CURID  and 'TT-'+Cast(OFFCURID as varchar(20)) = SOCODE
			    where CURVAL = '{0}' and CURSTS = 1 and OFFCURPOSITION = 'MONTAJCI'", loginRes[0].CURVAL);
                var dt2 = DbQuery.Query(q, ConnectionString);
                if (dt2 != null && dt.Rows.Count > 0) // Sorgudan dönen veri kontrolü
                {
                    Montajci.DataValueField = "OFFCURID";
                    Montajci.DataTextField = "OFFCURNAME";
                    Montajci.DataSource = dt2;
                    Montajci.DataBind();
                }
                else
                {
                    Montajci.ClearSelection();
                    Montajci.DataSource = null;
                    Montajci.DataBind();
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

        protected void Onayla_Click(object sender, EventArgs e)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "showLoading", "showLoading();", true);
            var loginRes = (List<LoginObj>)Session["Login"];
            if (loginRes != null)
            {
                foreach (GridViewRow row in Musteri.Rows)
                {
                    CheckBox chkSelect = (CheckBox)row.FindControl("chkSelect");
                    if (chkSelect != null && chkSelect.Checked)
                    {
                        // Seçili olan satırı işleme kodu buraya yazılır.
                        string ORDCHID = Musteri.Rows[row.RowIndex].Cells[2].Text.ToString();
                        var CURVAL = DbQuery.GetValue($"select CURVAL from PRODEMAND left outer join MDE_GENEL.dbo.MB_Teslimatci on MB_DIVVAL = PRDEDIVISON left outer join CURRENTS on CURID = MB_CURID where PRDEID = {ORDCHID}");
                        if (CURVAL != "SIFIR")
                        {
                            var SALID = "0";
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
                            if (NewOrOld.InnerText == "1")
                            {
                                Dictionary<string, string> val = new Dictionary<string, string>();
                                val.Add("@MB_CURVAL", CURVAL);
                                val.Add("@MB_SALID", SALID);
                                val.Add("@MB_ORDCHID", ORDCHID);
                                val.Add("@MB_PROID", PROID);
                                val.Add("@MB_SellAmount", sellAmount.ToString());
                                val.Add("@MB_PlanTarih", PlanTarih);
                                val.Add("@MB_Ekleyen", Montajci.SelectedValue);
                                val.Add("@ReturnDesc", "");
                                var sonuc = DbQuery.Insert2("MB_Islemler_New", val);
                            }
                            else
                            {

                            }
                        }
                    }
                    else
                    {
                        WebMsgBox.Show(row.RowIndex.ToString() + " Seçilmediğinden eklenmedi");
                    }
                }
            }
            else
            {
                Response.Redirect("NewLogin.aspx");
            }
            TeslimatListesi();
        }

        protected void Musteri_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.Cells.Count > 1)
            {
                e.Row.Cells[1].Visible = false;
                e.Row.Cells[2].Visible = false;
                e.Row.Cells[3].Visible = false;
            }
        }

        protected void GridView2_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            //Yeni sayfa numarasını ayarla
            GridView1.PageIndex = e.NewPageIndex;

            // GridView'i yeniden bağla
            BekleyenListesi();
        }

        protected void GridView2_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.Cells.Count > 1)
            {
                e.Row.Cells[2].Visible = false;
                e.Row.Cells[7].Visible = false;
            }
        }

        protected void GridView2_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            var loginRes = (List<LoginObj>)Session["Login"];
            if (e.CommandName != "Page")
            {
                // Butonun hangi satırda olduğunu belirleyin
                int index = Convert.ToInt32(e.CommandArgument);
                GridViewRow row = GridView2.Rows[index];

                CURVAL = GridView2.Rows[index].Cells[2].Text;
                var PRDEDATE = Convert.ToDateTime(GridView2.Rows[index].Cells[1].Text).ToString("yyyy-MM-dd");
                var CURNAME = GridView2.Rows[index].Cells[3].Text;
                var montajci = GridView2.Rows[index].Cells[7].Text;
                //var CURNAME = System.Web.HttpUtility.HtmlDecode(GridView1.Rows[index].Cells[5].Text);
                // SQL Sorgusu ile veriyi al
                var SALID = GridView2.Rows[index].Cells[1].Text;
                var PlanDate = GridView2.Rows[index].Cells[1].Text;

                string w = String.Format(@"select DCITYNAME as CURCHCITY,DIVADR2 as CURCHCOUNTY,upper(replace(replace(DIVADR1,DIVNAME,''),'()','')) as CURCHADR from DIVISON 
                left outer join DEFCITY on DCITYVAL = DIVCITY
                where DIVVAL = '{0}'", CURVAL);
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
                    $('#customerCURNAME').val('{CURNAME}');
                    $('#customerCURVAL').val('{CURVAL}');
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
                NewOrOld.InnerText = "0";
                customerDATE.Value = Convert.ToDateTime(PlanDate).ToString("yyyy-MM-dd");
                string query = String.Format(@"select PRDEID,PROID,PROVAL,PRONAME,PRDEQUAN,PRLPRICE,
                1 as isSelected from PRODEMAND 
                left outer join PRODUCTS on PROID = PRDEPROID
                left outer join DIVISON on PRDEDIVISON = DIVVAL
                outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = PROID and PRLDPRID = 740) as pesinfiyat
                where PRDEKIND= 1 
                AND exists (select * from MDE_GENEL.dbo.MB_Islemler where MB_SALID = 0 AND MB_ORDCHID = PRDEID and MB_PlanTarih = '{1}' and MB_Tamamlandi = 0)
                and DIVVAL = '{0}'
                order by 1 desc", CURVAL, PRDEDATE);
                SqlDataAdapter da = new SqlDataAdapter(query, sql);
                DataTable dt = new DataTable();
                da.Fill(dt);
                musterirow = dt.Rows.Count;
                Musteri.DataSource = dt;
                Musteri.DataBind();

                Montajci.DataSource = null;
                Montajci.Items.Clear(); // Eklenen bu satır, mevcut öğeleri temizler
                string q = string.Format(@"
                select OFFCURID,OFFCURNAME from OFFICALCUR
                left outer join CURRENTS on OFFCURCURID = CURID
			    left outer join SOCIAL on SOCURID = CURID  and 'TT-'+Cast(OFFCURID as varchar(20)) = SOCODE
			    where CURVAL = '{0}' and CURSTS = 1 and OFFCURPOSITION = 'MONTAJCI'", loginRes[0].CURVAL);
                var dt2 = DbQuery.Query(q, ConnectionString);
                if (dt2 != null && dt.Rows.Count > 0) // Sorgudan dönen veri kontrolü
                {
                    Montajci.DataValueField = "OFFCURID";
                    Montajci.DataTextField = "OFFCURNAME";
                    Montajci.DataSource = dt2;
                    Montajci.DataBind();
                    Montajci.SelectedValue = montajci;
                }
                else
                {
                    Montajci.ClearSelection();
                    Montajci.DataSource = null;
                    Montajci.DataBind();
                }
            }

        }

        protected void Musteri_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // chkSelect checkbox kontrolünü bulun
                CheckBox chkSelect = (CheckBox)e.Row.FindControl("chkSelect");

                // isSelected değerini alın
                DataRowView rowView = (DataRowView)e.Row.DataItem;
                int isSelected = Convert.ToInt32(rowView["isSelected"]);

                // Eğer isSelected değeri 1 ise checkbox'ı seçili yapın
                if (chkSelect != null)
                {
                    chkSelect.Checked = (isSelected == 1);
                }
            }
        }
    }
}