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
    public partial class SettingMevcutKullanıcılar : System.Web.UI.Page
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
            string qq = String.Format(@"select '0' as CURID,'Seçiniz..' as CURNAME 
            union
            select CURID, CURNAME
            from CURRENTS
            left outer join CURRENTSCHILD on CURID = CURCHID
            left outer join CURNOTES on CURNTCURID = CURID
            where CURSUPPLIER = 1 and CURVAL = '{0}'",loginRes[0].CURVAL);
            var dt = DbQuery.Query(qq, ConnectionString);

            Bayiler.DataValueField = "CURID";
            Bayiler.DataTextField = "CURNAME";
            Bayiler.DataSource = dt;
            Bayiler.DataBind();
        }
        protected void Bayiler_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Bayiler.SelectedValue == "0")
            {
                Users.ClearSelection();
                Users.DataSource = null;
                Users.Items.Clear();
                Users.DataBind();
                OFFICURNAME.Value = "";
                OFFCUREMAIL.Value = "";
                SOENTERKEY.Value = "";
                OFFCURPHONE.Value = "";
                OFFCURGSM.Value = "";
                OFFCURNOTES.InnerText = "";
                OFFCURPOSITION.SelectedIndex = 0;
            }
            else
            {
                var loginRes = (List<LoginObj>)Session["Login"];
                Users.DataSource = null;
                Users.Items.Clear(); // Eklenen bu satır, mevcut öğeleri temizler
                string q = string.Format(@"select 0 as OFFCURID, 'Seçiniz...' as OFFCURNAME
			    union
                select OFFCURID,OFFCURNAME from OFFICALCUR
                left outer join CURRENTS on OFFCURCURID = CURID
			    left outer join SOCIAL on SOCURID = CURID  and 'TT-'+Cast(OFFCURID as varchar(20)) = SOCODE
			    where OFFCURCURID = '{0}' and CURSTS = 1
                and SOCIAL.SOCODE != '{1}'", Bayiler.SelectedValue, loginRes[0].SOCODE);
                var dt = DbQuery.Query(q, ConnectionString);
                if (dt != null && dt.Rows.Count > 0) // Sorgudan dönen veri kontrolü
                {
                    Users.DataValueField = "OFFCURID";
                    Users.DataTextField = "OFFCURNAME";
                    Users.DataSource = dt;
                    Users.DataBind();
                }
                else
                {
                    Users.ClearSelection();
                    Users.DataSource = null;
                    Users.DataBind();
                }
            }
        }

        protected void Users_SelectedIndexChanged(object sender, EventArgs e)
        {
            string q = string.Format(@"select * from OFFICALCUR
            left outer join CURRENTS on OFFCURCURID = CURID
			left outer join SOCIAL on SOCURID = OFFCURCURID
			where OFFCURID = {0}", Users.SelectedValue);
            var dt = DbQuery.Query(q, ConnectionString);
            if (dt.Rows.Count > 0)
            {
                OFFICURNAME.Value = dt.Rows[0]["OFFCURNAME"].ToString();
                OFFCUREMAIL.Value = dt.Rows[0]["OFFCUREMAIL"].ToString();
                SOENTERKEY.Value = dt.Rows[0]["SOENTERKEY"].ToString();
                OFFCURPHONE.Value = dt.Rows[0]["OFFCURPHONE"].ToString();
                OFFCURGSM.Value = dt.Rows[0]["OFFCURGSM"].ToString();
                OFFCURNOTES.InnerText = dt.Rows[0]["OFFCURNOTES"].ToString();
                int secim = 0;
                for (int i = 0; i < OFFCURPOSITION.Items.Count; i++)
                {
                    if (OFFCURPOSITION.Items[i].Value == dt.Rows[0]["OFFCURPOSITION"].ToString())
                    {
                        secim = i;
                    }
                }
                OFFCURPOSITION.SelectedIndex = secim;
            }
        }

        protected void Kaydet_Click(object sender, EventArgs e)
        {
            var loginRes = (List<LoginObj>)Session["Login"];
            if (loginRes == null)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showalert", "alert('Sisteme Tekrar Giriş Yapın');", true);
            }
            else
            {

                try
                {
                    int secim = 0;
                    for (int i = 0; i < OFFCURPOSITION.Items.Count; i++)
                    {
                        if (OFFCURPOSITION.Items[i].Selected == true)
                        {
                            secim = i;
                        }
                    }
                    string q = String.Format("update OFFICALCUR set OFFCURPHONE = '{1}',OFFCURGSM = '{2}', OFFCUREMAIL = '{3}', OFFCURNAME = '{4}', OFFCURPOSITION = '{5}', OFFCURNOTES = '{6}' where OFFCURID = {0}", Users.SelectedValue, OFFCURPHONE.Value, OFFCURGSM.Value, OFFCUREMAIL.Value, OFFICURNAME.Value, OFFCURPOSITION.Items[secim].Value,
                        OFFCURNOTES.InnerText);
                    DbQuery.insertquery($"update SOCIAL set SOENTERKEY = '{SOENTERKEY.Value}' where SOCODE = 'TT-{Users.SelectedValue}'", ConnectionString);
                    DbQuery.insertquery(q, ConnectionString);
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
                                            <div class='form-control'style='width: 100%; padding: 10px; margin-bottom: 15px; border: 1px solid #ccc;'>TT-{Users.SelectedValue}</div>
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
                    DbQuery.SendEmail(OFFCUREMAIL.Value, "Şifreniz Güncellenmiştir", mailBody);
                    Response.Redirect(Request.RawUrl);
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}