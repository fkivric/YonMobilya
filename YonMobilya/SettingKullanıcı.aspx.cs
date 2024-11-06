using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using YonMobilya.Class;
using static YonMobilya.Class.Siniflar;

namespace YonMobilya
{
    public partial class SettingKullanıcı : System.Web.UI.Page
    {
        public static string ConnectionString = "Server=192.168.4.24;Database=VDB_YON01;User Id=sa;Password=MagicUser2023!;";
        SqlConnection sql = new SqlConnection(ConnectionString);
        HttpClient httpClient = new HttpClient();

        public static string SmsUrl = "https://restapi.ttmesaj.com/";
        public static string SmsToken = "";
        public static HashSet<string> userphone = new HashSet<string>();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var loginRes = (List<LoginObj>)Session["Login"];
                if (loginRes != null)
                {
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
                    var dt = DbQuery.Query(String.Format(@"select OFFCURGSM from OFFICALCUR
                    left outer join CURRENTS on OFFCURCURID = CURID
			        left outer join SOCIAL on SOCURID = CURID  and 'TT-'+Cast(OFFCURID as varchar(20)) = SOCODE
			        where CURVAL = '{0}' and CURSTS = 1 and SOSTS = 1", loginRes[0].CURVAL),ConnectionString);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (!userphone.Add(dt.Rows[i][0].ToString()))
                        {

                        }
                    }
                }
                else
                {
                    Response.Redirect("NewLogin.aspx");
                }
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
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
        protected void Kaydet_Click(object sender, EventArgs e)
        {
            try
            {
                string GSM = OFFCURGSM.Value.ToString();
                if (userphone.Add(GSM))
                {
                    var loginRes = (List<LoginObj>)Session["Login"];
                    int secim = 0;
                    var CURID = DbQuery.GetValue(String.Format(@"select CURID from CURRENTS where CURVAL = '{0}'", loginRes[0].CURVAL));
                    for (int i = 0; i < OFFCURPOSITION.Items.Count; i++)
                    {
                        if (OFFCURPOSITION.Items[i].Selected == true)
                        {
                            secim = i;
                        }
                    }
                    string OFFCURID = DbQuery.GetValue("update REGISTER set RGID = RGID + 1 where RGKIND = 116 select RGID from REGISTER where RGKIND = 116");
                    string OFFCALCUR = $"insert into OFFICALCUR values ({OFFCURID},{CURID},(select count(*)+1 from OFFICALCUR where OFFCURCURID = {CURID}),'{OFFICURNAME.Value + " " + OFFICURSURNAME.Value}','{OFFCUREMAIL.Value}','{OFFCURPHONE.Value}','','{OFFCURGSM.Value}','{OFFCURPOSITION.Items[secim].Value}','{OFFCURNOTE.InnerText}')";
                    DbQuery.insertquery(OFFCALCUR, ConnectionString);
                    string socialinsertq = String.Format($@"insert into SOCIAL values ('TT-{OFFCURID}','{SOENTERKEY.Value}','{OFFICURNAME.Value}','{OFFICURSURNAME.Value}','027',1,0,NULL,0,0,NULL,{CURID},0)");
                    DbQuery.insertquery(socialinsertq, ConnectionString);
                    string mailBody = $@"
                    <div class='col-lg-5 col-md-7 bg-white' style='font-family: Arial, sans-serif;'>
                        <div class='p-3'>
                            <img src='http://yonavm.xyz/assets/images/big/icon.png' alt='wrapkit' style='display: block; margin-left: auto; margin-right: auto; width: 50px;'>
                            <h2 class='mt-3 text-center' style='color: #333;'>Kayıt Bilgisi</h2>
                            <form class='mt-4'>
                                <div class='row'>
                                    <div class='col-lg-12'>
                                        <div class='form-group'>                                        
                                            <label for='inputNumber' class='col - sm - 2 col - form - label'>Kullanıcı Adınız</label>
                                            <div class='form-control'style='width: 100%; padding: 10px; margin-bottom: 15px; border: 1px solid #ccc;'>TT-{OFFCURID}</div>
                                        </div>
                                    </div>
                                    <div class='col-lg-12'>
                                        <div class='form-group'>
                                            <label for='inputNumber' class='col - sm - 2 col - form - label'>Parolanız</label>
                                            <div class='form-control'style='width: 100%; padding: 10px; margin-bottom: 15px; border: 1px solid #ccc;'>{SOENTERKEY.Value}</div>
                                        </div>
                                    </div>
                                </div>
                            </form>
                        </div>
                    </div>";
                    string Musmetin = "Sayın " + OFFICURNAME.Value+" "+  OFFICURSURNAME.Value + " Yön Avm® sistem Girş Bilileriniz " + Environment.NewLine + " Kullanıcı Adı = TT-" + OFFCURID + Environment.NewLine + "Parola = "+ SOENTERKEY.Value + " olarak belirlenmiştir.";

                    var sms = SMS(OFFCURGSM.Value.ToString(), Musmetin);
                    Response.Write(sms);
                    if (DbQuery.SendEmail(OFFCUREMAIL.Value, "Yeni Hesap Giriş Bilgileriniz", ""))
                    {
                        Response.Redirect(Request.RawUrl);
                    }
                }
            }
            catch
            {
                WebMsgBox.Show("GSM KAYITLI");
            }
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

    }
}