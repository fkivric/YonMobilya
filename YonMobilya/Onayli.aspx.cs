using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using YonMobilya.Class;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using static YonMobilya.Class.Siniflar;

namespace YonMobilya
{
    public partial class Onayli : System.Web.UI.Page
    {
        public static string ConnectionString = "Server=192.168.4.24;Database=VDB_YON01;User Id=sa;Password=MagicUser2023!;";
        SqlConnection sql = new SqlConnection(ConnectionString);
        public static double Oran = 0;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var loginRes = (List<LoginObj>)Session["Login"];
                if (loginRes != null)
                {
                    Oran = double.Parse(loginRes[0].CURCHDISCRATE) / 100;
                    BindGrid();
                }
                else
                {
                    Response.Redirect("NewLogin.aspx");
                }
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "hideLoading", "hideLoading();", true);
            }
        }
        private void BindGrid()
        {
            var loginRes = (List<LoginObj>)Session["Login"];
            if (loginRes != null)
            {
                string magaza = "";
                if (loginRes[0].DIVVAL.Length >= 3)
                {
                    var magazalar = DbQuery.Query($"select * from MDE_GENEL.dbo.FK_fn_Split('{loginRes[0].DIVVAL.ToString()}',',')", ConnectionString);
                    for (int i = 0; i < magazalar.Rows.Count; i++)
                    {
                        if (i == 0)
                        {
                            magaza = "'" + magazalar.Rows[i][0].ToString() + "'";
                        }
                        else
                        {
                            magaza = magaza + ",'" + magazalar.Rows[i][0].ToString() + "'";
                        }
                    }
                }
                else
                {
                    magaza = loginRes[0].DIVVAL.ToString();
                }
                string q = "";
                if (loginRes[0].SOCODE == "TT-69")
                {
                    q = String.Format(@"	select distinct 
                    STRING_AGG(CDRSALID, ',') as CDRSALID, CDRCURID,CURVAL,CURNAME,ORDDATE,sum(ORDCHBALANCEQUAN) as ORDCHBALANCEQUAN,sum(PRLPRICE) as PRLPRICE,CURCHCOUNTY,ADRESS,DIVNAME,TELEFON  from (
	                select PRDEID as CDRSALID, '0' as CDRCURID, ISTEYEN.DIVVAL as CURVAL, ISTEYEN.DIVNAME as CURNAME, MB_PlanTarih as ORDDATE, sum(PRDEQUAN) as ORDCHBALANCEQUAN, sum(PRLPRICE) as PRLPRICE, ISTEYEN.DIVADR2 as CURCHCOUNTY, 
                    ISTEYEN.DIVADR1 as ADRESS, TESLIM.DIVNAME, ISTEYEN.DIVPHN1 as TELEFON
                    FROM MDE_GENEL.dbo.MB_Islemler
                    left outer join PRODEMAND on PRDEID = MB_ORDCHID
                    left outer join DIVISON ISTEYEN on ISTEYEN.DIVVAL = PRDEDIVISON
                    left outer join DEFSTORAGE on DSTORID = PRDEDSTORIDOUT
                    left outer join DIVISON TESLIM WITH(NOLOCK) ON TESLIM.DIVVAL = DSTORDIVISON
                    left outer join SOCIAL on SOCODE = 'TT-' + MB_Ekleyen
                    outer apply(select PRLPRICE from PRICELIST prl where prl.PRLPROID = PRDEPROID and PRLDPRID = 740) as pesinfiyat
                    where MB_Tamamlandi = 0 and MB_SUPCURVAL = '{0}'  
                    and PRDEKIND = 1 and PRDESTS = 0
                    group by PRDEID, ISTEYEN.DIVVAL, ISTEYEN.DIVNAME, MB_PlanTarih, ISTEYEN.DIVADR2, ISTEYEN.DIVADR1, TESLIM.DIVNAME, ISTEYEN.DIVPHN1
                    ) sonnet
					group by CDRCURID,CURVAL,CURNAME,ORDDATE,CURCHCOUNTY,ADRESS,DIVNAME,TELEFON
					order by ORDDATE, CURCHCOUNTY, CURNAME;", loginRes[0].CURVAL);
                }
                else
                {

                    q = String.Format(@"select CDRSALID,CDRCURID,CURVAL,CURNAME,MB_PlanTarih as ORDDATE,sum(ORDCHQUAN) as ORDCHBALANCEQUAN,Convert(numeric(18,2),sum(ORDCHQUAN*PRLPRICE)) as PRLPRICE,CURCHCOUNTY,CURCHADR1 + CURCHADR2 as ADRESS
                    ,TESLIM.DIVNAME,CURCHGSM1 as TELEFON
                    FROM MDE_GENEL.dbo.MB_Islemler WITH (NOLOCK)
                    inner join CUSDELIVER t WITH (NOLOCK) on MB_SALID = t.CDRSALID and t.CDRORDCHID = MB_ORDCHID
                    left outer join CURRENTS WITH (NOLOCK) on CURID = t.CDRCURID
                    left outer join CURRENTSCHILD WITH (NOLOCK) on CURCHID = CURID
                    left outer join ORDERS WITH (NOLOCK) on ORDSALID = t.CDRSALID
                    left outer join ORDERSCHILD WITH (NOLOCK) on ORDCHORDID = ORDID and ORDCHID = MB_ORDCHID and CDRORDCHID = ORDCHID
                    left outer join DEFSTORAGE WITH (NOLOCK) ON CDRSTORID = DSTORID
                    left outer join DIVISON TESLIM WITH (NOLOCK) ON TESLIM.DIVVAL = DSTORVAL AND ORDCHCOMPANY = TESLIM.DIVCOMPANY
                    left outer join SOCIAL on SOCODE = 'TT-' + MB_Ekleyen
                    outer apply (select PRLPRICE from PRICELIST prl WITH (NOLOCK) where prl.PRLPROID = ORDCHPROID and PRLDPRID = 740) as pesinfiyat
                    where MB_Tamamlandi = 0 and MB_SUPCURVAL = '{0}' AND CDRSHIPVAL = 'ANTMOB' and CDRBASECANID is NULL
                    and SOCODE = '{1}'
				    and not exists (select top 1 * from CUSDELIVER i WITH (NOLOCK) where i.CDRBASECANID = t.CDRID and MB_SALID = i.CDRSALID and i.CDRORDCHID = MB_ORDCHID)
                    group by CDRSALID,CDRCURID,CURVAL,CURNAME,MB_PlanTarih,CURCHCOUNTY,CURCHADR1 + CURCHADR2,TESLIM.DIVNAME,CURCHGSM1
                    union
                    select PRDEID as CDRSALID,'' as CDRCURID,ISTEYEN.DIVVAL as CURVAL,ISTEYEN.DIVNAME as CURNAME,MB_PlanTarih as ORDDATE,sum(PRDEQUAN) as ORDCHBALANCEQUAN,sum(PRLPRICE) as PRLPRICE,ISTEYEN.DIVADR2 as CURCHCOUNTY,ISTEYEN.DIVADR1 as ADRESS,TESLIM.DIVNAME
				    ,ISTEYEN.DIVPHN1 as TELEFON
                    FROM MDE_GENEL.dbo.MB_Islemler
				    left outer join PRODEMAND on PRDEID = MB_ORDCHID
				    left outer join DIVISON ISTEYEN on ISTEYEN .DIVVAL = PRDEDIVISON
				    left outer join DEFSTORAGE on DSTORID = PRDEDSTORIDOUT
                    left outer join DIVISON TESLIM WITH (NOLOCK) ON TESLIM.DIVVAL = DSTORDIVISON
                    left outer join SOCIAL on SOCODE = 'TT-' + MB_Ekleyen
                    outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = PRDEPROID and PRLDPRID = 740) as pesinfiyat
				    where MB_Tamamlandi = 0 and MB_SUPCURVAL = '{0}' and PRDEKIND= 1 and PRDESTS = 0
                    and SOCODE = '{1}'
				    group by PRDEID,ISTEYEN.DIVVAL,ISTEYEN.DIVNAME,MB_PlanTarih,ISTEYEN.DIVADR2,ISTEYEN.DIVADR1,TESLIM.DIVNAME,ISTEYEN.DIVPHN1
                    order by ORDDATE,CURCHCOUNTY,CURNAME", loginRes[0].CURVAL, loginRes[0].SOCODE);
                }
                var dt = DbQuery.Query(q, ConnectionString);
                GridView1.DataSource = dt;
                GridView1.DataBind();
                if (dt != null)
                {
                    //toplamadet.InnerText = dt.Rows.Count.ToString();
                    //double ciro = 0;
                    //for (int i = 0; i < dt.Rows.Count; i++)
                    //{
                    //    ciro = ciro + double.Parse(dt.Rows[0]["PRLPRICE"].ToString());
                    //}
                    //toplamciro.InnerText = ciro.ToString("C", new CultureInfo("tr-TR"));
                    //hakedis.InnerText = (ciro * 0.08).ToString("C", new CultureInfo("tr-TR"));
                }
                else
                {
                    //toplamadet.InnerText = "0";
                    //toplamciro.InnerText = "0";
                    //hakedis.InnerText = "0";
                }
            }
        }
        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridView1.PageIndex = e.NewPageIndex;
            BindGrid(); // Sayfa değiştiğinde verileri yeniden bağlayın
        }

        protected void GridView1_RowCreated(object sender, GridViewRowEventArgs e)
        {
            var ss = e.Row.RowType;
            if (e.Row.Cells.Count > 1)
            {
                e.Row.Cells[1].Visible = false;
                e.Row.Cells[2].Visible = false;
                e.Row.Cells[3].Visible = false;
            }
        }

        protected void GridView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session.Add("DIVNAME", GridView1.SelectedRow.Cells[6].Text);
            var curid = GridView1.SelectedRow.Cells[2].Text;
            var salid = GridView1.SelectedRow.Cells[1].Text;
            //String URL = string.Format("SatisDetay.aspx?id=" + Oid);
            //OpenNewWindow(this, URL, "1");
            //WebMsgBox.Show("Döküm İşlemlerini Tamamlayınız.....!");
            Response.Redirect("Montaj.aspx?curid=" + curid + "&salid=" + salid);
        }
    }
}