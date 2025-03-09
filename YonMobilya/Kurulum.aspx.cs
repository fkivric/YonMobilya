using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using YonMobilya.Class;
using static YonMobilya.Class.Siniflar;

namespace YonMobilya
{
    public partial class Kurulum : System.Web.UI.Page
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
                    sorgu1();
                    secililiste.Visible = false;
                    GorevAta.Visible = false;
                }
                else
                {
                    Response.Redirect("NewLogin.aspx");
                }
            }
            else
            {

            }
        }
        void sorgu1()
        {
            string magaza = "";
            var loginRes = (List<LoginObj>)Session["Login"];
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
            string q = String.Format(@"
            select '' as DEPOVAL, 'SEÇİNİZ...' as DEPONAME
            union
            select distinct d1.DIVVAL as DEPOVAL,d1.DIVNAME as DEPONAME from DEFSTORAGEMATCH d
            left outer JOIN DEFSTORAGE ds1 on ds1.DSTORID=DSTMTODSTORID
            left outer JOIN DEFSTORAGE ds2 on ds2.DSTORID=DSTMDSTORID
            left outer join  DIVISON d1 on d1.DIVVAL = ds1.DSTORVAL
            left outer join  DIVISON d2 on d2.DIVVAL = ds2.DSTORVAL
            left outer join MDE_GENEL.dbo.MB_Teslimatci on MB_DIVVAL = d1.DIVVAL
            where d1.DIVSALESTS = 0 and d2.DIVSTS = 1 and d1.DIVSTS=1 and d1.DIVCITY = d2.DIVCITY
            and MB_CURID != 0
            and d2.DIVVAL in ({0})
            ", magaza);
            var dt = DbQuery.Query(q, ConnectionString);
            Depo.DataSource = dt;
            Depo.DataValueField = "DEPOVAL";
            Depo.DataTextField = "DEPONAME";
            Depo.DataBind();


            Marka.DataSource = dt;
            Marka.DataValueField = "DEPOVAL";
            Marka.DataTextField = "DEPONAME";
            Marka.DataBind();


            Model.DataSource = dt;
            Model.DataValueField = "DEPOVAL";
            Model.DataTextField = "DEPONAME";
            Model.DataBind();
        }


        protected void filtreaktif_CheckedChanged(object sender, EventArgs e)
        {
            if (filtreaktif.Checked)
            {
                MarkaFiltresi.Visible = true;
                ModelFiltresi.Visible = true;
                filitremetini.InnerText = "Seçili filitredeki Ürünleri Göster";
            }
            else
            {
                MarkaFiltresi.Visible = false;
                ModelFiltresi.Visible = false;
                filitremetini.InnerText = "Seçili Depo Envanterini Göster";

            }
        }
        protected void Depo_SelectedIndexChanged(object sender, EventArgs e)
        {
            Marka.Enabled = true;
            Marka.DataSource = null;
            Marka.Items.Clear();
            string q = String.Format(@"
            select '' as CURID,'  SEÇİNİZ...' as CURNAME
            union
            select CURID,upper(CURNAME) as CURNAME from CURRENTS where CURSUPPLIER = 1 and CURSTS = 1
            and exists (select * from PROSUPPLIER 
			            left outer join PRODUCTS on PROID = PROSUPPROID
			            where PROSUPCURID = CURID and PROPROUID = 36)
            order by 2");
            var dt = DbQuery.Query(q, ConnectionString);
            Marka.DataSource = dt;
            Marka.DataValueField = "CURID";
            Marka.DataTextField = "CURNAME";
            Marka.DataBind();
            Listele.Enabled = true;
        }

        protected void Marka_SelectedIndexChanged(object sender, EventArgs e)
        {
            var CURID = Marka.SelectedValue;
            Model.Enabled = true;
            Model.DataSource = null;
            Model.Items.Clear();
            string q = String.Format(@"
            select '' as WPTREVAL,'  SEÇİNİZ...' as WPTRENAME
            union
            select distinct WPTREVAL,upper(WPTRENAME) as WPTRENAME from WAVEPROTREE
            left outer join WAVEPRODUCTS on WPROUNIQ = WPTREUNIQ and WPROVAL = WPTREVAL
            where WPROUNIQ = 5 
            and exists (select * from PROSUPPLIER 
			            left outer join PRODUCTS on PROID = PROSUPPROID
			            where PROID = WPROID and PROPROUID = 36  and PROSUPCURID = {0})
            order by 2", CURID);
            var dt = DbQuery.Query(q, ConnectionString);
            Model.DataSource = dt;
            Model.DataValueField = "WPTREVAL";
            Model.DataTextField = "WPTRENAME";
            Model.DataBind();
        }

        protected void Model_SelectedIndexChanged(object sender, EventArgs e)
        {
            Listele.Enabled = true;
        }

        protected void Listele_Click(object sender, EventArgs e)
        {
            if (Search.Text != "")
            {
                BindGridView(Search.Text);
            }
            else
            {
                BindGridView("");
            }
        }

        private static List<Envanterlistesi> envanter = new List<Envanterlistesi>();
        private static List<Envanterlistesi> secili = new List<Envanterlistesi>();

        private void BindGridView(string PRONAME)
        {
            string q = String.Format(@"
            select * from (
            select PROID,PROVAL,PRONAME,Convert(int,sum(PINVQUAN)) as adet from PROINV
            left outer join DEFSTORAGE on DSTORID = PINVSTORID
            left outer join PRODUCTS on PROID = PINVPROID
            where PINVYEAR != '' and PINVMONTH != '' and DSTORVAL = '{0}'
            and PROPROUID = 36
            group by PROID,PROVAL,PRONAME
            having sum(PINVQUAN) > 0
            ) net", Depo.SelectedValue);
            if (filtreaktif.Checked)
            {
                q = q + String.Format(@"
                    left outer join WAVEPRODUCTS on WPROID = PROID
                    left outer join WAVEPROTREE on WPROUNIQ = WPTREUNIQ and WPROVAL = WPTREVAL
                    left outer join PROSUPPLIER on PROSUPPROID = PROID
                    left outer join CURRENTS on CURID = PROSUPCURID
                    where 1 = 1 and PRONAME like '%{0}%'", PRONAME);
                if (Marka.SelectedValue != "")
                {
                    q = q + String.Format(@"
                    and PROSUPCURID = '{0}'", Marka.SelectedValue);
                }
                if (Model.SelectedValue != "")
                {
                    q = q + String.Format(@"
                    and WPTREVAL = '{0}'", Model.SelectedValue);
                }
            }
            else
            {
                q = q + String.Format(@" where PRONAME like '%{0}%'  
                order by 3", PRONAME);
            }
            envanter = DbQuery.Query(q, ConnectionString).DataTableToList<Envanterlistesi>();
            grid.DataSource = envanter;
            grid.DataBind();
        }
        protected void grid_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Select")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                if (!envanter.Any(f => f.ToString() == grid.Rows[rowIndex].Cells[1].Text.ToString()))
                {
                }
                var sec = new Envanterlistesi();
                sec.PROID = grid.Rows[rowIndex].Cells[1].Text.ToString();
                sec.PROVAL = grid.Rows[rowIndex].Cells[2].Text.ToString();
                sec.PRONAME = grid.Rows[rowIndex].Cells[3].Text.ToString();
                sec.adet = "1";
                secili.Add(sec);

                //int index = envanter.FindIndex(a => a.PROID == sec.PROID);
                //envanter.RemoveAt(index);
                //grid.DataSource = envanter;
                //grid.DataBind();
                Selected.DataSource = secili;
                Selected.DataBind();
                secililiste.Visible = true;
                GorevAta.Visible = true;
            }
        }

        protected void grid_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            //Yeni sayfa numarasını ayarla
            grid.PageIndex = e.NewPageIndex;

            // GridView'i yeniden bağla
            if (Search.Text != "")
            {
                BindGridView(Search.Text);
            }
            else
            {
                BindGridView("");
            }

            //grid.PageIndex = e.NewPageIndex;
            //grid.DataBind();
        }

        protected void grid_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.Cells.Count > 1)
            {
                var ss = e.Row.RowType;
                e.Row.Cells[1].Visible = false;
                e.Row.Cells[4].Visible = false;
            }
        }

        protected void Search_TextChanged(object sender, EventArgs e)
        {
            if (Search.Text != "")
            {
                BindGridView(Search.Text);
            }
            else
            {
                BindGridView("");
            }
        }

        protected void grid_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Footer)
            {
                // Toplamları hesapla
                int totalAdet = 0;
                for (int i = 0; i < grid.Rows.Count; i++)
                {
                    GridViewRow row = grid.Rows[i];
                    var ss = row.Cells[5].Text;
                    totalAdet += int.Parse(row.Cells[4].Text.ToString());
                }
                //foreach (GridViewRow row in grid.Rows)
                //{
                //    totalAdet += Convert.ToInt32(DataBinder.Eval(row.DataItem, "adet"));
                //}

                // Toplamı Footer'daki Label'a ata
                Label lblTotalAdet = (Label)e.Row.FindControl("lblTotalAdet");
                lblTotalAdet.Text = totalAdet.ToString();
                if (lblTotalAdet.Text == "0")
                {
                }
            }
        }

        protected void Selected_RowCreated(object sender, GridViewRowEventArgs e)
        {
            //if (e.Row.Cells.Count > 1)
            //{
            //    var ss = e.Row.RowType;
            //    e.Row.Cells[1].Visible = false;
            //    e.Row.Cells[4].Visible = false;
            //}
        }
    }
}