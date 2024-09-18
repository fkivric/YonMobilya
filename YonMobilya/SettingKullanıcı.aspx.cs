using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
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
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var loginRes = (List<LoginObj>)Session["Login"];
                if (loginRes != null)
                {
                }
                else
                {
                    Response.Redirect("NewLogin.aspx");
                }
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            }
        }

        protected void Kaydet_Click(object sender, EventArgs e)
        {
            try
            {
                var loginRes = (List<LoginObj>)Session["Login"];
                int secim = 0;
                for (int i = 0; i < OFFCURPOSITION.Items.Count; i++)
                {
                    if (OFFCURPOSITION.Items[i].Selected == true)
                    {
                        secim = i;
                    }
                }
                string q = String.Format("update OFFICALCUR set OFFCURPHONE = '{1}',OFFCURGSM = '{2}', OFFCUREMAIL = '{3}', OFFCURNAME = '{4}', OFFCURPOSITION = '{5}', OFFCURNOTES = '{6}' where OFFCURID = {0}", loginRes[0].CURVAL, OFFCURPHONE.Value, OFFCURGSM.Value, OFFCUREMAIL.Value, OFFICURNAME.Value, OFFCURPOSITION.Items[secim].Value,
                    OFFCURNOTE.InnerText);
                DbQuery.insertquery(q, ConnectionString);
                Response.Redirect(Request.RawUrl);
            }
            catch
            {

            }
        }
    }
}