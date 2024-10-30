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
    public partial class KullaniciProfili : System.Web.UI.Page
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
                    Bayii(loginRes[0].CURVAL);
                }
                else
                {
                    Response.Redirect("NewLogin.aspx");
                }
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            }
        }
        protected void Bayii(string curval)
        {
            var loginRes = (List<LoginObj>)Session["Login"];
            string qq = String.Format(@"
            select CURID, CURNAME
            from CURRENTS
            left outer join CURRENTSCHILD on CURID = CURCHID
            left outer join CURNOTES on CURNTCURID = CURID
            where CURSUPPLIER = 1 and CURVAL = '{0}'", loginRes[0].CURVAL);
            var id = DbQuery.Query(qq, ConnectionString);
            var CURID = id.Rows[0]["CURID"].ToString();
            string q = string.Format(@"select * from OFFICALCUR
            left outer join CURRENTS on OFFCURCURID = CURID
			left outer join CURRENTSCHILD on CURCHID = CURID
			left outer join SOCIAL on SOCURID = OFFCURCURID and SOCODE = 'TT-'+cast(OFFCURID as varchar(10))
			where OFFCURCURID = {0} and SOCODE = '{1}'", CURID,loginRes[0].SOCODE);
            var dt = DbQuery.Query(q, ConnectionString);
            if (dt.Rows.Count > 0)
            {
                ProfildeAdi.InnerText = dt.Rows[0]["OFFCURNAME"].ToString();
                EditAdi.Value = dt.Rows[0]["OFFCURNAME"].ToString();
                ProfildeSirket.InnerText = dt.Rows[0]["CURNAME"].ToString();
                EditSirket.Value = dt.Rows[0]["CURNAME"].ToString();
                ProfildeGorevi.InnerText = dt.Rows[0]["OFFCURPOSITION"].ToString();
                EditIs.Value = dt.Rows[0]["OFFCURPOSITION"].ToString();
                ProfildeMail.InnerText = dt.Rows[0]["OFFCUREMAIL"].ToString();
                EditMail.Value = dt.Rows[0]["OFFCUREMAIL"].ToString();
                ProfildeTelefon.InnerText = dt.Rows[0]["OFFCURPHONE"].ToString();
                EditTelefon.Value = dt.Rows[0]["OFFCURPHONE"].ToString();
                ProfildeAdres.InnerText = dt.Rows[0]["CURCHADR1"].ToString() + " " + dt.Rows[0]["CURCHADR2"].ToString();
                EditAdres.Value = dt.Rows[0]["CURCHADR1"].ToString() + " " + dt.Rows[0]["CURCHADR2"].ToString();
                EditUlke.Value = dt.Rows[0]["CURCHCITY"].ToString();
                EditHakkinda.Value = dt.Rows[0]["OFFCURNOTES"].ToString();
            }
        }

        protected void EditKaydet_Click(object sender, EventArgs e)
        {
            var loginRes = (List<LoginObj>)Session["Login"];
            string mailBody = $@"
                <div class='col-lg-5 col-md-7 bg-white' style='font-family: Arial, sans-serif;'>
                    <div class='p-3'>
                        <img src='assets/images/big/icon.png' alt='wrapkit' style='display: block; margin-left: auto; margin-right: auto; width: 50px;'>
                        <h2 class='mt-3 text-center' style='color: #333;'>Kayıt Bilgisi</h2>
                        <form class='mt-4'>
                            <div class='row'>
                                <div class='col-lg-12'>
                                    <div class='form-group'>                                        
                                        <label for='inputNumber' class='col - sm - 2 col - form - label'>Kullanıcı Adınız</label>
                                        <div class='form-control'style='width: 100%; padding: 10px; margin-bottom: 15px; border: 1px solid #ccc;'>TT-{loginRes[0].SOCODE}</div>
                                    </div>
                                </div>
                                <div class='col-lg-12'>
                                    <div class='form-group'>
                                        <label for='inputNumber' class='col - sm - 2 col - form - label'>Parolanız</label>
                                        <div class='form-control'style='width: 100%; padding: 10px; margin-bottom: 15px; border: 1px solid #ccc;'>{loginRes[0].SONAME}</div>
                                    </div>
                                </div>
                            </div>
                        </form>
                    </div>
                </div>";
            DbQuery.SendEmail("fatihkivric@yonavm.com.tr", "Yeni Hesap Giriş Bilgileriniz", mailBody);
        }

        protected void SettingsKaydet_Click(object sender, EventArgs e)
        {
            // Buton tıklandığında profil şifre değiştirme sekmesini aktif et
            profileoverview.Attributes["class"] = "nav-link";
            profileedit.Attributes["class"] = "nav-link";
            profilesettings.Attributes["class"] = "nav-link active";
            profilepassword.Attributes["class"] = "nav-link"; // Aktif hale getir
            string script = "activateTab('profile_settings');";
            ClientScript.RegisterStartupScript(this.GetType(), "ActivateTab", script, true);

        }

        protected void ParolaKaydet_Click(object sender, EventArgs e)
        {
            // Buton tıklandığında profil şifre değiştirme sekmesini aktif et
            profileoverview.Attributes["class"] = "nav-link";
            profileedit.Attributes["class"] = "nav-link";
            profilesettings.Attributes["class"] = "nav-link";
            profilepassword.Attributes["class"] = "nav-link active"; // Aktif hale getir
            string script = "activateTab('profile_changepassword');";
            ClientScript.RegisterStartupScript(this.GetType(), "ActivateTab", script, true);
            if (newPassword == null || string.IsNullOrEmpty(newPassword.Value) || renewPassword == null || string.IsNullOrEmpty(renewPassword.Value))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showalert", "alert('Şifre Boş Olamaz');", true);

            }
            else
            {
                if (newPassword.Value == renewPassword.Value)
                {
                    var loginRes = (List<LoginObj>)Session["Login"];
                    DbQuery.insertquery($"update SOCIAL set SOENTERKEY = '{newPassword.Value}' where SOCODE = '{loginRes[0].SOCODE}'", ConnectionString);
                }
                else
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "showalert", "alert('Şifreler Aynı Değil');", true);
                }

            }
        }
    }
}