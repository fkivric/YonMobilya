using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using static YonMobilya.Class.Siniflar;
using Timer = System.Timers.Timer;

namespace YonMobilya
{
    public partial class admin : System.Web.UI.MasterPage
    {
        private static Timer _timer;
        private static NotificationService _notificationService = new NotificationService();
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
        }

        private void LoadNotifications()
        {
            var notifications = _notificationService.GetNotifications(loginRes[0].SOCODE);
            var json = new JavaScriptSerializer().Serialize(notifications);
            uyariadet.InnerText = notifications.Count.ToString();
            // Store JSON in a hidden field or another place to be accessed by JavaScript
            ScriptManager.RegisterStartupScript(this, GetType(), "LoadNotifications", $"loadNotifications({json});", true);
        }
    }
}