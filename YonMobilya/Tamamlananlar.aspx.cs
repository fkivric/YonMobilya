using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static YonMobilya.Class.Siniflar;
using YonMobilya.Class;
using System.Data.SqlClient;
using System.Net;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.IO;
using System.Data;

namespace YonMobilya
{
    public partial class Tamamlananlar : System.Web.UI.Page
    {
        public static string ConnectionString = "Server=192.168.4.24;Database=VDB_YON01;User Id=sa;Password=MagicUser2023!;";
        public static string ConnectionString2 = "Server=192.168.4.24;Database=MDE_GENEL;User Id=sa;Password=MagicUser2023!;";
        SqlConnection sql = new SqlConnection(ConnectionString);
        SqlConnection sql2 = new SqlConnection(ConnectionString2);
        static string ftpUrl = "";
        static string ftpUsername = "";
        static string ftpPassword = "";
        public static string CURID = "";
        public static string SALID = "";
        public static string ORDCHID = "";
        public static string PRONAME = "";
        public static string PROID = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var loginRes = (List<LoginObj>)Session["Login"];
                if (loginRes != null)
                {
                    StartDate.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("yyyy-MM-dd");
                    EndDate.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd");
                    ScriptManager.RegisterStartupScript(this, GetType(), "showLoading", "showLoading();", true);
                    var ftp = (List<Ftp>)Session["FTP"];
                    if (ftp != null)
                    {
                        ftpUrl = ftp[0].VolFtpHost;
                        ftpUsername = ftp[0].VolFtpUser;
                        ftpPassword = ftp[0].VolFtpPass;
                    }
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
                ScriptManager.RegisterStartupScript(this, GetType(), "hideLoading", "hideLoading();", true);
            }

        }

