using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using YonMobilya.Class;
using static YonMobilya.Class.Siniflar;

namespace YonMobilya
{
    public partial class Onayli : System.Web.UI.Page
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
                    ScriptManager.RegisterStartupScript(this, GetType(), "showLoading", "showLoading();", true);
                    BindGrid();
                    ScriptManager.RegisterStartupScript(this, GetType(), "hideLoading", "hideLoading();", true);
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
                string q = String.Format(@"select CDRSALID,CDRCURID,CURVAL,CURNAME,ORDDATE,sum(ORDCHBALANCEQUAN) as ORDCHBALANCEQUAN,Convert(numeric(18,2),sum(ORDCHBALANCEQUAN*PRLPRICE)) as PRLPRICE,CURCHCOUNTY
                FROM MDE_GENEL.dbo.MB_Islemler 
                inner join CUSDELIVER on MB_SALID = CDRSALID and CDRORDCHID = MB_ORDCHID
                left outer join CURRENTS on CURID = CDRCURID
                left outer join CURRENTSCHILD on CURCHID = CURID
                left outer join ORDERS on ORDSALID = CDRSALID
                left outer join ORDERSCHILD on ORDCHORDID = ORDID and ORDCHID = MB_ORDCHID and CDRORDCHID = ORDCHID
                left outer join DEFSTORAGE WITH (NOLOCK) ON CDRSTORID = DSTORID
                left outer join DIVISON TESLIM WITH (NOLOCK) ON TESLIM.DIVVAL = DSTORVAL AND ORDCHCOMPANY = TESLIM.DIVCOMPANY
                outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = ORDCHPROID and PRLDPRID = 740) as pesinfiyat
                where MB_Tamamlandi = 0 and MB_SUPCURVAL = '{0}' and ORDCHBALANCEQUAN >= ORDCHQUAN
                group by CDRSALID,CDRCURID,CURVAL,CURNAME,ORDDATE,CURCHCOUNTY
                union
                select PRDEID as CDRSALID,'' as CDRCURID,DIVVAL as CURVAL,DIVNAME as CURNAME,PRDEDATE as ORDDATE,sum(PRDEQUAN) as ORDCHBALANCEQUAN,sum(PRLPRICE) as PRLPRICE,DIVADR2 as CURCHCOUNTY
				FROM MDE_GENEL.dbo.MB_Islemler
				left outer join PRODEMAND on PRDEID = MB_ORDCHID
				left outer join DIVISON on DIVVAL = PRDEDIVISON
                outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = PRDEPROID and PRLDPRID = 740) as pesinfiyat
				where MB_Tamamlandi = 0 and MB_SUPCURVAL = 'T003387' and PRDEKIND= 1 and PRDESTS = 0
				group by PRDEID,DIVVAL,DIVNAME,PRDEDATE,DIVADR2
                order by ORDDATE,CURCHCOUNTY,CURNAME", loginRes[0].CURVAL);
                var dt = DbQuery.Query(q, ConnectionString);
                GridView1.DataSource = dt;
                GridView1.DataBind();
                if (dt != null)
                {
                    toplamadet.InnerText = dt.Rows.Count.ToString();
                    double ciro = 0;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        ciro = ciro + double.Parse(dt.Rows[0]["PRLPRICE"].ToString());
                    }
                    toplamciro.InnerText = ciro.ToString("C", new CultureInfo("tr-TR"));
                    hakedis.InnerText = (ciro * 0.08).ToString("C", new CultureInfo("tr-TR"));
                }
                else
                {
                    toplamadet.InnerText = "0";
                    toplamciro.InnerText = "0";
                    hakedis.InnerText = "0";
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
            var curid = GridView1.SelectedRow.Cells[2].Text;
            var salid = GridView1.SelectedRow.Cells[1].Text;
            //String URL = string.Format("SatisDetay.aspx?id=" + Oid);
            //OpenNewWindow(this, URL, "1");
            //WebMsgBox.Show("Döküm İşlemlerini Tamamlayınız.....!");
            Response.Redirect("Montaj.aspx?curid=" + curid + "&salid=" + salid);
        }
    }
}