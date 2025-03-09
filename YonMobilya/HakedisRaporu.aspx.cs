using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using YonMobilya.Class;
using static YonMobilya.Class.Siniflar;

namespace YonMobilya
{
    public partial class HakedisRaporu : System.Web.UI.Page
    {
        admin admin = new admin();
        public static string ConnectionString = "Server=192.168.4.24;Database=VDB_YON01;User Id=sa;Password=MagicUser2023!;";
        SqlConnection sql = new SqlConnection(ConnectionString);
        CultureInfo culture = new CultureInfo("tr-TR");
        static string ftpUrl = "";
        static string ftpUsername = "";
        static string ftpPassword = "";
        public static string CURID = "";
        public static string SALID = "";
        public static string ORDCHID = "";
        public static double Oran = 0;
        public static DataTable Veriler = new DataTable();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var loginRes = (List<LoginObj>)Session["Login"];
                if (loginRes != null)
                {
                    Oran = double.Parse(loginRes[0].CURCHDISCRATE) / 100;
                    StartDate.Value = new DateTime(DateTime.Now.Year,DateTime.Now.Month,1).ToString("yyyy-MM-dd");
                    EndDate.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd");
                    BindGrid(StartDate.Value, EndDate.Value);
                    AyIsmi.InnerText = DateTime.Parse(StartDate.Value).ToString("dd", new CultureInfo("tr-TR")) + "-" + DateTime.Parse(StartDate.Value).ToString("MMMM", new CultureInfo("tr-TR")) + " - " + DateTime.Parse(EndDate.Value).ToString("dd", new CultureInfo("tr-TR")) + "-" + DateTime.Parse(EndDate.Value).ToString("MMMM", new CultureInfo("tr-TR")) + " Hakediş Raporu";
                    var ftp = (List<Ftp>)Session["FTP"];
                    if (ftp != null)
                    {
                        ftpUrl = ftp[0].VolFtpHost;
                        ftpUsername = ftp[0].VolFtpUser;
                        ftpPassword = ftp[0].VolFtpPass;
                    }
                }
                else
                {
                    Response.Redirect("NewLogin.aspx");
                }
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            }
        }
        private void BindGrid(string startdate, string enddate)
        {
            //var startdate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("yyyy-MM-dd");
            //var enddate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd");
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
                string q = String.Format(@"select count(adet) as Toplamadet, isnull(sum(tutar),0) as Toplamtutar from (
                select distinct Convert(varchar(50),MB_SALID) as adet, sum(MB_SellAmount) as tutar 
                    from MDE_GENEL..MB_Islemler
                    inner join CUSDELIVER T on MB_SALID = CDRSALID and CDRORDCHID = MB_ORDCHID
                    outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = MB_PROID and PRLDPRID = 740) as pesinfiyat
                    where MB_Tamamlandi = 0 and MB_SALID != 0 and MB_SUPCURVAL = 'T003387' and CDRSHIPVAL = 'ANTMOB' 
				    and not exists (select top 1 * from CUSDELIVER i WITH (NOLOCK) where i.CDRBASECANID = t.CDRID and T.CDRSALID = i.CDRSALID and i.CDRORDCHID = T.CDRORDCHID)
                    and MB_PlanTarih between '{1}' and '{2}'
                    group by MB_SALID
                union
                select outd.DIVNAME as ISTEYEN,sum(MB_SellAmount) as TUTAR
                    from PRODEMAND 
                    left outer join PRODUCTS on PROID = PRDEPROID
                    left outer join DEFSTORAGE outs on outs.DSTORID = PRDEDSTORIDOUT
                    left outer join DEFSTORAGE ins on ins.DSTORID = PRDEDSTORIDIN
                    left outer join DIVISON outd on outd.DIVVAL = ins.DSTORDIVISON
                    left outer join DIVISON ind on ind.DIVVAL = outs.DSTORDIVISON
			        inner join MDE_GENEL.dbo.MB_Islemler on MB_ORDCHID = PRDEID
                    outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = MB_PROID and PRLDPRID = 740) as pesinfiyat
                    where PRDESTS = 0 and PRDEKIND= 1 and MB_Tamamlandi = 0 
                    and MB_SUPCURVAL = '{0}'
                    and MB_PlanTarih between '{1}' and '{2}'
                    group by outd.DIVVAL,outd.DIVNAME,MB_PlanTarih,ind.DIVNAME) sonuc", loginRes[0].CURVAL, startdate, enddate);
                string qq = String.Format(@"select count(adet) as Toplamadet, sum(tutar) as Toplamtutar from (
                select distinct Convert(varchar(50),MB_SALID) as adet, sum(MB_SellAmount) as tutar from MDE_GENEL..MB_Islemler
                outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = MB_PROID and PRLDPRID = 740) as pesinfiyat
                where MB_Tamamlandi = 1 and MB_SALID != 0 and MB_SUPCURVAL = '{0}'
                and MB_PlanTarih between '{1}' and '{2}'
                group by MB_SALID
                union
                select outd.DIVNAME as ISTEYEN,sum(MB_SellAmount) as TUTAR
                            from PRODEMAND 
                            left outer join PRODUCTS on PROID = PRDEPROID
                            left outer join DEFSTORAGE outs on outs.DSTORID = PRDEDSTORIDOUT
                            left outer join DEFSTORAGE ins on ins.DSTORID = PRDEDSTORIDIN
                            left outer join DIVISON outd on outd.DIVVAL = ins.DSTORDIVISON
                            left outer join DIVISON ind on ind.DIVVAL = outs.DSTORDIVISON
			                inner join MDE_GENEL.dbo.MB_Islemler on MB_ORDCHID = PRDEID
                            outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = PROID and PRLDPRID = 740) as pesinfiyat
                            where MB_Tamamlandi = 1 
                            and MB_SUPCURVAL = '{0}'
                            and MB_PlanTarih between '{1}' and '{2}'
                            group by outd.DIVVAL,outd.DIVNAME,MB_PlanTarih,ind.DIVNAME) sonuc", loginRes[0].CURVAL, startdate, enddate);
                //string q = String.Format(@"select CDRSALID,CDRCURID,CURVAL,CURNAME,ORDDATE,sum(ORDCHBALANCEQUAN) as ORDCHBALANCEQUAN,Convert(numeric(18,2),sum(ORDCHBALANCEQUAN*PRLPRICE)) as PRLPRICE,CURCHCOUNTY
                //FROM  CUSDELIVER
                //inner join MDE_GENEL.dbo.MB_Islemler on MB_SALID = CDRSALID and CDRORDCHID = MB_ORDCHID
                //left outer join CURRENTS on CURID = CDRCURID
                //left outer join CURRENTSCHILD on CURCHID = CURID
                //left outer join ORDERS on ORDSALID = CDRSALID
                //left outer join ORDERSCHILD on ORDCHORDID = ORDID and ORDCHID = MB_ORDCHID and CDRORDCHID = ORDCHID
                //left outer join DEFSTORAGE WITH (NOLOCK) ON CDRSTORID = DSTORID
                //left outer join DIVISON TESLIM WITH (NOLOCK) ON TESLIM.DIVVAL = DSTORVAL AND ORDCHCOMPANY = TESLIM.DIVCOMPANY
                //outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = MB_PROID and PRLDPRID = 740) as pesinfiyat
                //where MB_Tamamlandi = 0 and MB_SUPCURVAL = '{0}'  --and ORDCHBALANCEQUAN > 0
                //group by CDRSALID,CDRCURID,CURVAL,CURNAME,ORDDATE,CURCHCOUNTY
                //order by ORDDATE,CURCHCOUNTY,CURNAME", loginRes[0].CURVAL);
                //string qq = String.Format(@"select CDRSALID,CDRCURID,CURVAL,CURNAME,ORDDATE,sum(ORDCHBALANCEQUAN) as ORDCHBALANCEQUAN,Convert(numeric(18,2),sum(ORDCHBALANCEQUAN*PRLPRICE)) as PRLPRICE,CURCHCOUNTY
                //FROM  CUSDELIVER
                //inner join MDE_GENEL.dbo.MB_Islemler on MB_SALID = CDRSALID and CDRORDCHID = MB_ORDCHID
                //left outer join CURRENTS on CURID = CDRCURID
                //left outer join CURRENTSCHILD on CURCHID = CURID
                //left outer join ORDERS on ORDSALID = CDRSALID
                //left outer join ORDERSCHILD on ORDCHORDID = ORDID and ORDCHID = MB_ORDCHID and CDRORDCHID = ORDCHID
                //left outer join DEFSTORAGE WITH (NOLOCK) ON CDRSTORID = DSTORID
                //left outer join DIVISON TESLIM WITH (NOLOCK) ON TESLIM.DIVVAL = DSTORVAL AND ORDCHCOMPANY = TESLIM.DIVCOMPANY
                //outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = MB_PROID and PRLDPRID = 740) as pesinfiyat
                //where MB_Tamamlandi = 1 and MB_SUPCURVAL = '{0}'  --and ORDCHBALANCEQUAN > 0
                //group by CDRSALID,CDRCURID,CURVAL,CURNAME,ORDDATE,CURCHCOUNTY
                //order by ORDDATE,CURCHCOUNTY,CURNAME", loginRes[0].CURVAL);
                var dt = DbQuery.Query(q, ConnectionString);
                var dt2 = DbQuery.Query(qq, ConnectionString);

                if (dt != null)
                {
                    bekleyenadet.InnerText = dt.Rows[0]["Toplamadet"].ToString();
                    double ciro = 0;
                    ciro = double.Parse(dt.Rows[0]["Toplamtutar"].ToString());
                    bekleyenciro.InnerText = ciro.ToString("C", new CultureInfo("tr-TR"));
                    bekleyenhakedis.InnerText = (ciro * Oran).ToString("C", new CultureInfo("tr-TR"));
                    //bekleyenadet.InnerText = dt.Rows.Count.ToString();
                    //double ciro = 0;
                    //for (int i = 0; i < dt.Rows.Count; i++)
                    //{
                    //    ciro = ciro + double.Parse(dt.Rows[0]["PRLPRICE"].ToString());
                    //}
                    //bekleyenciro.InnerText = ciro.ToString("C", new CultureInfo("tr-TR"));
                    //bekleyenhakedis.InnerText = (ciro * 0.08).ToString("C", new CultureInfo("tr-TR"));
                }
                else
                {
                    bekleyenadet.InnerText = "0";
                    bekleyenciro.InnerText = "0";
                    bekleyenhakedis.InnerText = "0";


                }
                if (dt2 != null)
                {
                    tamamlananadet.InnerText = dt2.Rows[0]["Toplamadet"].ToString();
                    double ciro2 = 0;
                    ciro2 = double.Parse(dt2.Rows[0]["Toplamtutar"].ToString());
                    tamamalananciro.InnerText = ciro2.ToString("C", new CultureInfo("tr-TR"));
                    tamamlananhakedis.InnerText = (ciro2 * Oran).ToString("C", new CultureInfo("tr-TR"));
                    //tamamlananadet.InnerText = dt2.Rows.Count.ToString();
                    //double ciro2 = 0;
                    //for (int i = 0; i < dt2.Rows.Count; i++)
                    //{
                    //    ciro2 = ciro2 + double.Parse(dt2.Rows[0]["PRLPRICE"].ToString());
                    //}
                    //tamamalananciro.InnerText = ciro2.ToString("C", new CultureInfo("tr-TR"));
                    //tamamlananhakedis.InnerText = (ciro2 * 0.08).ToString("C", new CultureInfo("tr-TR"));
                }
                else
                {
                    tamamlananadet.InnerText = "0";
                    tamamalananciro.InnerText = "0";
                    tamamlananhakedis.InnerText = "0";
                }
            }
        }

        protected void Onayla_Click(object sender, EventArgs e)
        {
            Liste(StartDate.Value.ToString(), EndDate.Value.ToString());
        }
        void Liste(string Startdate, string Enddate)
        {
            BindGrid(Startdate, Enddate);
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
                string q = String.Format(@"select Convert(varchar(max),SALID) as SALID,CURID,CURNAME,MB_PlanTarih,case when MB_Tamamlandi = 1 then 'Tamamlandı' else 'Bekliyor' end Sonuc,OFFCURNAME,sum(MB_SellAmount) as Ciro,sum(MB_SellAmount*{3}) as Hakedis from MDE_GENEL..MB_Islemler islem
                left outer join SALES on SALID = islem.MB_SALID
                left outer join CURRENTS on CURID = SALCURID
                left outer join OFFICALCUR on OFFCURID = MB_Ekleyen
                outer apply (select PRLPRICE from PRICELIST prl WITH (NOLOCK) where prl.PRLPROID = islem.MB_PROID and PRLDPRID = 740) as pesinfiyat
                where MB_Tamamlandi = 1 and islem.MB_SALID != 0 and SALDIVISON in ({0}) 
				and MB_PlanTarih between '{1}' and '{2}'
				group by SALID,CURID,CURNAME,MB_PlanTarih,MB_Tamamlandi,OFFCURNAME
                union
                select STRING_AGG(SALID, ','),CURID,CURNAME,MB_PlanTarih,case when MB_Tamamlandi = 1 then 'Tamamlandı' else 'Bekliyor' end Sonuc,OFFCURNAME,sum(PRLPRICE) as Ciro,sum(Hakedis) as Hakedis from (
                    select MB_ORDCHID as SALID,MB_SALID as CURID,DIVNAME as CURNAME,MB_PlanTarih,MB_Tamamlandi,OFFCURNAME,sum(MB_SellAmount) as PRLPRICE,sum(MB_SellAmount*{3}) as Hakedis from MDE_GENEL..MB_Islemler
                inner join PRODEMAND on PRDEID = MB_ORDCHID
                left outer join DIVISON on PRDEDIVISON = DIVVAL
                left outer join OFFICALCUR on OFFCURID = MB_Ekleyen
                outer apply (select PRLPRICE from PRICELIST prl WITH (NOLOCK) where prl.PRLPROID = MB_PROID and PRLDPRID = 740) as pesinfiyat
                where MB_Tamamlandi = 1 and DIVVAL in ({0}) 
				and MB_PlanTarih between '{1}' and '{2}'
				group by MB_ORDCHID,MB_SALID,DIVNAME,MB_PlanTarih,MB_Tamamlandi,OFFCURNAME
				) son
				group by CURID,CURNAME,MB_PlanTarih,MB_Tamamlandi,OFFCURNAME
                order by MB_PlanTarih", magaza, Startdate, Enddate,Oran);

    //            string q = String.Format(@"SELECT DISTINCT
    //                MB_PlanTarih,
				//    Convert(varchar(100),SALID) as SALID,
				//    CURID,
    //                CURNAME,
				//    case when MB_Tamamlandi = 1 then 'Tamamlandı' else 'Bekliyor' end Sonuc,
				//    sum(MB_SellAmount*ORDCHQUAN) as Ciro,
				//    sum(MB_SellAmount*ORDCHQUAN)*0.08 as Hakedis
    //            FROM MDE_GENEL.dbo.MB_Islemler
    //            LEFT OUTER JOIN SALES ON SALID = MB_SALID
    //            LEFT OUTER JOIN CURRENTS ON CURID = SALCURID
			 //   LEFT OUTER JOIN ORDERSCHILD on ORDCHID = MB_ORDCHID
				//LEFT OUTER JOIN PRICELIST on PRLPROID = ORDCHPROID and PRLDPRID = 740
    //            where MB_SALID != 0 --and MB_Tamamlandi = 1
			 //   and SALDIVISON in ({0})
			 //   and MB_PlanTarih between '{1}' and '{2}'
			 //   group by MB_PlanTarih,
				//    SALID,
				//    CURID,CURNAME,MB_Tamamlandi
    //            union
    //            SELECT DISTINCT
    //                MB_PlanTarih,
				//	STRING_AGG(MB_ORDCHID,',') as SALID,
				//	0 as CURID,
    //                DIVNAME,
				//    case when MB_Tamamlandi = 1 then 'Tamamlandı' else 'Bekliyor' end Sonuc,
				//    sum(MB_SellAmount*PRDEQUAN) as Ciro,
				//    sum(MB_SellAmount*PRDEQUAN)*0.08 as Hakedis
    //            FROM MDE_GENEL.dbo.MB_Islemler
			 //   LEFT OUTER JOIN PRODEMAND on PRDEID = MB_ORDCHID
    //            left outer join PRODUCTS on PROID = PRDEPROID
    //            left outer join DIVISON on PRDEDIVISON = DIVVAL
				//left outer join DEFSTORAGE on DSTORID = PRDEDSTORIDIN
    //            where MB_SALID = 0 --and MB_Tamamlandi = 1
			 //   and DIVVAL  in ({0})
			 //   and MB_PlanTarih between '{1}' and '{2}'
			 //   group by MB_PlanTarih,
				//	MB_SALID,
				//	DIVVAL,DIVNAME,MB_Tamamlandi
			 //   order by 1,2", magaza, Startdate, Enddate);
                Veriler = DbQuery.Query(q, ConnectionString);
                GridView1.DataSource = Veriler;
                GridView1.DataBind();

                AyIsmi.InnerText = DateTime.Parse(Startdate).ToString("dd", new CultureInfo("tr-TR")) +"-"+ DateTime.Parse(Startdate).ToString("MMMM", new CultureInfo("tr-TR")) + " - " + DateTime.Parse(Enddate).ToString("dd", new CultureInfo("tr-TR"))+"-"+ DateTime.Parse(Enddate).ToString("MMMM", new CultureInfo("tr-TR")) + " Hakediş Raporu";
            }
        }

        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridView1.PageIndex = e.NewPageIndex;
            Liste(StartDate.Value.ToString(), EndDate.Value.ToString()); // Sayfa değiştiğinde verileri yeniden bağlayın
        }

        protected void GridView1_RowCreated(object sender, GridViewRowEventArgs e)
        {
            var ss = e.Row.RowType;
            if (e.Row.Cells.Count > 1)
            {
                e.Row.Cells[1].Visible = false;
                e.Row.Cells[2].Visible = false;
            }
        }

        protected void GridView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            table.DataSource = null;
            table.DataBind();
            CURID = GridView1.SelectedRow.Cells[2].Text;
            SALID = GridView1.SelectedRow.Cells[1].Text;
            if (CURID != "0")
            {
                string q = String.Format(@"select 
	             d.MB_CURID as CURID
	            ,CURNAME
	            ,FileTypeName
	            ,MB_FileName as FileName
                from MDE_GENEL.dbo.MB_BayiDosyaları d
                left outer join MDE_GENEL.dbo.MB_DosyaTipi t on t.id = MB_FileType
                left outer join VDB_YON01.dbo.CURRENTS c on c.CURID = d.MB_CURID
                where d.MB_CURID = {0}
                order by MB_FileType", CURID);
                var dt = DbQuery.Query(q, ConnectionString);
                if (dt != null)
                {
                    FileLoad.Visible = true;
                    table.DataSource = dt;
                    table.DataBind();
                }
            }
            else
            {
                string q = String.Format(@"select 
	             d.MB_CURID as CURID
	            ,CURNAME
	            ,FileTypeName
	            ,MB_FileName as FileName
                from MDE_GENEL.dbo.MB_BayiDosyaları d
                left outer join MDE_GENEL.dbo.MB_DosyaTipi t on t.id = MB_FileType
                left outer join VDB_YON01.dbo.CURRENTS c on c.CURID = d.MB_CURID
                where d.MB_CURID = {0} and MB_SALID in ({1})
                order by MB_FileType", CURID, SALID);
                var dt = DbQuery.Query(q, ConnectionString);
                if (dt != null)
                {
                    FileLoad.Visible = true;
                    table.DataSource = dt;
                    table.DataBind();
                }
            }
            imgViewer.Visible = false;
            pdfViewerPlaceHolder.Visible = false;
        }

        protected void table_SelectedIndexChanged(object sender, EventArgs e)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "showLoading", "showLoading();", true);
            // Retrieve the URL from ViewState
            var Filename = table.SelectedRow.Cells[4].Text;
            var FileType = table.SelectedRow.Cells[3].Text.ToUpper();
            string uploadedFileUrl = "";
            if (CURID != "0")
            {
                uploadedFileUrl = ftpUrl + "/" + CURID + "/Mobilya Montaj/" + FileType + "/" + Filename;
            }
            else
            {
                uploadedFileUrl = ftpUrl + "/" + CURID + "/Mobilya Montaj/" + FileType + "/" + SALID + "/" + Filename;
            }
            try
            {
                if (Filename.EndsWith("pdf") == true || Filename.EndsWith("PDF") == true)
                {
                    WebClient ftpClient = new WebClient();
                    ftpClient.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
                    byte[] imageByte = ftpClient.DownloadData(uploadedFileUrl);


                    var tempFileName = Path.GetTempFileName().Replace("tmp", "pdf");

                    System.IO.File.WriteAllBytes(tempFileName, imageByte);

                    string webFolderPath = Server.MapPath("~/TempImages/");
                    if (!Directory.Exists(webFolderPath))
                    {
                        Directory.CreateDirectory(webFolderPath);
                    }
                    string webFilePath = Path.Combine(webFolderPath, Path.GetFileName(tempFileName));
                    File.Copy(tempFileName, webFilePath, true);

                    string relativeFilePath = "~/TempImages/" + Path.GetFileName(tempFileName);

                    // Use iframe to display the PDF
                    string pdfIframe = $"<iframe src='{ResolveUrl(relativeFilePath)}' type='application/pdf' width='600' height='500'></iframe>";
                    iframe.Src = $"{ResolveUrl(relativeFilePath)}";
                    pdfViewerPlaceHolder.Controls.Clear();
                    pdfViewerPlaceHolder.Controls.Add(new Literal { Text = pdfIframe });
                    iframe.Visible = false;
                    imgViewer.Visible = false;
                    pdfViewerPlaceHolder.Visible = false;

                }
                else if (Filename.EndsWith("jpg") || Filename.EndsWith("JPG") || Filename.EndsWith("jpeg") || Filename.EndsWith("JPEG") || Filename.EndsWith("png") || Filename.EndsWith("PNG") || Filename.EndsWith("jfif"))
                {
                    //System.Threading.Thread.Sleep(5000);
                    WebClient ftpClient = new WebClient();
                    ftpClient.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
                    byte[] imageByte = ftpClient.DownloadData(uploadedFileUrl);


                    var tempFileName = Path.GetTempFileName();
                    System.IO.File.WriteAllBytes(tempFileName, imageByte);

                    // Save the file in a web accessible folder
                    string webFolderPath = Server.MapPath("~/TempImages/");
                    if (!Directory.Exists(webFolderPath))
                    {
                        Directory.CreateDirectory(webFolderPath);
                    }

                    string webFilePath = Path.Combine(webFolderPath, Path.GetFileName(tempFileName) + ".jpg");
                    File.Copy(tempFileName, webFilePath, true);

                    // Set the ImageUrl to the web accessible file path
                    imgViewer.ImageUrl = "~/TempImages/" + Path.GetFileName(webFilePath);
                    imgViewer.Visible = true;
                    // İşlem tamamlandığında yükleme ekranını gizle
                    ScriptManager.RegisterStartupScript(this, GetType(), "hideLoading", "hideLoading();", true);

                }
                else
                {
                    imgViewer.Visible = false;
                    pdfViewerPlaceHolder.Visible = false;
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda yükleme ekranını gizle
                ScriptManager.RegisterStartupScript(this, GetType(), "hideLoading", "hideLoading();", true);
                FileLoad.Visible = true;
                FileLoad.Text = "Error: " + ex.Message;
            }
        }

        protected void table_RowCreated(object sender, GridViewRowEventArgs e)
        {
            e.Row.Cells[1].Visible = false;
        }

        protected void Excel_Click(object sender, EventArgs e)
        {
            GridView1.AllowPaging = false;
            Liste(StartDate.Value.ToString(), EndDate.Value.ToString());
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (ExcelPackage package = new ExcelPackage(memoryStream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(StartDate.Value + " arası " + EndDate.Value);
                    worksheet.Cells[1, 1].Value = StartDate.Value + " arası " + EndDate.Value + "Hakediş Listesi";

                    int genislik = 0;

                    // Başlıkları yaz (gizli olanları hariç tut)
                    int colIndex = 1;
                    for (int i = 0; i < GridView1.HeaderRow.Cells.Count; i++)
                    {
                        if (GridView1.HeaderRow.Cells[i].Visible) // Gizli sütunları kontrol et
                        {
                            worksheet.Cells[2, colIndex].Value = HttpUtility.HtmlDecode(GridView1.HeaderRow.Cells[i].Text);
                            worksheet.Cells[2, colIndex].AutoFilter = true;
                            // Hücreye çerçeve ekle
                            worksheet.Cells[2, colIndex].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            worksheet.Cells[2, colIndex].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            worksheet.Cells[2, colIndex].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            worksheet.Cells[2, colIndex].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                            colIndex++;
                            genislik++;
                        }
                    }
                    worksheet.Cells[1, 1, 1, genislik].Merge = true; // 1. satırdaki tüm sütunları birleştir

                    // Hücreyi ortala ve arka plan rengini gri yap
                    var headerCell = worksheet.Cells[1, 1, 1, genislik];
                    headerCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center; // Yatay ortalama
                    headerCell.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center; // Dikey ortalama
                    headerCell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid; // Arka plan tipini ayarla
                    headerCell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightSeaGreen); // Arka plan rengini gri yap

                    // Verileri yaz (gizli olanları hariç tut)
                    decimal totalCiro = 0, totalHakedis = 0; // Toplamlar

                    // Kullanıcının bilgisayarındaki geçerli para birimi simgesini alalım
                    var currentCulture = CultureInfo.CurrentCulture;
                    var currencySymbol = currentCulture.NumberFormat.CurrencySymbol;

                    for (int rowIndex = 0; rowIndex < GridView1.Rows.Count; rowIndex++)
                    {
                        colIndex = 1; // Her yeni satır için column index'ini sıfırla
                        for (int cellIndex = 0; cellIndex < GridView1.Rows[rowIndex].Cells.Count; cellIndex++)
                        {
                            if (GridView1.Rows[rowIndex].Cells[cellIndex].Visible) // Gizli sütunları kontrol et
                            {
                                string cellValue = HttpUtility.HtmlDecode(GridView1.Rows[rowIndex].Cells[cellIndex].Text);
                                worksheet.Cells[rowIndex + 3, colIndex].Value = HttpUtility.HtmlDecode(GridView1.Rows[rowIndex].Cells[cellIndex].Text);


                                // Hücreye çerçeve ekle
                                worksheet.Cells[rowIndex + 3, colIndex].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                worksheet.Cells[rowIndex + 3, colIndex].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                worksheet.Cells[rowIndex + 3, colIndex].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                worksheet.Cells[rowIndex + 3, colIndex].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                                // Sayısal alanlar için toplamları hesapla
                                if (GridView1.HeaderRow.Cells[cellIndex].Text == "CİRO")
                                {
                                    // Para birimi simgesini kaldır ve sayıya dönüştür
                                    string numericValue = cellValue.Replace(currencySymbol, "").Trim(); // Sadece sayıyı al
                                    totalCiro += Convert.ToDecimal(numericValue); // CultureInfo ile dönüşüm yap
                                }
                                if (GridView1.HeaderRow.Cells[cellIndex].Text == "HAKEDİŞ")
                                {
                                    // Para birimi simgesini kaldır ve sayıya dönüştür
                                    string numericValue = cellValue.Replace(currencySymbol, "").Trim(); // Sadece sayıyı al
                                    totalHakedis += Convert.ToDecimal(numericValue); // CultureInfo ile dönüşüm yap
                                }
                                // "SONUÇ" sütunundaki değeri kontrol et ve koşul uygulaması
                                if (GridView1.HeaderRow.Cells[cellIndex].Text == "SONU&#199;" && cellValue == "Bekliyor")
                                {
                                    // "Bekliyor" değeri ise arka planı kırmızı, yazıyı beyaz yap
                                    worksheet.Cells[rowIndex + 3, colIndex].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                    worksheet.Cells[rowIndex + 3, colIndex].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                                    worksheet.Cells[rowIndex + 3, colIndex].Style.Font.Color.SetColor(System.Drawing.Color.White);
                                }
                                colIndex++;
                            }
                        }
                    }
                    // Toplamları en alta ekle
                    int lastRow = GridView1.Rows.Count + 3; // Son satır

                    worksheet.Cells[lastRow, 1].Value = "Toplam"; // Toplam yazısı
                    worksheet.Cells[lastRow, 1, lastRow, genislik-2].Merge = true;
                    worksheet.Cells[lastRow, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right; // Yatay ortalama
                    worksheet.Cells[lastRow, 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center; // Dikey ortalama
                    worksheet.Cells[lastRow, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid; // Arka plan tipini ayarla
                    worksheet.Cells[lastRow, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray); // Arka plan rengini gri yap

                    // Ciro ve Hakediş toplamlarını yaz
                    worksheet.Cells[lastRow, 5].Value = totalCiro.ToString("C2"); // Ciro toplamı
                    worksheet.Cells[lastRow, 6].Value = totalHakedis.ToString("C2"); // Hakediş toplamı

                    // Sütunları veri boyutuna göre genişlet
                    worksheet.Cells.AutoFitColumns();

                    // AutoFilter eklemek için "SONUÇ" sütununa göre AutoFilter uygulama
                    // Burada 5. sütun olan "SONUÇ" sütununa filtre ekliyoruz
                    worksheet.Cells[2, 1, lastRow-1, genislik].AutoFilter = true;
                    // Dosyayı kaydet
                    package.Save();
                }

                // Kullanıcıya dosyayı indirmesi için bir response gönderin
                string filename = $"{StartDate.Value} arası {EndDate.Value}";
                Response.Clear();
                Response.Buffer = true;
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("Content-Disposition", $"attachment;filename=Hakediş Listesi - {filename}.xlsx");
                Response.BinaryWrite(memoryStream.ToArray());
                Response.End();
            }
            #region eskikod
            //ExportGridToExcel();
            //ExportExcel();
            //try
            //{
            //    // Sayfalamayı geçici olarak devre dışı bırak
            //    bool originalPaging = GridView1.AllowPaging;
            //    GridView1.AllowPaging = false;
            //    Liste(StartDate.Value.ToString(), EndDate.Value.ToString()); // Tüm verileri yeniden bağla

            //    Response.Clear();
            //    Response.Buffer = true;
            //    Response.AddHeader("content-disposition", "attachment;filename=SiparisListesi.xls");
            //    Response.Charset = "";
            //    Response.ContentType = "application/vnd.ms-excel";
            //    Response.ContentEncoding = System.Text.Encoding.UTF8;
            //    Response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble());

            //    using (StringWriter sw = new StringWriter())
            //    {
            //        using (HtmlTextWriter hw = new HtmlTextWriter(sw))
            //        {
            //            // Excel stil tanımlamaları
            //            hw.Write("<style>");
            //            hw.Write("td { border: 0.5pt solid #000000; vertical-align: middle; }");
            //            hw.Write("th { border: 0.5pt solid #000000; background-color: #D3D3D3; font-weight: bold; }");
            //            hw.Write("</style>");

            //            // Başlık tablosu
            //            Table headerTable = new Table();
            //            headerTable.GridLines = GridLines.Both;

            //            // Başlık satırı
            //            TableRow titleRow = new TableRow();
            //            TableCell titleCell = new TableCell();
            //            titleCell.Text = "Sipariş Listesi";
            //            titleCell.ColumnSpan = GridView1.Columns.Count - 1; // Resimleri Gör butonu hariç
            //            titleCell.HorizontalAlign = HorizontalAlign.Center;
            //            titleCell.Style.Add("background-color", "#D3D3D3");
            //            titleCell.Style.Add("font-weight", "bold");
            //            titleCell.Style.Add("padding", "10px");
            //            titleRow.Cells.Add(titleCell);
            //            headerTable.Rows.Add(titleRow);

            //            // Tarih satırı
            //            TableRow dateRow = new TableRow();
            //            TableCell dateCell = new TableCell();
            //            dateCell.Text = "Oluşturma Tarihi: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm");
            //            dateCell.ColumnSpan = GridView1.Columns.Count - 1;
            //            dateCell.HorizontalAlign = HorizontalAlign.Right;
            //            dateCell.Style.Add("padding", "5px");
            //            dateRow.Cells.Add(dateCell);
            //            headerTable.Rows.Add(dateRow);

            //            // Boş satır
            //            TableRow emptyRow = new TableRow();
            //            TableCell emptyCell = new TableCell();
            //            emptyCell.Text = "&nbsp;";
            //            emptyCell.ColumnSpan = GridView1.Columns.Count - 1;
            //            emptyRow.Cells.Add(emptyCell);
            //            headerTable.Rows.Add(emptyRow);

            //            headerTable.RenderControl(hw);

            //            // GridView'i hazırla
            //            GridView exportGrid = new GridView();
            //            exportGrid.DataSource = GridView1.DataSource;
            //            exportGrid.DataBind();

            //            // Resimleri Gör kolonunu kaldır
            //            exportGrid.Columns.RemoveAt(0);

            //            // Stil ayarları
            //            exportGrid.GridLines = GridLines.Both;
            //            exportGrid.HeaderStyle.BackColor = System.Drawing.Color.LightGray;
            //            exportGrid.HeaderStyle.Font.Bold = true;

            //            // Her satır için stil ve format ayarları
            //            foreach (GridViewRow row in exportGrid.Rows)
            //            {
            //                row.BackColor = row.RowIndex % 2 == 0 ?
            //                    System.Drawing.Color.White :
            //                    System.Drawing.Color.WhiteSmoke;

            //                for (int i = 0; i < row.Cells.Count; i++)
            //                {
            //                    // Para birimi düzeltmesi
            //                    if (i == 5 || i == 6) // Ciro ve Hakedis kolonları
            //                    {
            //                        if (row.Cells[i].Text.StartsWith("₺"))
            //                        {
            //                            row.Cells[i].Text = row.Cells[i].Text.Replace("₺", "TL ");
            //                        }
            //                    }

            //                    // Hizalama
            //                    row.Cells[i].HorizontalAlign = (i == 2 || i == 4 || i == 5 || i == 6) ?
            //                        HorizontalAlign.Left :
            //                        HorizontalAlign.Center;

            //                    row.Cells[i].Style.Add("mso-number-format", "@"); // Metin formatı
            //                }
            //            }

            //            exportGrid.RenderControl(hw);

            //            // Alt bilgi
            //            Table footerTable = new Table();
            //            footerTable.GridLines = GridLines.Both;
            //            TableRow footerRow = new TableRow();
            //            TableCell footerCell = new TableCell();
            //            footerCell.Text = "© " + DateTime.Now.Year.ToString() + " - Tüm hakları saklıdır.";
            //            footerCell.ColumnSpan = GridView1.Columns.Count - 1;
            //            footerCell.HorizontalAlign = HorizontalAlign.Center;
            //            footerCell.Style.Add("padding", "5px");
            //            footerRow.Cells.Add(footerCell);
            //            footerTable.Rows.Add(footerRow);
            //            footerTable.RenderControl(hw);

            //            // Excel dosyasını oluştur
            //            Response.Output.Write(sw.ToString());
            //            Response.Flush();
            //            Response.End();
            //        }
            //    }

            //    // Sayfalamayı eski haline getir
            //    GridView1.AllowPaging = originalPaging;
            //    Liste(StartDate.Value.ToString(), EndDate.Value.ToString());
            //}
            //catch (Exception ex)
            //{
            //    // Hata yönetimi
            //    Response.Clear();
            //    Response.Write("Excel dışa aktarma sırasında bir hata oluştu: " + ex.Message);
            //    Response.End();
            //}
            #endregion
            GridView1.AllowPaging = true;
        }
        private void ExportGridToExcel()
        {
            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment;filename=SiparisListesi.xls");
            Response.Charset = "";
            Response.ContentType = "application/vnd.ms-excel";

            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                {
                    // Excel başlık satırı oluştur
                    Table headerTable = new Table();
                    TableRow headerRow = new TableRow();

                    TableCell titleCell = new TableCell();
                    titleCell.Text = "Sipariş Listesi";
                    titleCell.ColumnSpan = GridView1.Columns.Count;
                    titleCell.HorizontalAlign = HorizontalAlign.Center;
                    titleCell.BackColor = System.Drawing.Color.LightGray;
                    headerRow.Cells.Add(titleCell);
                    headerTable.Rows.Add(headerRow);

                    // Tarih bilgisi ekle
                    TableRow dateRow = new TableRow();
                    TableCell dateCell = new TableCell();
                    dateCell.Text = "Oluşturma Tarihi: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                    dateCell.ColumnSpan = GridView1.Columns.Count;
                    dateCell.HorizontalAlign = HorizontalAlign.Right;
                    dateRow.Cells.Add(dateCell);
                    headerTable.Rows.Add(dateRow);

                    // Boş satır ekle
                    TableRow emptyRow = new TableRow();
                    TableCell emptyCell = new TableCell();
                    emptyCell.Text = "&nbsp;";
                    emptyCell.ColumnSpan = GridView1.Columns.Count;
                    emptyRow.Cells.Add(emptyCell);
                    headerTable.Rows.Add(emptyRow);

                    headerTable.RenderControl(hw);

                    // GridView'i Excel formatına uygun hale getir
                    GridView1.AllowPaging = false;
                    GridView1.DataBind();

                    // Stil ayarları
                    GridView1.GridLines = GridLines.Both;
                    GridView1.HeaderStyle.Font.Bold = true;
                    GridView1.HeaderStyle.BackColor = System.Drawing.Color.LightBlue;

                    // Resimleri Gör butonunu gizle
                    GridView1.Columns[0].Visible = false;

                    foreach (GridViewRow row in GridView1.Rows)
                    {
                        // Alternatif satır renklendirmesi
                        if (row.RowIndex % 2 == 0)
                        {
                            row.BackColor = System.Drawing.Color.White;
                        }
                        else
                        {
                            row.BackColor = System.Drawing.Color.WhiteSmoke;
                        }

                        // Para birimi formatlaması için özel işlem
                        if (row.Cells[6].Text.StartsWith("₺"))
                        {
                            row.Cells[6].Text = row.Cells[6].Text.Replace("₺", "TL ");
                        }
                        if (row.Cells[7].Text.StartsWith("₺"))
                        {
                            row.Cells[7].Text = row.Cells[7].Text.Replace("₺", "TL ");
                        }

                        // Hücre stillerini ayarla
                        for (int i = 0; i < row.Cells.Count; i++)
                        {
                            row.Cells[i].Wrap = false;

                            // Orijinal GridView'deki hizalamaları koru
                            if (i == 2 || i == 3 || i == 5 || i == 6 || i == 7) // CURNAME, Sonuc, Ciro ve Hakedis kolonları
                            {
                                row.Cells[i].HorizontalAlign = HorizontalAlign.Left;
                            }
                            else
                            {
                                row.Cells[i].HorizontalAlign = HorizontalAlign.Center;
                            }
                        }
                    }

                    // GridView'i render et
                    GridView1.RenderControl(hw);

                    // Alt bilgi ekle
                    Table footerTable = new Table();
                    TableRow footerRow = new TableRow();
                    TableCell footerCell = new TableCell();
                    footerCell.Text = "© " + DateTime.Now.Year.ToString() + " - Tüm hakları saklıdır.";
                    footerCell.ColumnSpan = GridView1.Columns.Count;
                    footerCell.HorizontalAlign = HorizontalAlign.Center;
                    footerRow.Cells.Add(footerCell);
                    footerTable.Rows.Add(footerRow);

                    footerTable.RenderControl(hw);

                    // Excel dosyasını oluştur ve indir
                    Response.Output.Write(sw.ToString());
                    Response.Flush();
                    Response.End();
                }
            }
        }
        void ExportExcel()
        {
            // Clear the response to ensure no other data is sent to the browser
            Response.Clear();
            Response.Buffer = true;

            // Set the content type and the filename for the Excel file
            Response.ContentType = "application/vnd.ms-excel";
            Response.AddHeader("content-disposition", "attachment;filename=GridViewExport.xls");

            // Set the style for the Excel export (optional, you can add more styles)
            //Response.Write("<style> .text { mso-number-format:\@; } </style>");

            // Create a StringWriter and HtmlTextWriter to render the GridView as HTML
            StringWriter sw = new StringWriter();
            HtmlTextWriter hw = new HtmlTextWriter(sw);

            // Hide the pager for the export
            GridView1.AllowPaging = false;
            GridView1.DataBind();  // Ensure the GridView has data

            // Render the GridView as HTML to the StringWriter
            GridView1.RenderControl(hw);

            // Output the content to the browser
            Response.Write(sw.ToString());
            Response.End();
        }
        public override void VerifyRenderingInServerForm(Control control)
        {
            // Excel export işlemi için form doğrulamasını devre dışı bırak
            if (control is GridView || control is Table)
            {
                return;
            }
            else
            {
                base.VerifyRenderingInServerForm(control);
            }
        }
    }
}