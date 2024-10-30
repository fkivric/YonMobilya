using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
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
                if (DbQuery.SendEmail(OFFCUREMAIL.Value, "Yeni Hesap Giriş Bilgileriniz", ""))
                {
                    Response.Redirect(Request.RawUrl);
                }
            }
            catch
            {

            }
        }
    }
}