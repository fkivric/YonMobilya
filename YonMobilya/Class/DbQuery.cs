using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static System.Collections.Specialized.BitVector32;
using static YonMobilya.Class.Siniflar;

namespace YonMobilya.Class
{
    public class DbQuery
    {
        public static string ConnectionString = "Server=192.168.4.24;Database=VDB_YON01;User Id=sa;Password=MagicUser2023!;";
        public static string ConnectionString2 = "Server=192.168.4.24;Database=MDE_GENEL;User Id=sa;Password=MagicUser2023!;";
        public static DataTable Query(string Sorgu, string Connection)
        {
            DataTable dt = new DataTable();
            SqlConnection conn = new SqlConnection(Connection);
            SqlDataAdapter da = new SqlDataAdapter(Sorgu, conn);
            da.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                return dt;
            }
            else { return null; }
        }
        public static DataTable Select(string spName, Dictionary<string, string> parameters)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(spName, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    foreach (var item in parameters)
                    {
                        cmd.Parameters.AddWithValue(item.Key, item.Value);
                    }

                    SqlDataAdapter adap = new SqlDataAdapter(cmd);
                    adap.Fill(dt);
                }
            }
            return dt;
        }

        public static void Insert(string spName, Dictionary<string, string> parameters)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(spName, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    foreach (var item in parameters)
                    {
                        cmd.Parameters.AddWithValue(item.Key, item.Value);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }
        public static string Insert2(string spName, Dictionary<string, string> param)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString2))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(spName, conn))
                {
                    cmd.CommandTimeout = 0;
                    if (param != null)
                    {
                        foreach (var item in param)
                        {
                            if (item.Key == "@ReturnDesc")
                            {
                                cmd.Parameters.Add("@ReturnDesc", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue(item.Key, item.Value);
                            }
                        }
                    }
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.ExecuteNonQuery();

                    return (string)cmd.Parameters["@ReturnDesc"].Value;
                }
            }

        }
        public static void insertquery(string query, string Connection)
        {
            using (SqlConnection conn = new SqlConnection(Connection))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandTimeout = 0;
                    if (query != null)
                        cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }
        public static string GetValue(string query)
        {
            using (SqlConnection sql = new SqlConnection(ConnectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, sql);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt != null)
                {
                    return dt.Rows[0][0].ToString();
                }
                else
                {
                    return null;
                }
            }
        }
        public static bool SendEmail(string recipientEmail, string subject, string body)
        {
            try
            {
                // SMTP istemcisi oluştur
                SmtpClient smtpClient = new SmtpClient("smtp.yandex.com", 587); // SMTP sunucusu ve portu

                // Gönderici e-posta bilgileri
                smtpClient.Credentials = new System.Net.NetworkCredential("merkezbilgilendirme@yonavm.com.tr", "Mb!654?123");
                smtpClient.EnableSsl = true; // SSL kullanmak için

                // E-posta mesajı oluştur
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("merkezbilgilendirme@yonavm.com.tr", "YönAVM®");
                mail.To.Add(recipientEmail); // Alıcı e-posta adresi
                mail.Subject = subject; // E-posta başlığı
                mail.Body = body; // E-posta içeriği
                mail.IsBodyHtml = true; // Eğer HTML formatında göndermek istiyorsanız bunu true yapın

                // E-postayı gönder
                smtpClient.Send(mail);

                // Başarılı olduğunda bir mesaj gösterebilirsiniz
                return true;
            }
            catch (Exception ex)
            {
                // Hata durumunda loglama veya hata mesajı gösterebilirsiniz
                return false;
            }
        }

        public static string SmsUrl = "https://restapi.ttmesaj.com/";
        public static string SmsToken = "";
        public string SMSToken()
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

                    return tokenDetails.FirstOrDefault().Value;
                }
                else
                {
                    return "";
                }
            }
        }
        //private string SmmsToken = "";
        public async Task SMS(string gsm, string Message,List<LoginObj> loginRes, List<SmstokenObj> SMSRes)
        {
            var SmmsToken = SMSRes[0].TokenSMS;
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
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}