        protected void Onayla_Click(object sender, EventArgs e)
        {

            BindGrid(StartDate.Value.ToString(),EndDate.Value.ToString());
        }
        private void BindGrid(string Startdate, string Enddate)
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
                string q = String.Format(@"select Convert(varchar(max),SALID) as SALID,CURID,CURNAME,MB_PlanTarih,Convert(Char(10),MB_TamamlanmaTarihi,121) as MB_TamamlanmaTarihi,OFFCURNAME,sum(PRLPRICE) as PRLPRICE,sum(PRLPRICE*0.08) as Hakedis from MDE_GENEL..MB_Islemler islem
                left outer join SALES on SALID = islem.MB_SALID
                left outer join CURRENTS on CURID = SALCURID
                left outer join OFFICALCUR on OFFCURID = MB_Ekleyen
                outer apply (select PRLPRICE from PRICELIST prl WITH (NOLOCK) where prl.PRLPROID = islem.MB_PROID and PRLDPRID = 740) as pesinfiyat
                where MB_Tamamlandi = 1 and islem.MB_SALID != 0 and SALDIVISON in ({0}) 
				and MB_PlanTarih between '{1}' and '{2}'
				group by SALID,CURID,CURNAME,MB_PlanTarih,MB_TamamlanmaTarihi,OFFCURNAME
                union
				select STRING_AGG(SALID, ','),CURID,CURNAME,MB_PlanTarih,MB_TamamlanmaTarihi,OFFCURNAME,sum(PRLPRICE) as PRLPRICE,sum(Hakedis) as Hakedis from (
                select MB_ORDCHID as SALID,MB_SALID as CURID,DIVNAME as CURNAME,MB_PlanTarih,Convert(Char(10),MB_TamamlanmaTarihi,121) as MB_TamamlanmaTarihi,OFFCURNAME,sum(PRLPRICE) as PRLPRICE,sum(PRLPRICE*0.08) as Hakedis from MDE_GENEL..MB_Islemler
                inner join PRODEMAND on PRDEID = MB_ORDCHID
                left outer join DIVISON on PRDEDIVISON = DIVVAL
                left outer join OFFICALCUR on OFFCURID = MB_Ekleyen
                outer apply (select PRLPRICE from PRICELIST prl WITH (NOLOCK) where prl.PRLPROID = MB_PROID and PRLDPRID = 740) as pesinfiyat
                where MB_Tamamlandi = 1 and DIVVAL in ({0}) 
				and MB_PlanTarih between '{1}' and '{2}'
				group by MB_ORDCHID,MB_SALID,DIVNAME,MB_PlanTarih,MB_TamamlanmaTarihi,OFFCURNAME
				) son
				group by CURID,CURNAME,MB_PlanTarih,MB_TamamlanmaTarihi,OFFCURNAME
                order by MB_TamamlanmaTarihi", magaza,Startdate,Enddate);
                var dt = DbQuery.Query(q, ConnectionString);
                GridView1.DataSource = dt;
                GridView1.DataBind();
            }
        }
        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridView1.PageIndex = e.NewPageIndex;
            BindGrid(StartDate.Value.ToString(),EndDate.Value.ToString()); // Sayfa değiştiğinde verileri yeniden bağlayın
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
	            ,d.MB_SALID as CURNAME
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
            uploadarea.Visible = false;
        }
        protected void table_SelectedIndexChanged(object sender, EventArgs e)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "showLoading", "showLoading();", true);
            // Retrieve the URL from ViewState
            var CURNAME = table.SelectedRow.Cells[2].Text;
            var Filename = table.SelectedRow.Cells[4].Text;
            var FileType = table.SelectedRow.Cells[3].Text.ToUpper();
            string uploadedFileUrl = "";
            if (CURID != "0")
            {
                uploadedFileUrl = ftpUrl + "/" + CURID + "/Mobilya Montaj/" + FileType + "/" + Filename;
            }
            else
            {
                uploadedFileUrl = ftpUrl + "/" + CURID + "/Mobilya Montaj/" + FileType + "/" + CURNAME + "/" + Filename;
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
        public bool FTPCechkFolder(string folderPath)
        {
            var ftp = (List<Ftp>)Session["FTP"];
            if (ftp != null)
            {
                ftpUrl = ftp[0].VolFtpHost;
                ftpUsername = ftp[0].VolFtpUser;
                ftpPassword = ftp[0].VolFtpPass;
            }
            string server = ftpUrl;//"ftp.example.com"; // FTP sunucu adresi
            string username = ftpUsername; //"Yon"; // FTP kullanıcı adı
            string password = ftpPassword;//"Yonavm123"; // FTP şifre

            try
            {
                // FTP sunucusuna bağlan
                FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create($"{server}/{folderPath}");
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                ftpRequest.Credentials = new NetworkCredential(username, password);

                using (FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse())
                {
                    using (Stream responseStream = ftpResponse.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            // Sunucuda belirtilen klasörün dosya listesini al
                            string fileList = reader.ReadToEnd();

                            // Klasörün var olup olmadığını kontrol et
                            bool folderExists = !string.IsNullOrEmpty(fileList);

                            // Sonucu kullanıcıya göster
                            if (folderExists)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            catch (WebException)
            {
                return false;
            }
        }
        public void CreateFolderFTP(string FileNAme)
        {
            try
            {
                string path = FileNAme;
                string xmlPath = ftpUrl + "/" + path;
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(xmlPath);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            }
            catch (Exception ex)
            {
            }
        }

        DataTable dosyalar = new DataTable();
        protected void btnUpload_Click(object sender, EventArgs e)
        {
            var _curid = CURID;
            var _salid = SALID;
            var _proid = PROID;
            var loginRes = (List<LoginObj>)Session["Login"];
            string strFileName;
            string strFilePath;
            string strFolder;
            string ftpFolder;
            string ftpFilePath;
            strFolder = Server.MapPath("./UploadedFiles/Mobilya Kurulum/TAMAMLANAN/");
            // Get the name of the file that is posted.
            strFileName = oFile.PostedFile.FileName;
            //strFileName = Path.GetFileName(strFileName);
            if (oFile.Value != "")
            {
                //if (!Directory.Exists(Server.MapPath("./UploadedFiles/Mobilya Kurulum/" + CURID)))
                //{
                //    Directory.CreateDirectory(Server.MapPath("./UploadedFiles/Mobilya Kurulum/" + CURID));
                //}
                // Create the directory if it does not exist.
                if (!Directory.Exists(strFolder))
                {
                    Directory.CreateDirectory(strFolder);
                }
                // Save the uploaded file to the server.
                strFilePath = strFolder + strFileName;
                if (File.Exists(strFilePath))
                {
                    lblUploadResult.Text = strFileName + " Bu Dosya Daha Önce Yüklenmiş.....!";
                }
                else
                {
                    oFile.PostedFile.SaveAs(strFilePath);
                    try
                    {
                        ftpFolder = Server.MapPath("./UploadedFiles/Mobilya Kurulum/" + CURID + "/" + SALID + "/FTPGiden/");
                        ftpFilePath = ftpFolder + strFileName;
                        if (!Directory.Exists(ftpFolder))
                        {
                            Directory.CreateDirectory(ftpFolder);
                        }
                        if (!File.Exists(ftpFilePath))
                            oFile.PostedFile.SaveAs(ftpFilePath);

                        if (!FTPCechkFolder(CURID))
                        {
                            CreateFolderFTP(CURID);
                            if (!FTPCechkFolder(CURID + "/Mobilya Montaj"))
                            {
                                CreateFolderFTP(CURID + "/Mobilya Montaj");
                            }
                            if (!FTPCechkFolder(CURID + "/Mobilya Montaj/TAMAMLANAN"))
                            {
                                CreateFolderFTP(CURID + "/Mobilya Montaj/TAMAMLANAN");
                            }
                        }
                        else
                        {
                            if (!FTPCechkFolder(CURID + "/Mobilya Montaj"))
                            {
                                CreateFolderFTP(CURID + "/Mobilya Montaj");
                            }
                            if (!FTPCechkFolder(CURID + "/Mobilya Montaj/TAMAMLANAN"))
                            {
                                CreateFolderFTP(CURID + "/Mobilya Montaj/TAMAMLANAN");
                            }
                        }
                        if (CURID == "0")
                        {
                            if (!FTPCechkFolder(CURID + "/Mobilya Montaj/TAMAMLANAN/" + SALID))
                            {
                                CreateFolderFTP(CURID + "/Mobilya Montaj/TAMAMLANAN/" + SALID);
                            }
                            string sira = "";
                            string qq = String.Format(@"select COUNT(*) as adet
                            from MDE_GENEL.dbo.MB_BayiDosyaları d
                            left outer join MDE_GENEL.dbo.MB_DosyaTipi t on t.id = MB_FileType
                            left outer join VDB_YON01.dbo.CURRENTS c on c.CURID = d.MB_CURID
                            where d.MB_CURID = {0} and MB_SALID = {1} and t.id = 1", CURID, SALID);
                            dosyalar = DbQuery.Query(qq, ConnectionString2);
                            if (dosyalar == null)
                            {
                                sira = "0";
                            }
                            else
                            {
                                if (dosyalar.Rows[0][0].ToString() == "1")
                                {
                                    sira = "1";
                                }
                                else
                                {
                                    sira = (int.Parse(dosyalar.Rows[0]["adet"].ToString())).ToString();
                                }
                            }
                            //var PROID = GridView1.Rows[0].Cells[4].Text.ToString();
                            string newFileName = PROID + "_" + sira + Path.GetExtension(strFileName); // Örneğin "newFileName.ext"

                            string newFilePath = Path.Combine(ftpFolder, newFileName);

                            // Rename the file
                            File.Move(ftpFilePath, newFilePath);
                            // Upload the file to FTP server
                            string ftpFullUrl = ftpUrl + "/" + CURID + "/Mobilya Montaj/TAMAMLANAN/" + SALID + "/" + newFileName;
                            FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(ftpFullUrl);
                            ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
                            ftpRequest.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

                            byte[] fileContents = File.ReadAllBytes(newFilePath);
                            ftpRequest.ContentLength = fileContents.Length;

                            using (Stream requestStream = ftpRequest.GetRequestStream())
                            {
                                requestStream.Write(fileContents, 0, fileContents.Length);
                            }

                            using (FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse())
                            {
                                string q = String.Format(@"insert into MDE_GENEL.dbo.MB_BayiDosyaları values ({0},{1},1,'{2}','{3}')", CURID, SALID, newFileName, loginRes[0].SOCODE);
                                DbQuery.insertquery(q, ConnectionString2);
                                lblUploadResult.Text = strFileName + " Dosya Kaydedildi";
                            }
                            // Delete the local file after upload
                            //File.Delete(newFilePath);
                        }
                        else
                        {

                            string sira = "";
                            string qq = String.Format(@"select COUNT(*) as adet
                            from MDE_GENEL.dbo.MB_BayiDosyaları d
                            left outer join MDE_GENEL.dbo.MB_DosyaTipi t on t.id = MB_FileType
                            left outer join VDB_YON01.dbo.CURRENTS c on c.CURID = d.MB_CURID
                            where d.MB_CURID = {0} and t.id = 1", CURID);
                            dosyalar = DbQuery.Query(qq, ConnectionString2);
                            if (dosyalar == null)
                            {
                                sira = "0";
                            }
                            else
                            {
                                if (dosyalar.Rows[0][0].ToString() == "1")
                                {
                                    sira = "1";
                                }
                                else
                                {
                                    sira = (int.Parse(dosyalar.Rows[0]["adet"].ToString())).ToString();
                                }
                            }
                            string newFileName = SALID + "_" + sira + Path.GetExtension(strFileName); // Örneğin "newFileName.ext"
                            string newFilePath = Path.Combine(ftpFolder, newFileName);

                            // Rename the file
                            File.Move(ftpFilePath, newFilePath);
                            // Upload the file to FTP server
                            string ftpFullUrl = ftpUrl + "/" + CURID + "/Mobilya Montaj/TAMAMLANAN/" + newFileName;
                            FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(ftpFullUrl);
                            ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
                            ftpRequest.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

                            byte[] fileContents = File.ReadAllBytes(newFilePath);
                            ftpRequest.ContentLength = fileContents.Length;

                            using (Stream requestStream = ftpRequest.GetRequestStream())
                            {
                                requestStream.Write(fileContents, 0, fileContents.Length);
                            }

                            using (FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse())
                            {
                                string q = String.Format(@"insert into MDE_GENEL.dbo.MB_BayiDosyaları values ({0},{1},1,'{2}','{3}')", CURID, SALID, newFileName, loginRes[0].SOCODE);
                                DbQuery.insertquery(q, ConnectionString2);
                                lblUploadResult.Text = strFileName + " Dosya Kaydedildi";
                            }
                            // Delete the local file after upload
                            File.Delete(newFilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        lblUploadResult.Text = lblUploadResult.Text + "\r\n" + ex.Message;
                    }
                }
            }
        }
        protected void btnUpload3_Click(object sender, EventArgs e)
        {
            var loginRes = (List<LoginObj>)Session["Login"];
            string strFileName;
            string strFilePath;
            string strFolder;
            string ftpFolder;
            string ftpFilePath;
            strFolder = Server.MapPath("./UploadedFiles/Mobilya Kurulum/SSH/");
            // Get the name of the file that is posted.
            strFileName = oFile3.PostedFile.FileName;
            //strFileName = Path.GetFileName(strFileName);
            if (oFile3.Value != "")
            {
                //if (!Directory.Exists(Server.MapPath("./UploadedFiles/Mobilya Kurulum/" + CURID)))
                //{
                //    Directory.CreateDirectory(Server.MapPath("./UploadedFiles/Mobilya Kurulum/" + CURID));
                //}
                // Create the directory if it does not exist.
                if (!Directory.Exists(strFolder))
                {
                    Directory.CreateDirectory(strFolder);
                }
                // Save the uploaded file to the server.
                strFilePath = strFolder + strFileName;
                if (File.Exists(strFilePath))
                {
                    lblUploadResult3.Text = strFileName + " Bu Dosya Daha Önce Yüklenmiş.....!";
                }
                else
                {
                    oFile3.PostedFile.SaveAs(strFilePath);


                    try
                    {
                        ftpFolder = Server.MapPath("./UploadedFiles/Mobilya Kurulum/" + CURID + "/" + SALID + "/FTPGiden/");
                        ftpFilePath = ftpFolder + strFileName;
                        if (!Directory.Exists(ftpFolder))
                        {
                            Directory.CreateDirectory(ftpFolder);
                        }
                        if (!File.Exists(ftpFilePath))
                            oFile.PostedFile.SaveAs(ftpFilePath);

                        if (!FTPCechkFolder(CURID))
                        {
                            CreateFolderFTP(CURID);
                            if (!FTPCechkFolder(CURID + "/Mobilya Montaj"))
                            {
                                CreateFolderFTP(CURID + "/Mobilya Montaj");
                            }
                            if (!FTPCechkFolder(CURID + "/Mobilya Montaj/SSH"))
                            {
                                CreateFolderFTP(CURID + "/Mobilya Montaj/SSH");
                            }
                        }
                        else
                        {
                            if (!FTPCechkFolder(CURID + "/Mobilya Montaj"))
                            {
                                CreateFolderFTP(CURID + "/Mobilya Montaj");
                            }
                            if (!FTPCechkFolder(CURID + "/Mobilya Montaj/SSH"))
                            {
                                CreateFolderFTP(CURID + "/Mobilya Montaj/SSH");
                            }
                        }
                        if (CURID == "0")
                        {
                            if (!FTPCechkFolder(CURID + "/Mobilya Montaj/SSH/" + SALID))
                            {
                                CreateFolderFTP(CURID + "/Mobilya Montaj/SSH/" + SALID);
                            }
                            string sira = "";
                            string qq = String.Format(@"select COUNT(*) as adet
                            from MDE_GENEL.dbo.MB_BayiDosyaları d
                            left outer join MDE_GENEL.dbo.MB_DosyaTipi t on t.id = MB_FileType
                            left outer join VDB_YON01.dbo.CURRENTS c on c.CURID = d.MB_CURID
                            where d.MB_CURID = {0} and MB_SALID = {1} and t.id = 2", CURID, SALID);
                            dosyalar = DbQuery.Query(qq, ConnectionString2);
                            if (dosyalar == null)
                            {
                                sira = "0";
                            }
                            else
                            {
                                if (dosyalar.Rows[0][0].ToString() == "1")
                                {
                                    sira = "1";
                                }
                                else
                                {
                                    sira = (int.Parse(dosyalar.Rows[0]["adet"].ToString())).ToString();
                                }
                            }
                            //var PROID = GridView1.Rows[0].Cells[4].Text.ToString();
                            string newFileName = PROID + "_" + sira + Path.GetExtension(strFileName); // Örneğin "newFileName.ext"

                            string newFilePath = Path.Combine(ftpFolder, newFileName);

                            // Rename the file
                            File.Move(ftpFilePath, newFilePath);
                            // Upload the file to FTP server
                            string ftpFullUrl = ftpUrl + "/" + CURID + "/Mobilya Montaj/SSH/" + SALID + "/" + newFileName;
                            FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(ftpFullUrl);
                            ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
                            ftpRequest.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

                            byte[] fileContents = File.ReadAllBytes(newFilePath);
                            ftpRequest.ContentLength = fileContents.Length;

                            using (Stream requestStream = ftpRequest.GetRequestStream())
                            {
                                requestStream.Write(fileContents, 0, fileContents.Length);
                            }

                            using (FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse())
                            {
                                string q = String.Format(@"insert into MDE_GENEL.dbo.MB_BayiDosyaları values ({0},{1},2,'{2}','{3}')", CURID, SALID, newFileName, loginRes[0].SOCODE);
                                DbQuery.insertquery(q, ConnectionString2);
                                lblUploadResult.Text = strFileName + " Dosya Kaydedildi";
                            }
                        }
                        else
                        {
                            string sira = "";
                            string qq = String.Format(@"select COUNT(*) as adet
                                from MDE_GENEL.dbo.MB_BayiDosyaları d
                                left outer join MDE_GENEL.dbo.MB_DosyaTipi t on t.id = MB_FileType
                                left outer join VDB_YON01.dbo.CURRENTS c on c.CURID = d.MB_CURID
                                where d.MB_CURID = {0} and t.id = 2", CURID);
                            dosyalar = DbQuery.Query(qq, ConnectionString2);
                            if (dosyalar == null)
                            {
                                sira = "0";
                            }
                            else
                            {
                                if (dosyalar.Rows.Count > 0)
                                {
                                    sira = (int.Parse(dosyalar.Rows[0]["adet"].ToString()) + 1).ToString();
                                }
                                else
                                {
                                    sira = "0";
                                }
                            }
                            string newFileName = SALID + "_" + sira + Path.GetExtension(strFileName); // Örneğin "newFileName.ext"
                            string newFilePath = Path.Combine(ftpFolder, newFileName);

                            // Rename the file
                            File.Move(ftpFilePath, newFilePath);
                            // Upload the file to FTP server
                            string ftpFullUrl = ftpUrl + "/" + CURID + "/Mobilya Montaj/SSH/" + newFileName;
                            FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(ftpFullUrl);
                            ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
                            ftpRequest.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

                            byte[] fileContents = File.ReadAllBytes(newFilePath);
                            ftpRequest.ContentLength = fileContents.Length;

                            using (Stream requestStream = ftpRequest.GetRequestStream())
                            {
                                requestStream.Write(fileContents, 0, fileContents.Length);
                            }

                            using (FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse())
                            {
                                string q = String.Format(@"insert into MDE_GENEL.dbo.MB_BayiDosyaları values ({0},{1},2,'{2}','{3}')", CURID, SALID, newFileName, loginRes[0].SOCODE);
                                DbQuery.insertquery(q, ConnectionString2);
                                lblUploadResult3.Text = strFileName + " dosyası kaydedildi";
                            }

                            // Delete the local file after upload
                            //File.Delete(newFilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        lblUploadResult3.Text = "Bir hata oluştu:" + ex.Message;
                    }
                }
            }
            else
            {
                lblUploadResult3.Text = "Yüklenecek dosyayı seçmek için 'Gözat'a tıklayın.";
            }
            // Display the result of the upload.
            frmConfirmation3.Visible = true;
        }

    }
}