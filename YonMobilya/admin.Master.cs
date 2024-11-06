using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Timers;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using YonMobilya.Class;
using static YonMobilya.Class.Siniflar;
using Label = System.Windows.Forms.Label;
using Timer = System.Timers.Timer;

namespace YonMobilya
{
    public partial class admin : System.Web.UI.MasterPage
    {
        private static Timer _timer;
        private static NotificationService _notificationService = new NotificationService();
        private static List<Notification> _notifications = new List<Notification>();
        private static List<LoginObj> loginRes = new List<LoginObj>();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                loginRes = (List<LoginObj>)Session["Login"];
                if (loginRes != null)
                {
                    Username.InnerText = loginRes[0].SONAME;
                    InitializeTimer();
                    LoadNotifications();
                    if (loginRes[0].CURVAL != "YON")
                    {
                        yon.Visible = false;
                        yon1.Visible = false;
                        if (loginRes[0].SOADMIN == "1")
                        {
                            adminbar.Visible = true;
                        }
                        else
                        {
                            adminbar.Visible = false;
                            NewUser.Visible = false;
                            EditUser.Visible = false;
                            MainPage.Visible = false;
                        }
                    }
                    else
                    {
                        adminbar.Visible = false;
                        firma.Visible = false;
                    }
                }
                else
                {
                    Response.Redirect("NewLogin.aspx");
                }
            }
            else
            {
                //if (_notifications.Count > 0)
                //    ShowSimplePopup();
            }
        }

        private void InitializeTimer()
        {
            _timer = new Timer(60000); // 60 seconds interval
            _timer.Elapsed += TimerElapsed;
            _timer.Start();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            LoadNotifications();
            //ShowSimplePopup();
        }

        private void LoadNotifications()
        {
            _notifications  = _notificationService.GetNotifications(loginRes[0].SOCODE);
            //int offsetY = 10; // Her bildirim popup'ını ekranda biraz yukarı yerleştirmek için
            //foreach (var notification in notifications)
            //{
            //    ShowNotificationPopup(notification, offsetY);
            //    offsetY += 110; // Her popup için dikey pozisyonu artır
            //}
            //foreach (var notification in notifications)
            //{
            //    ShowNotificationPopup(notification.Title, notification.Message);
            //}
            var json = new JavaScriptSerializer().Serialize(_notifications);
            uyariadet.InnerText = _notifications.Count.ToString();
            // Store JSON in a hidden field or another place to be accessed by JavaScript
            ScriptManager.RegisterStartupScript(this, GetType(), "LoadNotifications", $"loadNotifications({json});", true);
            
        }
        public void ShowNotificationPopup(string title, string message)
        {
            // JavaScript kodu hazırlayın
            string script = $@"
            <script type='text/javascript'>
                showPopup('{title}', '{message}');
            </script>";

            // Script'i sayfaya kaydedin ve çalıştırın
            ScriptManager.RegisterStartupScript(this, GetType(), "showPopup", script,true);
            //ClientScript.RegisterStartupScript(this.GetType(), "ShowPopupScript", script);
        }
        private void ShowNotificationPopup(Notification notification, int offsetY)
        {
            // Bildirim penceresi oluştur
            Form notificationPopup = new Form
            {
                Size = new Size(500, 100),
                StartPosition = FormStartPosition.Manual,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                BackColor = Color.DarkBlue,
                TopMost = true,
                ShowInTaskbar = false,
                Opacity = 1 // Opacity kontrolü
            };

            // Başlık Label'i
            Label titleLabel = new Label
            {
                Text = notification.Title,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor=Color.White
            };

            // Mesaj Label'i
            Label messageLabel = new Label
            {
                Text = notification.Message,
                Font = new Font("Arial", 9),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White
            };

            notificationPopup.Controls.Add(titleLabel);
            notificationPopup.Controls.Add(messageLabel);

            // Pencereyi ekranın sağ alt köşesinde göster
            notificationPopup.Location = new Point(
                Screen.PrimaryScreen.WorkingArea.Width - notificationPopup.Width - 50,
                Screen.PrimaryScreen.WorkingArea.Height - notificationPopup.Height - offsetY
            );

            // Bildirime tıklanınca yapılacak işlemi tanımla
            notificationPopup.Click += (s, e) =>
            {
                // İlgili sayfaya yönlendirme işlemi
                WebMsgBox.Show($"Bildirim tıklandı: {notification.Title}\nSayfaya yönlendirilme yapılabilir.");
                notificationPopup.Close();
            };

            // Popup formunu göster
            notificationPopup.Show();

            // Bildirim popup'ını belirli bir süre sonra kapat
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer { Interval = 50000 }; // 5 saniye
            timer.Start();
            timer.Tick += (s, e) =>
            {
                timer.Interval = 55000;
                notificationPopup.Close();
                timer.Stop();
                timer.Dispose();
            };
        }
        private void ShowSimplePopup()
        {
            Form testPopup = new Form
            {
                Size = new Size(500, 150),
                StartPosition = FormStartPosition.CenterScreen, // Ortada konumlandır
                FormBorderStyle = FormBorderStyle.FixedDialog,
                BackColor = Color.LightBlue,
                TopMost = true,
                ShowInTaskbar = false
            };
            Label titleLabel = new Label
            {
                Text = "DİKKAT",
                Font = new Font("Arial", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.TopCenter,
            };
            // Basit bir Label ekleyelim
            Label messageLabel = new Label
            {
                Text = "Yeni Bildiriminiz Var. Lütfen Bildirileri okuyup çözüme kavuşturun",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };

            testPopup.Controls.Add(titleLabel);
            testPopup.Controls.Add(messageLabel);
            testPopup.ShowDialog(); // Show yerine ShowDialog kullanarak ekran üzerinde kalmasını sağlıyoruz
        }

    }
}