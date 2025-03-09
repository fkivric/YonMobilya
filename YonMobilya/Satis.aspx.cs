using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
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
        HttpClient httpClient = new HttpClient();
        public static double Oran;

        public static string SmsUrl = "https://restapi.ttmesaj.com/";
        public static string SmsToken = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var loginRes = (List<LoginObj>)Session["Login"];
                if (loginRes != null)
                {
                    Oran = double.Parse(loginRes[0].CURCHDISCRATE)/100;
                    ScriptManager.RegisterStartupScript(this, GetType(), "showLoading", "showLoading();", true);
                    StartDate.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("yyyy-MM-dd");
                    EndDate.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd");
                    TeslimatListesi(); 
                    BekleyenListesi();
                    ScriptManager.RegisterStartupScript(this, GetType(), "hideLoading", "hideLoading();", true);
                    string Token = Session["Smstoken"] as string;
                    if (Token == "" || Token == null)
                    {
                        List<SmstokenObj> smstokenObjs = new List<SmstokenObj>();
                        SMSToken();
                        Session.Add("Smstoken", SmsToken);
                        SmstokenObj obj = new SmstokenObj();
                        obj.SmsUrl = SmsUrl;
                        obj.SmsUser = "yon.kara";
                        obj.SmsPassword = "N6K9L3A55";
                        obj.TokenSMS = SmsToken;
                        smstokenObjs.Add(obj);
                        Session.Add("SMS", smstokenObjs);
                        Session.Add("Smstoken", SmsToken);
                    }
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
                string script = "$(document).ready(function () { $('[id*=btnSubmit]').click(); });";
                ClientScript.RegisterStartupScript(this.GetType(), "load", script, true);
            }
        }

        private void SMSToken()
        {
            var login = new Dictionary<string, string>
               {
                   {"grant_type", "password"},
                   {"username", "ttapiuser1"},//TT Mesaj Tarfından Size Verilen Api Kullanıcı Adı
                   {"password", "ttapiuser1123"},//TT Mesaj Tarfından Size Verilen Api Şifre
               };


            using (HttpClient httpClient = new HttpClient())
            {

                var response = httpClient.PostAsync(SmsUrl + "ttmesajToken", new FormUrlEncodedContent(login)).Result;

                if (response.IsSuccessStatusCode)
                {
                    Dictionary<string, string> tokenDetails = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Content.ReadAsStringAsync().Result);

                    SmsToken = tokenDetails.FirstOrDefault().Value;
                }
                else
                {
                    WebMsgBox.Show("Sms Token Bilgisi Okunamadı Çıkış Yapıp Tekrar Girin");
                }
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
            string q = String.Format(@"select SALID,SALCURID,CUSCUR.CURVAL,CUSCUR.CURNAME,CDRDATE2 as SALDATE,sum(ORDCHBALANCEQUAN) as ORDCHBALANCEQUAN,Convert(numeric(18,2),sum(ORDCHBALANCEQUAN*PRLPRICE)) as PRLPRICE,TESLIM.DIVNAME  as TESLIMDIVNAME,SATIS.DIVNAME as SATISDIVNAME,DSHIPNAME,CURCHCOUNTY
            FROM  CUSDELIVER T
            LEFT OUTER JOIN ORDERSCHILD WITH (NOLOCK) ON ORDCHID = T.CDRORDCHID
            LEFT OUTER JOIN SALES WITH (NOLOCK) ON SALID=T.CDRSALID
            LEFT OUTER JOIN ORDERS WITH (NOLOCK) ON ORDID = ORDCHORDID
            LEFT OUTER JOIN DEEDS WITH (NOLOCK) ON DEEDID=T.CDRDEEDID
            LEFT OUTER JOIN PRODUCTSBEHAVE B WITH (NOLOCK) ON B.PROBHDEEDID= DEEDID AND B.PROBHORDCHID=T.CDRORDCHID
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
             WHERE T.CDRSTS = 1 
             AND T.CDRRNDSTS = 1 
             AND T.CDRKIND <> 1 
             AND T.CDRSALID > 0 
             AND ORDERSCHILD.ORDCHBALANCEQUAN > 0 
             AND ORDERSCHILD.ORDCHBEYOND = 0
             AND PROUVAL in ('111','118')
			 AND CDRSHIPVAL = 'ANTMOB'
             AND SALDIVISON in ({0})
             AND not exists (select * from MDE_GENEL.dbo.MB_Islemler where MB_SALID = SALID AND MB_ORDCHID = CDRORDCHID)
			 and not exists (select top 1 * from CUSDELIVER i WITH (NOLOCK) where i.CDRBASECANID = t.CDRID and T.CDRSALID = i.CDRSALID and i.CDRORDCHID = T.CDRORDCHID)
			 group by SALID,SALCURID,CUSCUR.CURVAL,CUSCUR.CURNAME,CDRDATE2,TESLIM.DIVNAME,SATIS.DIVNAME,DSHIPNAME,CURCHCOUNTY
            order by CUSCUR.CURNAME", magaza);
            SqlDataAdapter da = new SqlDataAdapter(q, ConnectionString);
            DataTable dt = new DataTable();
            da.Fill(dt);
            GridView1.DataSource = dt;
            GridView1.DataBind();
            toplamadet.InnerText = dt.Rows.Count.ToString();
            double ciro = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                ciro = ciro + double.Parse(dt.Rows[i]["PRLPRICE"].ToString());
            }
            toplamciro.InnerText = ciro.ToString("C", new CultureInfo("tr-TR"));
            hakedis.InnerText = (ciro * Oran).ToString("C", new CultureInfo("tr-TR"));

            //Page.ClientScript.RegisterStartupScript(
            //    this.GetType(),
            //    "HideSpinner",
            //    "window.onload = function() { document.getElementById('loadingSpinner').style.display = 'none'; };",
            //true
            //);

        }
        void BekleyenListesi()
        {
            string q = $@"
            select MB_SALID, CURID,OFFCURID, CURVAL, CURNAME,OFFCURNAME, MB_PlanTarih, sum(ORDCHQUAN) as ORDCHBALANCEQUAN,Convert(numeric(18,2), sum(tutar)) as PRLPRICE from (
select distinct Convert(varchar(50),MB_SALID) as MB_SALID,MB_ORDCHID,MB_PlanTarih,MB_Ekleyen, sum(PRLPRICE) as tutar from MDE_GENEL..MB_Islemler
                outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = MB_PROID and PRLDPRID = 740) as pesinfiyat
                where MB_Tamamlandi = 0 and MB_SALID != 0 and MB_SUPCURVAL = 'T003387'
                group by MB_SALID,MB_ORDCHID,MB_PlanTarih,MB_Ekleyen) net
			inner join CUSDELIVER T on MB_SALID = CDRSALID and CDRORDCHID = MB_ORDCHID
            inner join SALES on SALID = MB_SALID
            inner join CURRENTS on CURID = SALCURID
            inner join ORDERSCHILD on ORDCHID = CDRORDCHID
            inner join OFFICALCUR on Convert(varchar(3),OFFCURID) = MB_Ekleyen
            outer apply(select PRLPRICE from PRICELIST prl where prl.PRLPROID = CDRLPROID and PRLDPRID = 740) as pesinfiyat
            where CDRSHIPVAL = 'ANTMOB'
            and MB_PlanTarih between '{StartDate.Value}' and '{EndDate.Value}'
			and not exists (select top 1 * from CUSDELIVER i WITH (NOLOCK) where i.CDRBASECANID = t.CDRID and T.CDRSALID = i.CDRSALID and i.CDRORDCHID = T.CDRORDCHID)
            group by MB_SALID,CURID,OFFCURID,CURVAL,CURNAME,OFFCURNAME,MB_PlanTarih
            order by MB_PlanTarih";
            SqlDataAdapter da = new SqlDataAdapter(q, ConnectionString);
            DataTable dt = new DataTable();
            da.Fill(dt);
            GridView2.DataSource = dt;
            GridView2.DataBind();
            tamamlananadet.InnerText = dt.Rows.Count.ToString();
            double ciro = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var tutar = double.Parse(dt.Rows[i]["PRLPRICE"].ToString());
                ciro = ciro + double.Parse(dt.Rows[i]["PRLPRICE"].ToString());
            }
            tamamalananciro.InnerText = ciro.ToString("C", new CultureInfo("tr-TR"));
            tamamlananhakedis.InnerText = (ciro * Oran).ToString("C", new CultureInfo("tr-TR"));
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
            var loginRes = (List<LoginObj>)Session["Login"];
            if (e.CommandName != "Page")
            {
                // Butonun hangi satırda olduğunu belirleyin

                int index = Convert.ToInt32(e.CommandArgument);
                GridViewRow row = GridView1.Rows[index];
                var CURID = row.Cells[2].Text;
                CURVAL = row.Cells[3].Text;
                var CURNAME = System.Web.HttpUtility.HtmlDecode(row.Cells[4].Text);
                // SQL Sorgusu ile veriyi al
                var SALID = row.Cells[1].Text;
                var DIVNAME = row.Cells[8].Text;
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
                NewOrOld.InnerText = "1";
                customerDATE.Value = DateTime.Now.ToString("yyyy-MM-dd");
                string query = String.Format(@"select distinct CDRSALID,ORDCHID,PROID, PROVAL,PRONAME,Convert(int,ORDCHBALANCEQUAN) as ORDCHBALANCEQUAN,Convert(numeric(18,2),ORDCHBALANCEQUAN*PRLPRICE) as PRLPRICE
			     , 0 as isSelected
                from CUSDELIVER
                left outer join CURRENTS on CURID = CDRCURID
                left outer join ORDERSCHILD on ORDCHID = CDRORDCHID
                left outer join PRODUCTS on PROID = ORDCHPROID
                left outer join WAVEPRODUCTS on WPROID = PROID and WPROUNIQ = 4 and WPROVAL in ('DD','EE','YY')
                left outer join DEFSTORAGE WITH (NOLOCK) ON CDRSTORID = DSTORID
                left outer join DIVISON TESLIM WITH (NOLOCK) ON TESLIM.DIVVAL = DSTORVAL AND ORDCHCOMPANY = TESLIM.DIVCOMPANY
                outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = PROID and PRLDPRID = 740) as pesinfiyat
                where CDRSALID =  '{0}' and WPROVAL in ('DD','EE','YY')
				AND CDRSHIPVAL = 'ANTMOB'
                AND TESLIM.DIVNAME = '{1}'
                AND not exists (select * from MDE_GENEL.dbo.MB_Islemler where MB_SALID = CDRSALID AND MB_ORDCHID = CDRORDCHID)
                order by 1 desc", SALID, DIVNAME);
                SqlDataAdapter da = new SqlDataAdapter(query, sql);
                DataTable dt = new DataTable();
                da.Fill(dt);
                musterirow = dt.Rows.Count;
                Musteri.DataSource = dt;
                Musteri.DataBind();
                CDRPLNNOTES.Value = DbQuery.GetValue($"select CDRPLNNOTES from CUSDELIVER where CDRSALID = {SALID}");
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
        protected void btnCustomerInfo_Click(object sender, EventArgs e)
        {

        }

        protected void Onayla_Click(object sender, EventArgs e)
        {
            StringBuilder ordChidList = new StringBuilder();
            var loginRes = (List<LoginObj>)Session["Login"];
            if (loginRes != null)
            {
                try
                {
                    var CURID = DbQuery.GetValue($"select SALCURID from SALES where SALID = {Musteri.Rows[0].Cells[1].Text.ToString()}");
                    foreach (GridViewRow row in Musteri.Rows)
                    {
                        var kayitli = DbQuery.GetValue($"select MB_ID from MDE_GENEL.dbo.MB_Islemler where MB_ORDCHID = {Musteri.Rows[row.RowIndex].Cells[2].Text}");
                        CheckBox chkSelect = (CheckBox)row.FindControl("chkSelect");
                        if (chkSelect != null && chkSelect.Checked)
                        {
                            if (kayitli != null)
                            {
                                var PlanTarih = customerDATE.Value.ToString();
                                var tutar = Musteri.Rows[row.RowIndex].Cells[4].Text;
                                DbQuery.insertquery($"update MDE_GENEL.dbo.MB_Islemler set MB_PlanTarih = '{PlanTarih}', MB_Ekleyen = '{Montajci.SelectedValue}' where MB_ID = {kayitli}", ConnectionString);
                            }
                            else
                            {

                                // Seçili olan satırı işleme kodu buraya yazılır.
                                string SALID = Musteri.Rows[row.RowIndex].Cells[1].Text.ToString();
                                var CURVAL = DbQuery.GetValue($"select CURVAL from SALES left outer join MDE_GENEL.dbo.MB_Teslimatci on MB_DIVVAL = SALDIVISON left outer join CURRENTS on CURID = MB_CURID where SALID = {SALID}");
                                if (CURVAL != "SIFIR")
                                {
                                    var ORDCHID = Musteri.Rows[row.RowIndex].Cells[2].Text;
                                    // ORDCHID'yi StringBuilder'a ekle
                                    if (ordChidList.Length > 0)
                                    {
                                        ordChidList.Append(","); // Eğer daha önce değer eklenmişse, virgül ile ayır
                                    }
                                    ordChidList.Append(ORDCHID); // ORDCHID'yi ekle

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
                                    var SMSRes = (List<SmstokenObj>)Session["SMS"];
                                    var PROVAL = Musteri.Rows[row.RowIndex].Cells[4].Text;
                                    var PRONAME = Musteri.Rows[row.RowIndex].Cells[5].Text;
                                    var CURGSM = DbQuery.GetValue($"select isnull(isnull(CURCHGSM1,CURCHGSM2),CURCHGSM3) from CURRENTSCHILD where CURCHID in (select SALCURID from SALES where SALID = {SALID})");
                                    var CURNAME = DbQuery.GetValue($"select CURNAME from CURRENTS where CURID  in (select SALCURID from SALES where SALID = {SALID})");
                                    string Musmetin = "Sayın " + CURNAME + " Yön Avm® den almış oldugunuz " + Environment.NewLine + PRONAME + " Ürünüz " + PlanTarih + Environment.NewLine + "tarihinde Telimat için planlanmıştır.";
                                    var BGMUDURGSM = DbQuery.GetValue($"select substring(DIVPHN2,3,10) as DIVPHN2 from DIVISON where DIVVAL in (select SALDIVISON from SALES where SALID = {SALID})");
                                    var MGMUDURGSM = DbQuery.GetValue($"select DIVPHN1 from DIVISON where DIVVAL in (select SALDIVISON from SALES where SALID = {SALID})");
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

                                        if (smsvar != null && smsvar.Checked)
                                        {
                                            var MUSTERİSMS = SMS(CURGSM, Musmetin);
                                        }
                                        var snc = SMS("5547414216", Musmetin);
                                        var BGMSMS = SMS(BGMUDURGSM, CURNAME + " Müşterinin " + PRONAME + " ürünü " + PlanTarih + Environment.NewLine + " tarihinde teslimat için Planlanmıştır.");
                                        var MGMSMS = SMS(MGMUDURGSM, CURNAME + " Müşterinin " + PRONAME + " ürünü " + PlanTarih + Environment.NewLine + " tarihinde teslimat için Planlanmıştır.");
                                    }
                                    else
                                    {
                                        //var kayitli = DbQuery.GetValue($"select MB_ID from MDE_GENEL.dbo.MB_Islemler where MB_ORDCHID = {ORDCHID}");
                                        DbQuery.insertquery($"update MDE_GENEL.dbo.MB_Islemler set MB_PlanTarih = '{PlanTarih}', MB_Ekleyen = '{Montajci.SelectedValue}' where MB_ID = {kayitli}", ConnectionString);
                                        var BGMSMS = SMS(BGMUDURGSM, CURNAME + " Müşterinin " + PRONAME + " ürünü " + PlanTarih + Environment.NewLine + " teslimat tarihi değişmiştir.");
                                        var MGMSMS = SMS(MGMUDURGSM, CURNAME + " Müşterinin " + PRONAME + " ürünü " + PlanTarih + Environment.NewLine + " teslimat tarihi değişmiştir.");
                                    }
                                }
                                else
                                {
                                    WebMsgBox.Show("Mağaza Teslimatcısı Tanımlanmamış");
                                }
                            }
                        }
                        else
                        {

                            DbQuery.insertquery($"delete MDE_GENEL.dbo.MB_Islemler where MB_ID =  {kayitli}", ConnectionString);

                            var ORDCHID = Musteri.Rows[row.RowIndex].Cells[2].Text;
                            string SALID = Musteri.Rows[row.RowIndex].Cells[1].Text.ToString();

                        }
                    }

                    //var SALID = Musteri.Rows[0].Cells[0].Text;
                    //var CURID = DbQuery.GetValue($"select CURVAL from SALES left outer join MDE_GENEL.dbo.MB_Teslimatci on MB_DIVVAL = SALDIVISON left outer join CURRENTS on CURID = MB_CURID where SALID = {SALID}");

                    TeslimatListesi();
                    BekleyenListesi();

                    Response.Redirect("MonajFormu.aspx?curid=" + CURID + "&salid=" + ordChidList);


                    //string url = "MonajFormu.aspx?curid=" + CURID + "&salid=" + ordChidList;
                    //string script = @"var printWindow = window.open('" + url + "', '_blank');";
                    //ClientScript.RegisterStartupScript(this.GetType(), "OpenInNewTab", script, true);

                    //string url = "MonajFormu.aspx?curid=" + CURID + "&salid=" + ordChidList;
                    //string script = @"var printWindow = window.open('" + url + "', '_blank'); " +
                    //    "printWindow.onload = function() {printWindow.print();};";
                    //ClientScript.RegisterStartupScript(this.GetType(), "OpenPrintWindow", script, true);


                    //string url = "MonajFormu.aspx?curid=" + CURID + "&salid=" + ordChidList;
                    //string script = "window.open('" + url + "', '_blank'); window.print();";
                    //ClientScript.RegisterStartupScript(this.GetType(), "OpenPrintWindow", script, true);
                }
                catch (Exception)
                {
                }
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
        private string SmmsToken = "";
        private async Task SMS(string gsm, string Message)
        {
            var SMSRes = (List<SmstokenObj>)Session["SMS"];
            var loginRes = (List<LoginObj>)Session["Login"];
            SmmsToken = SMSRes[0].TokenSMS;
            try
            {
                string description = string.Empty;

                //isNotification parametresinin doldurulması
                bool? isNotificationValue = null;


                //recipentType parametresinin doldurulması
                string recipentTypeValue = string.Empty;
                var data = new
                {
                    username = SMSRes[0].SmsUser,
                    password = SMSRes[0].SmsPassword,
                    numbers = "0" + gsm,
                    message = Message,
                    origin = "YON AVM",
                    sd = "0",
                    ed = "0",
                    isNotification = isNotificationValue,
                    recipentType = recipentTypeValue,
                    brandCode = ""
                };

                var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

                using (HttpClient httpClient = new HttpClient())
                {
                    if (string.IsNullOrEmpty(SmmsToken))
                    {
                        WebMsgBox.Show("Token Hatalı");
                    }
                    else
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", SmmsToken);
                        var httppost = await httpClient.PostAsync(SMSRes[0].SmsUrl + "api/SendSms/SendSingle", content);
                        string responseContent = await httppost.Content.ReadAsStringAsync();

                        if (httppost.IsSuccessStatusCode)
                        {
                            try
                            {
                                var response = JsonConvert.DeserializeObject<SmsSonuc>(responseContent);

                                if (response.Sonuc.Contains("*OK*")) // success
                                {
                                    description = response.Sonuc.Replace("*OK*", "");
                                    List<SmsGiden> smsSonucs = new List<SmsGiden>();
                                    SmsGiden sonuc = new SmsGiden();
                                    sonuc.SOCODE = loginRes[0].SOCODE;
                                    sonuc.Sonuc = description;
                                    smsSonucs.Add(sonuc);
                                    Session.Add("SMSGONDERIM", smsSonucs);
                                }
                                else
                                {
                                    WebMsgBox.Show("SMS gönderimi başarısız: " + response.Sonuc);
                                }
                            }
                            catch (JsonException ex)
                            {
                                WebMsgBox.Show("JSON parse hatası: " + ex.Message);
                            }
                        }
                        else
                        {
                            WebMsgBox.Show("HTTP Hatası: " + httppost.StatusCode + " - " + responseContent);
                        }
                    }
                    //else
                    //{
                    //    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", SmmsToken);
                    //    var httppost = httpClient.PostAsync(ApiUrl + "api/SendSms/SendSingle", content).Result;
                    //    var response = JsonConvert.DeserializeObject<SmsSonuc>(httppost.Content.ReadAsStringAsync().Result);

                    //    if (response.Sonuc.Contains("*OK*")) // success
                    //    {
                    //        description = response.Sonuc.Replace("*OK*", "");                            
                    //    }
                    //    else
                    //    {

                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {

            }
        }

        protected void GridView2_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            //Yeni sayfa numarasını ayarla
            GridView2.PageIndex = e.NewPageIndex;

            // GridView'i yeniden bağla
            BekleyenListesi();
            ScriptManager.RegisterStartupScript(this, GetType(), "hideLoading", "hideLoading();", true);
        }

        protected void GridView2_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.Cells.Count > 1)
            {
                e.Row.Cells[1].Visible = false;
                e.Row.Cells[2].Visible = false;
                e.Row.Cells[3].Visible = false;
                e.Row.Cells[4].Visible = false;
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

                var CURID = GridView2.Rows[index].Cells[2].Text;
                CURVAL = GridView2.Rows[index].Cells[4].Text;
                var PlanDate = GridView2.Rows[index].Cells[6].Text;
                var montajci = GridView2.Rows[index].Cells[3].Text;
                var CURNAME = System.Web.HttpUtility.HtmlDecode(GridView2.Rows[index].Cells[5].Text);
                // SQL Sorgusu ile veriyi al
                var SALID = GridView2.Rows[index].Cells[1].Text;

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
                NewOrOld.InnerText = "0";
                customerDATE.Value = Convert.ToDateTime(PlanDate).ToString("yyyy-MM-dd");
                string query = String.Format(@"select distinct SALID as CDRSALID,MB_ORDCHID as ORDCHID,PROID, PROVAL,PRONAME,Convert(int,ORDCHQUAN) as ORDCHBALANCEQUAN,Convert(numeric(18,2),ORDCHQUAN*PRLPRICE) as PRLPRICE
			     ,
                1 as isSelected
                from MDE_GENEL..MB_Islemler
                left outer join SALES on SALID = MB_SALID
                --left outer join CUSDELIVER on MB_SALID = CDRSALID and CDRORDCHID = MB_ORDCHID
                left outer join CURRENTS on CURID = SALCURID
                left outer join PRODUCTS on PROID = MB_PROID
                left outer join ORDERSCHILD on ORDCHID = MB_ORDCHID
                outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = PROID and PRLDPRID = 740) as pesinfiyat
                where SALID =  {0} and MB_Tamamlandi = 0
                order by 1 desc", SALID);
                SqlDataAdapter da = new SqlDataAdapter(query, sql);
                DataTable dt = new DataTable();
                da.Fill(dt);
                musterirow = dt.Rows.Count;
                Musteri.DataSource = dt;
                Musteri.DataBind();
                //Musteri.Columns[7].Visible = false;
                Montajci.DataSource = null;
                Montajci.Items.Clear(); // Eklenen bu satır, mevcut öğeleri temizler
                string q = string.Format(@"select 0 as OFFCURID, 'Genel Kurulumcu...' as OFFCURNAME
			    union
                select OFFCURID,OFFCURNAME from OFFICALCUR
                left outer join CURRENTS on OFFCURCURID = CURID
			    left outer join SOCIAL on SOCURID = CURID  and 'TT-'+Cast(OFFCURID as varchar(20)) = SOCODE
			    where CURVAL = '{0}' and CURSTS = 1 and OFFCURPOSITION = 'MONTAJCI'", loginRes[0].CURVAL);
                var dt2 = DbQuery.Query(q, ConnectionString);
                if (dt2 != null || dt.Rows.Count > 0) // Sorgudan dönen veri kontrolü
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

        protected void Listele_Click(object sender, EventArgs e)
        {
            BekleyenListesi();
        }
    }
}