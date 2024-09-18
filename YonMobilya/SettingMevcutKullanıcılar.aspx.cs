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
            Users.DataSource = null;
            string q = string.Format(@"select 0 as OFFCURID, 'Seçiniz...' as OFFCURNAME
			union
            select OFFCURID,OFFCURNAME from OFFICALCUR
            left outer join CURRENTS on OFFCURCURID = CURID
			left outer join SOCIAL on SOCURID = CURID
			where OFFCURCURID = '{0}' and CURSTS = 1", Bayiler.SelectedValue);
            var dt = DbQuery.Query(q, ConnectionString);


            Users.DataValueField = "OFFCURID";
            Users.DataTextField = "OFFCURNAME";
            Users.DataSource = dt;
            Users.DataBind();
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
                DbQuery.insertquery(q, ConnectionString);
                Response.Redirect(Request.RawUrl);
            }
            catch (Exception ex)
            {

            }
        }
    }
}