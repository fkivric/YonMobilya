using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;

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
                    var query = $"SELECT MB_Baslik, MB_Bildiri, MB_Zaman FROM MDE_GENEL.dbo.MB_Notification where MB_Userid = '{_id}'";
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
                                    Icon = "settings"
                                });
                            }
                        }
                    }
                }

                return notifications;
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
        }
    }
}