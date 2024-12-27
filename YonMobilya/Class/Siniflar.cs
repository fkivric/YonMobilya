using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Windows.Forms;

namespace YonMobilya.Class
{
    public class Siniflar
    {
        public class Notification
        {
            public string Title { get; set; }
            public string Message { get; set; }
            public string Time { get; set; }
            public string Icon { get; set; }
            public string SALID { get; set; }
            public string CURID { get; set; }
        }
        public class NotificationService
        {
            private readonly string _connectionString = "Server=192.168.4.24;Database=VDB_YON01;User Id=sa;Password=MagicUser2023!;";

            public List<Notification> GetNotifications(string _id)
            {
                var notifications = new List<Notification>();

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = $"SELECT MB_Baslik, MB_Bildiri, MB_Zaman, MB_SALID, MB_CURID FROM MDE_GENEL.dbo.MB_Notification where MB_Userid = '{_id}'";
                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                notifications.Add(new Notification
                                {
                                    Title = reader["MB_Baslik"].ToString(),
                                    Message = reader["MB_Bildiri"].ToString(),
                                    Time = reader["MB_Zaman"].ToString(),
                                    SALID = reader["MB_SALID"].ToString(),
                                    CURID = reader["MB_CURID"].ToString(),
                                    Icon = "settings"
                                });
                            }
                        }
                    }
                }

                return notifications;
            }
        }
        public static string SmsUrl = "https://restapi.ttmesaj.com/";
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
        public class LoginObj
        {
            public string CURVAL { get; set; }
            public string SOCODE { get; set; }
            public string SOPAS { get; set; }
            public string SONAME { get; set; }
            public string DIVVAL { get; set; }
            public string SOADMIN { get; set; }
        }

        public class SmstokenObj
        {
            public string SmsUrl { get; set; }
            public string SmsUser { get; set; }
            public string SmsPassword { get; set; }
            public string TokenSMS { get; set; }
        }
        public class SmsSonuc
        {
            public string Sonuc { get; set; }
            public string Kontor { get; set; }
            public string Message { get; set; }

        }
        public class SmsGiden
        {
            public string SOCODE { get; set; }
            public string Sonuc { get; set; }

        }
        public class Ftp
        {
            public string VolFtpHost { get; set; }
            public string VolFtpUser { get; set; }
            public string VolFtpPass { get; set; }
        }

        public class Envanterlistesi
        {
            public string PROID { get; set; }
            public string PROVAL { get; set; }
            public string PRONAME { get; set; }
            public string adet { get; set; }
        }
        public class Islemler
        {
            public string ID { get; set; }
            public string CURNAME { get; set; }
            public int CURID { get; set; }
            public bool COMPLATE { get; set; }
        }
    }
}