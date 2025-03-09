using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using YonMobilya.Class;
using static YonMobilya.Class.Siniflar;

namespace YonMobilya
{
    public partial class NewLogin : System.Web.UI.Page
    {
        public static int girissayisi = 0;
        public static string SmsUrl = "https://restapi.ttmesaj.com/";
        public static string SmsToken = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var verison = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                var loginRes = (List<LoginObj>)Session["Login"];
                if (loginRes != null)
                {
                    Response.Redirect("MainForm.aspx");
                }
                else
                {
                }
                // Sayfa ilk defa yükleniyorsa yapılacak işlemler

            }
            else
            {
            }
        }
        protected string GetApplicationVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
        public static string ConnectionString = "Server=192.168.4.24;Database=VDB_YON01;User Id=sa;Password=MagicUser2023!;";
        protected void btnGiris_Click(object sender, EventArgs e)
        {
            string q = string.Format(@"select * from (
            select CURVAL, SOCODE as SOCODE,SOENTERKEY as SOPAS,SONAME + ' ' +SOSURNAME as SONAME, STRING_AGG(POTNOTES1,',') as DIVVAL,
			CASE WHEN OFFCURPOSITION = 'MONTAJCI' THEN 0 ELSE 1 END AS SOADMIN,CURCHDISCRATE
			from OFFICALCUR WITH (NOLOCK)
            LEFT OUTER JOIN CURRENTS WITH (NOLOCK) ON CURID = OFFCURCURID
            LEFT OUTER JOIN CURRENTSCHILD WITH (NOLOCK) ON CURID = CURCHID
            LEFT OUTER JOIN SOCIAL WITH (NOLOCK) ON SOCURID = CURID AND 'TT-' + CAST(OFFCURID AS VARCHAR(4)) =  SOCODE
            left outer join POTENCY on POTDEPART = SOCODE and POTSTS = 1 and POTNOTES1 != '00'
            where CURSTS = 1 and SODEPART  in ('027','003')
            group by CURVAL,SOCODE,SOENTERKEY,SONAME,SOSURNAME,OFFCURPOSITION,CURCHDISCRATE
            ) net
            where SOCODE = '{0}' and SOPAS = '{1}'", uname.Value, pwd.Value);
            var sonuc = DbQuery.Query(q, ConnectionString).DataTableToList<LoginObj>();
            if (sonuc != null)
            {
                Session.Add("Login", sonuc);

                string Sorgu = "select MTFTPIP as VolFtpHost,MTFTPUSER as VolFtpUser,MTFTPPASSWORD as VolFtpPass from MANAGEMENT";
                var ftp = DbQuery.Query(Sorgu, ConnectionString).DataTableToList<Ftp>();
                Session.Add("FTP", ftp);
                Session.Add("DIVNAME", "");                
                if (sonuc[0].SOADMIN == "1")
                {
                    Response.Redirect("frmAnaSayfa.aspx");
                }
                else
                {
                    Response.Redirect("Takvim.aspx");
                }
            }
            else
            {
                string w = String.Format(@"select 'YON' as CURVAL ,SOCODE as SOCODE,SOENTERKEY as SOPAS,SONAME + ' ' +SOSURNAME as SONAME,STRING_AGG(POTNOTES1,',') as DIVVAL,SOADMIN from SOCIAL
                left outer join CASHIER on CHSOCODE = SOCODE
                left outer join POTENCY on POTDEPART = SOCODE and POTSTS = 1 and POTNOTES1 != '00'
                left outer join DIVISON on DIVVAL = POTNOTES1
                LEFT OUTER JOIN CURRENTS ON CURID = SOCURID
                LEFT OUTER JOIN DEPARTMENT ON DEPVAL=SODEPART
                LEFT OUTER JOIN EMAILACCOUNT ON EMASOCODE= SOCODE  
                where SOSTS = 1 and SODEPART = '027'
                and SOCODE = '{0}' and SOENTERKEY = '{1}' and POTNOTES1 != ''
                group by CURVAL,SOCODE,SOENTERKEY,SONAME,SOSURNAME,SOADMIN", uname.Value, pwd.Value);
                var sonuc2 = DbQuery.Query(w, ConnectionString).DataTableToList<LoginObj>();
                if (sonuc2 != null)
                {
                    Session.Add("Login", sonuc);

                    string Sorgu = "select MTFTPIP as VolFtpHost,MTFTPUSER as VolFtpUser,MTFTPPASSWORD as VolFtpPass from MANAGEMENT";
                    var ftp = DbQuery.Query(Sorgu, ConnectionString).DataTableToList<Ftp>();
                    Session.Add("FTP", ftp);
                    if (sonuc[0].SOADMIN == "1")
                    {
                        Response.Redirect("frmAnaSayfa.aspx");
                    }
                    else
                    {
                        Response.Redirect("Takvim.aspx");
                    }
                }
                else
                {
                    WebMsgBox.Show("Giriş için Bilgileri Kontrol Edin");
                }
            }
        }
    }
}