using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using YonMobilya.Class;
using static YonMobilya.Class.Siniflar;

namespace YonMobilya
{
    public partial class Montaj : System.Web.UI.Page
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
        DataTable dosyalar = new DataTable();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var loginRes = (List<LoginObj>)Session["Login"];
                if (loginRes != null)
                {
                    string script = "$(document).ready(function () { $('[id*=btnSubmit]').click(); });";
                    ClientScript.RegisterStartupScript(this.GetType(), "load", script, true);
                    SALID = Request.QueryString["salid"];
                    CURID = Request.QueryString["curid"]; //DbQuery.GetValue($"select SALCURID from SALES where SALID = {SALID}");
                    var ftp = (List<Ftp>)Session["FTP"];
                    if (ftp != null)
                    {
                        ftpUrl = ftp[0].VolFtpHost;
                        ftpUsername = ftp[0].VolFtpUser;
                        ftpPassword = ftp[0].VolFtpPass;
                    }
                    //string imageUrl = "img/profile.jpg";
                    //profilepicture.Style["background-image"] = $"url('{ResolveUrl(imageUrl)}')";
                    BindGridView();
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
                string script = "$(document).ready(function () { $('[id*=btnSubmit]').click(); });";
                ClientScript.RegisterStartupScript(this.GetType(), "load", script, true);
            }
        }
        private void BindGridView()
        {
            string query = "";
            if (CURID != "0")
            {
                query = String.Format(@"select distinct CDRSALID,ORDCHID,PROID, PROVAL,PRONAME,Convert(int,ORDCHBALANCEQUAN) as ORDCHBALANCEQUAN,Convert(numeric(18,2),ORDCHBALANCEQUAN*PRLPRICE) as PRLPRICE
			     ,TESLIM.DIVNAME
                FROM MDE_GENEL.dbo.MB_Islemler 
                left outer join CUSDELIVER on MB_SALID = CDRSALID and CDRORDCHID = MB_ORDCHID
                left outer join CURRENTS on CURID = CDRCURID
                left outer join ORDERSCHILD on ORDCHID = CDRORDCHID
                left outer join PRODUCTS on PROID = ORDCHPROID
                left outer join WAVEPRODUCTS on WPROID = PROID and WPROUNIQ = 4 and WPROVAL in ('DD','EE','YY')
                left outer join DEFSTORAGE WITH (NOLOCK) ON CDRSTORID = DSTORID
                left outer join DIVISON TESLIM WITH (NOLOCK) ON TESLIM.DIVVAL = DSTORVAL AND ORDCHCOMPANY = TESLIM.DIVCOMPANY
                outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = PROID and PRLDPRID = 740) as pesinfiyat
                where CDRSALID =  '{0}' and ORDCHBALANCEQUAN >= ORDCHQUAN
			    and MB_Tamamlandi = 0        
                order by 1 desc", SALID);
            }
            else
            {
                query = String.Format(@"select 0 as CDRSALID,PRDEID as ORDCHID,PRDEPROID,PROVAL,PRONAME,Convert(int,PRDEQUAN) as ORDCHBALANCEQUAN,Convert(numeric(18,2),PRDEQUAN*PRLPRICE) as PRLPRICE
				FROM MDE_GENEL.dbo.MB_Islemler
				left outer join PRODEMAND on PRDEID = MB_ORDCHID
				left outer join PRODUCTS on PROID = MB_PROID
                outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = PRDEPROID and PRLDPRID = 740) as pesinfiyat
				where MB_Tamamlandi = 0 and MB_SUPCURVAL = 'T003387' and PRDEKIND= 1 and PRDESTS = 0
				and PRDEID = '{0}'
			    and MB_Tamamlandi = 0        
                order by 1 desc", SALID);
            }
            var dt = DbQuery.Query(query, ConnectionString);
            if (dt != null)
            {
                grid.DataSource = dt;
                grid.DataBind();
            }
            else
            {
                tamlandı.Visible = false;
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

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            string strFileName;
            string strFilePath;
            string strFolder;
            string ftpFolder;
            string ftpFilePath;
            strFolder = Server.MapPath("./UploadedFiles/" + CURID + "/Mobilya Kurulum/");
            // Get the name of the file that is posted.
            strFileName = oFile.PostedFile.FileName;
            //strFileName = Path.GetFileName(strFileName);
            if (oFile.Value != "")
            {
                if (!Directory.Exists(Server.MapPath("./UploadedFiles/" + CURID)))
                {
                    Directory.CreateDirectory(Server.MapPath("./UploadedFiles/" + CURID));
                }
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
                    lblUploadResult.Text = strFileName + " Dosya Kaydedildi";
                }

                try
                {
                    ftpFolder = Server.MapPath("./UploadedFiles/" + CURID + "/FTPGiden/");
                    ftpFilePath = ftpFolder + strFileName;
                    CreateFolderFTP(CURID);
                    CreateFolderFTP(CURID + "/Mobilya Montaj");
                    if (!Directory.Exists(ftpFolder))
                    {
                        Directory.CreateDirectory(ftpFolder);
                    }
                    if (!File.Exists(ftpFilePath))
                        oFile.PostedFile.SaveAs(ftpFilePath);
                    string sira = "";
                    string qq = String.Format(@"select FileTypeName,COUNT(*) as adet
                    from KrediPuan.dbo.MusteriBayiDosyaları d
                    left outer join MDE_GENEL.dbo.MB_DosyaTipi t on t.id = [FileType]
                    left outer join VDB_OTOBIL01.dbo.CURRENTS c on c.CURID = d.CURID
                    where d.CURID = {0}
                    group by FileTypeName,[FileType]
                    order by [FileType] ", CURID);
                    dosyalar = DbQuery.Query(qq, ConnectionString2);
                    if (dosyalar == null)
                    {
                        sira = "";
                    }
                    else
                    {
                        if (dosyalar.Rows.Count > 0)
                        {
                            for (int i = 0; i < dosyalar.Rows.Count; i++)
                            {
                                if (dosyalar.Rows[i]["FileTypeName"].ToString() == "RUHSAT")
                                {
                                    sira = (int.Parse(dosyalar.Rows[i]["adet"].ToString()) + 1).ToString();
                                }
                            }
                        }
                    }
                    string newFileName = "Ruhsat" + sira + Path.GetExtension(strFileName); // Örneğin "newFileName.ext"
                    string newFilePath = Path.Combine(ftpFolder, newFileName);

                    // Rename the file
                    File.Move(ftpFilePath, newFilePath);
                    // Upload the file to FTP server
                    string ftpFullUrl = ftpUrl + "/" + CURID + "/Bayi Dosyaları/" + newFileName;
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
                        string q = String.Format(@"insert into KrediPuan.dbo.MusteriBayiDosyaları values ({0},1,'{1}')", CURID, newFileName);
                        DbQuery.insertquery(q, ConnectionString2);
                    }
                    // Delete the local file after upload
                }
                catch (Exception ex)
                {
                    lblUploadResult.Text = lblUploadResult.Text + "\r\n" + ex.Message;
                }
            }
            else
            {
                lblUploadResult.Text = "Yüklenecek dosyayı seçmek için 'Gözat'a tıklayın.";
            }
            // Display the result of the upload.
            //File.Delete(ftpFilePath);
            frmConfirmation.Visible = true;
        }

        protected void btnUpload2_Click(object sender, EventArgs e)
        {
            string strFileName;
            string strFilePath;
            string strFolder;
            string ftpFolder;
            string ftpFilePath;
            strFolder = Server.MapPath("./UploadedFiles/" + CURID + "/BayiYüklenen/");
            // Get the name of the file that is posted.
            strFileName = oFile2.PostedFile.FileName;
            //strFileName = Path.GetFileName(strFileName);
            if (oFile2.Value != "")
            {
                if (!Directory.Exists(Server.MapPath("./UploadedFiles/" + CURID)))
                {
                    Directory.CreateDirectory(Server.MapPath("./UploadedFiles/" + CURID));
                }
                // Create the directory if it does not exist.
                if (!Directory.Exists(strFolder))
                {
                    Directory.CreateDirectory(strFolder);
                }
                // Save the uploaded file to the server.
                strFilePath = strFolder + strFileName;
                if (File.Exists(strFilePath))
                {
                    lblUploadResult2.Text = strFileName + " Bu Dosya Daha Önce Yüklenmiş.....!";
                }
                else
                {
                    oFile2.PostedFile.SaveAs(strFilePath);
                    lblUploadResult2.Text = strFileName + " dosyası Tramer olarak kaydedildi";


                    try
                    {
                        ftpFolder = Server.MapPath("./UploadedFiles/" + CURID + "/FTPGiden/");
                        ftpFilePath = ftpFolder + strFileName;
                        CreateFolderFTP(CURID);
                        CreateFolderFTP(CURID + "/Bayi Dosyaları");
                        if (!Directory.Exists(ftpFolder))
                        {
                            Directory.CreateDirectory(ftpFolder);
                        }
                        if (!File.Exists(ftpFilePath))
                            oFile2.PostedFile.SaveAs(ftpFilePath);

                        string sira = "";
                        string qq = String.Format(@"select FileTypeName,COUNT(*) as adet
                        from KrediPuan.dbo.MusteriBayiDosyaları d
                        left outer join KrediPuan.dbo.KrediPuan_DosyaTipi t on t.id = [FileType]
                        left outer join VDB_OTOBIL01.dbo.CURRENTS c on c.CURID = d.CURID
                        where d.CURID = {0}
                        group by FileTypeName,[FileType]
                        order by [FileType] ", CURID);
                        dosyalar = DbQuery.Query(qq, ConnectionString2);
                        if (dosyalar == null)
                        {
                            sira = "";
                        }
                        else
                        {
                            if (dosyalar.Rows.Count > 0)
                            {
                                for (int i = 0; i < dosyalar.Rows.Count; i++)
                                {
                                    if (dosyalar.Rows[i]["FileTypeName"].ToString() == "TRAMER")
                                    {
                                        sira = (int.Parse(dosyalar.Rows[i]["adet"].ToString()) + 1).ToString();
                                    }
                                }
                            }
                        }
                        string newFileName = "Tramer" + sira + Path.GetExtension(strFileName); // Örneğin "newFileName.ext"
                        string newFilePath = Path.Combine(ftpFolder, newFileName);

                        // Rename the file
                        File.Move(ftpFilePath, newFilePath);
                        // Upload the file to FTP server
                        string ftpFullUrl = ftpUrl + "/" + CURID + "/Bayi Dosyaları/" + newFileName;
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
                            string q = String.Format(@"insert into KrediPuan.dbo.MusteriBayiDosyaları values ({0},2,'{1}')", CURID, newFileName);
                            DbQuery.insertquery(q, ConnectionString2);
                        }

                        // Delete the local file after upload
                        //File.Delete(newFilePath);
                    }
                    catch (Exception ex)
                    {
                        lblUploadResult2.Text = "Bir hata oluştu:" + ex.Message;
                    }
                }
            }
            else
            {
                lblUploadResult2.Text = "Yüklenecek dosyayı seçmek için 'Gözat'a tıklayın.";
            }
            // Display the result of the upload.
            frmConfirmation2.Visible = true;
        }

        protected void btnUpload3_Click(object sender, EventArgs e)
        {
            string strFileName;
            string strFilePath;
            string strFolder;
            string ftpFolder;
            string ftpFilePath;
            strFolder = Server.MapPath("./UploadedFiles/" + CURID + "/");
            // Get the name of the file that is posted.
            strFileName = oFile3.PostedFile.FileName;
            //strFileName = Path.GetFileName(strFileName);
            if (oFile3.Value != "")
            {
                if (!Directory.Exists(Server.MapPath("./UploadedFiles/" + CURID)))
                {
                    Directory.CreateDirectory(Server.MapPath("./UploadedFiles/" + CURID));
                }
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

                    // Rename the file
                    lblUploadResult3.Text = strFileName + " dosyası Kimlik olarak kaydedildi";
                }
                try
                {
                    ftpFolder = Server.MapPath("./UploadedFiles/" + CURID + "/FTPGiden/");
                    ftpFilePath = ftpFolder + strFileName;
                    CreateFolderFTP(CURID);
                    CreateFolderFTP(CURID + "/Bayi Dosyaları");
                    if (!Directory.Exists(ftpFolder))
                    {
                        Directory.CreateDirectory(ftpFolder);
                    }
                    if (!File.Exists(ftpFilePath))
                        oFile3.PostedFile.SaveAs(ftpFilePath);

                    string sira = "";
                    string qq = String.Format(@"select FileTypeName,COUNT(*) as adet
                    from KrediPuan.dbo.MusteriBayiDosyaları d
                    left outer join KrediPuan.dbo.KrediPuan_DosyaTipi t on t.id = [FileType]
                    left outer join VDB_OTOBIL01.dbo.CURRENTS c on c.CURID = d.CURID
                    where d.CURID = {0}
                    group by FileTypeName,[FileType]
                    order by [FileType] ", CURID);
                    dosyalar = DbQuery.Query(qq, ConnectionString2);
                    if (dosyalar == null)
                    {
                        sira = "";
                    }
                    else
                    {
                        if (dosyalar.Rows.Count > 0)
                        {
                            for (int i = 0; i < dosyalar.Rows.Count; i++)
                            {
                                if (dosyalar.Rows[i]["FileTypeName"].ToString() == "KİMLİK")
                                {
                                    sira = (int.Parse(dosyalar.Rows[i]["adet"].ToString()) + 1).ToString();
                                }
                            }
                        }
                    }
                    string newFileName = "Kimlik" + sira + Path.GetExtension(strFileName); // Örneğin "newFileName.ext"
                    string newFilePath = Path.Combine(ftpFolder, newFileName);

                    // Rename the file
                    File.Move(ftpFilePath, newFilePath);
                    // Upload the file to FTP server
                    string ftpFullUrl = ftpUrl + "/" + CURID + "/Bayi Dosyaları/" + newFileName;
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
                        string q = String.Format(@"insert into KrediPuan.dbo.MusteriBayiDosyaları values ({0},3,'{1}')", CURID, newFileName);
                        DbQuery.insertquery(q, ConnectionString2);
                    }

                    // Delete the local file after upload
                    //File.Delete(newFilePath);
                }
                catch (Exception ex)
                {
                    lblUploadResult3.Text = "Bir hata oluştu:" + ex.Message;
                }
            }
            else
            {
                lblUploadResult3.Text = "Yüklenecek dosyayı seçmek için 'Gözat'a tıklayın.";
            }
            // Display the result of the upload.
            frmConfirmation3.Visible = true;
        }

        protected void btnUpload4_Click(object sender, EventArgs e)
        {
            string strFileName;
            string strFilePath;
            string strFolder;
            string ftpFolder;
            string ftpFilePath;
            strFolder = Server.MapPath("./UploadedFiles/" + CURID + "/");
            // Get the name of the file that is posted.
            strFileName = oFile4.PostedFile.FileName;
            strFileName = Path.GetFileName(strFileName);
            if (oFile4.Value != "")
            {
                if (!Directory.Exists(Server.MapPath("./UploadedFiles/" + CURID)))
                {
                    Directory.CreateDirectory(Server.MapPath("./UploadedFiles/" + CURID));
                }
                // Create the directory if it does not exist.
                if (!Directory.Exists(strFolder))
                {
                    Directory.CreateDirectory(strFolder);
                }
                // Save the uploaded file to the server.
                strFilePath = strFolder + strFileName;
                if (File.Exists(strFilePath))
                {
                    lblUploadResult4.Text = strFileName + " Bu Dosya Daha Önce Yüklenmiş.....!";
                }
                else
                {
                    oFile4.PostedFile.SaveAs(strFilePath);

                    // Rename the file
                    lblUploadResult4.Text = strFileName + " dosyası EkBelge olarak kaydedildi";
                }
                try
                {
                    ftpFolder = Server.MapPath("./UploadedFiles/" + CURID + "/FTPGiden/");
                    ftpFilePath = ftpFolder + strFileName;
                    CreateFolderFTP(CURID);
                    CreateFolderFTP(CURID + "/Bayi Dosyaları");
                    if (!Directory.Exists(ftpFolder))
                    {
                        Directory.CreateDirectory(ftpFolder);
                    }
                    if (!File.Exists(ftpFilePath))
                        oFile4.PostedFile.SaveAs(ftpFilePath);

                    string sira = "";
                    string qq = String.Format(@"select FileTypeName,COUNT(*) as adet
                    from KrediPuan.dbo.MusteriBayiDosyaları d
                    left outer join KrediPuan.dbo.KrediPuan_DosyaTipi t on t.id = [FileType]
                    left outer join VDB_OTOBIL01.dbo.CURRENTS c on c.CURID = d.CURID
                    where d.CURID = {0}
                    group by FileTypeName,[FileType]
                    order by [FileType] ", CURID);
                    dosyalar = DbQuery.Query(qq, ConnectionString2); if (dosyalar == null)
                    {
                        sira = "";
                    }
                    else
                    {
                        if (dosyalar.Rows.Count > 0)
                        {
                            for (int i = 0; i < dosyalar.Rows.Count; i++)
                            {
                                if (dosyalar.Rows[i]["FileTypeName"].ToString() == "EKDOSYA")
                                {
                                    sira = (int.Parse(dosyalar.Rows[i]["adet"].ToString()) + 1).ToString();
                                }
                            }
                        }
                    }
                    string newFileName = "EkBelge" + sira + Path.GetExtension(strFileName); // Örneğin "newFileName.ext"
                    string newFilePath = Path.Combine(ftpFolder, newFileName);

                    // Rename the file
                    File.Move(ftpFilePath, newFilePath);
                    // Upload the file to FTP server
                    string ftpFullUrl = ftpUrl + "/" + CURID + "/Bayi Dosyaları/" + newFileName;
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
                        string q = String.Format(@"insert into KrediPuan.dbo.MusteriBayiDosyaları values ({0},4,'{1}')", CURID, newFileName);
                        DbQuery.insertquery(q, ConnectionString2);
                    }

                    // Delete the local file after upload
                    //File.Delete(newFilePath);
                }
                catch (Exception ex)
                {
                    lblUploadResult4.Text = "Bir hata oluştu:" + ex.Message;
                }
            }
            else
            {
                lblUploadResult4.Text = "Yüklenecek dosyayı seçmek için 'Gözat'a tıklayın.";
            }
            // Display the result of the upload.
            frmConfirmation4.Visible = true;
        }

        protected void grid_SelectedIndexChanged(object sender, EventArgs e)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "showLoading", "showLoading();", true);
            // Retrieve the URL from ViewState
            var Filename = grid.SelectedRow.Cells[4].Text;
            string uploadedFileUrl = ftpUrl + "/" + CURID + "/Bayi Dosyaları/" + Filename;
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
                else if (Filename.EndsWith("jpg") || Filename.EndsWith("jpeg"))
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

        protected void grid_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Footer)
            {
                // Toplamları hesapla
                int totalAdet = 0;
                for (int i = 0; i < grid.Rows.Count; i++)
                {
                    GridViewRow row = grid.Rows[i];
                    var ss = row.Cells[4].Text;
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

        protected void grid_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.Cells.Count > 1)
            {
                var ss = e.Row.RowType;
                e.Row.Cells[1].Visible = false;
                e.Row.Cells[4].Visible = false;
            }
        }

        protected void grid_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            //Yeni sayfa numarasını ayarla
            grid.PageIndex = e.NewPageIndex;
            BindGridView();
        }

        protected void grid_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Select")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                GridViewRow row = grid.Rows[rowIndex];
                bool isChecked = ((CheckBox)row.FindControl("chkSelect")).Checked;

                if (isChecked)
                {
                    // Seçili satırın bilgilerini işleme
                    string proval = row.Cells[1].Text; // Ürün Kodu
                    string proname = row.Cells[2].Text; // Ürün Adı
                }
            }
        }

        protected void tamlandı_Click(object sender, EventArgs e)
        {
            bool tamam = true;
            for (int i = 0; i < grid.Rows.Count; i++)
            {
                GridViewRow row = grid.Rows[i];
                DropDownList ddl = (DropDownList)row.FindControl("chkSelect");
                if (ddl == null || ddl.SelectedValue == "0")
                {
                    tamam = false;
                }
            }
            if (tamam)
            {
                uploadarea.Visible = true;
                grid.Visible = false;
                tamlandı.Visible = false;
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showalert", "alert('Tüm Ürünlerde Sonuç Seçimi Yapınız...');", true);
            }
        }
    }
}