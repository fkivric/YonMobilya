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
                    Dosyalar();
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
                    if (CURID == "0")
                    {
                        //var adet = resimadet.Attributes["title"].ToString();
                        resimadet.Attributes["title"] = "1";
                        resimadet.InnerText = "Mağaza Montajı için 1 Resim Yeterli";
                    }
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
                query = String.Format(@"select distinct CDRSALID,ORDCHID,PROID, PROVAL,PRONAME,Convert(int,ORDCHQUAN) as ORDCHBALANCEQUAN,Convert(numeric(18,2),ORDCHQUAN*PRLPRICE) as PRLPRICE
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
                where CDRSALID =  '{0}' AND CDRSHIPVAL = 'ANTMOB' --and ORDCHBALANCEQUAN >= ORDCHQUAN
			    and MB_Tamamlandi = 0  and CDRBASECANID is NULL
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
                uploadarea.Visible = true;
                Kaydet.Visible = true;
            }
        }
        internal void Dosyalar()
        {
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
                where d.MB_CURID = {0} and MB_SALID = {1}
                order by MB_FileType", CURID,SALID);
                var dt = DbQuery.Query(q, ConnectionString);
                if (dt != null)
                {
                    FileLoad.Visible = true;
                    table.DataSource = dt;
                    table.DataBind();
                }

            }
        }
        public bool FTPCechkFolder(string folderPath)
        {
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

        protected void btnUpload_Click(object sender, EventArgs e)
        {
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
                            where d.MB_CURID = {0} and MB_SALID = {1} and t.id = 1", CURID,SALID);
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
                            var PROID  = grid.Rows[0].Cells[2].Text.ToString();
                            string newFileName = PROID + "_" + sira + Path.GetExtension(strFileName); // Örneğin "newFileName.ext"

                            string newFilePath = Path.Combine(ftpFolder, newFileName);

                            // Rename the file
                            File.Move(ftpFilePath, newFilePath);
                            // Upload the file to FTP server
                            string ftpFullUrl = ftpUrl + "/" + CURID + "/Mobilya Montaj/TAMAMLANAN/" + SALID +"/"+ newFileName;
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
            else
            {
                lblUploadResult.Text = "Yüklenecek dosyayı seçmek için 'Gözat'a tıklayın.";
            }
            // Display the result of the upload.
            //File.Delete(ftpFilePath);
            frmConfirmation.Visible = true;
            FileLoad.Visible = true;
            Dosyalar();
        }

        //protected void btnUpload2_Click(object sender, EventArgs e)
        //{
        //    string strFileName;
        //    string strFilePath;
        //    string strFolder;
        //    string ftpFolder;
        //    string ftpFilePath;
        //    strFolder = Server.MapPath("./UploadedFiles/" + CURID + "/BayiYüklenen/");
        //    // Get the name of the file that is posted.
        //    strFileName = oFile2.PostedFile.FileName;
        //    //strFileName = Path.GetFileName(strFileName);
        //    if (oFile2.Value != "")
        //    {
        //        if (!Directory.Exists(Server.MapPath("./UploadedFiles/" + CURID)))
        //        {
        //            Directory.CreateDirectory(Server.MapPath("./UploadedFiles/" + CURID));
        //        }
        //        // Create the directory if it does not exist.
        //        if (!Directory.Exists(strFolder))
        //        {
        //            Directory.CreateDirectory(strFolder);
        //        }
        //        // Save the uploaded file to the server.
        //        strFilePath = strFolder + strFileName;
        //        if (File.Exists(strFilePath))
        //        {
        //            lblUploadResult2.Text = strFileName + " Bu Dosya Daha Önce Yüklenmiş.....!";
        //        }
        //        else
        //        {
        //            oFile2.PostedFile.SaveAs(strFilePath);
        //            lblUploadResult2.Text = strFileName + " dosyası Tramer olarak kaydedildi";


        //            try
        //            {
        //                ftpFolder = Server.MapPath("./UploadedFiles/" + CURID + "/FTPGiden/");
        //                ftpFilePath = ftpFolder + strFileName;
        //                CreateFolderFTP(CURID);
        //                CreateFolderFTP(CURID + "/Bayi Dosyaları");
        //                if (!Directory.Exists(ftpFolder))
        //                {
        //                    Directory.CreateDirectory(ftpFolder);
        //                }
        //                if (!File.Exists(ftpFilePath))
        //                    oFile2.PostedFile.SaveAs(ftpFilePath);

        //                string sira = "";
        //                string qq = String.Format(@"select FileTypeName,COUNT(*) as adet
        //                from KrediPuan.dbo.MusteriBayiDosyaları d
        //                left outer join KrediPuan.dbo.KrediPuan_DosyaTipi t on t.id = [FileType]
        //                left outer join VDB_OTOBIL01.dbo.CURRENTS c on c.CURID = d.CURID
        //                where d.CURID = {0}
        //                group by FileTypeName,[FileType]
        //                order by [FileType] ", CURID);
        //                dosyalar = DbQuery.Query(qq, ConnectionString2);
        //                if (dosyalar == null)
        //                {
        //                    sira = "";
        //                }
        //                else
        //                {
        //                    if (dosyalar.Rows.Count > 0)
        //                    {
        //                        for (int i = 0; i < dosyalar.Rows.Count; i++)
        //                        {
        //                            if (dosyalar.Rows[i]["FileTypeName"].ToString() == "Tamamlanan")
        //                            {
        //                                sira = (int.Parse(dosyalar.Rows[i]["adet"].ToString()) + 1).ToString();
        //                            }
        //                        }
        //                    }
        //                }
        //                string newFileName = "Tamamlanan" + sira + Path.GetExtension(strFileName); // Örneğin "newFileName.ext"
        //                string newFilePath = Path.Combine(ftpFolder, newFileName);

        //                // Rename the file
        //                File.Move(ftpFilePath, newFilePath);
        //                // Upload the file to FTP server
        //                string ftpFullUrl = ftpUrl + "/" + CURID + "/Bayi Dosyaları/" + newFileName;
        //                FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(ftpFullUrl);
        //                ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
        //                ftpRequest.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

        //                byte[] fileContents = File.ReadAllBytes(newFilePath);
        //                ftpRequest.ContentLength = fileContents.Length;

        //                using (Stream requestStream = ftpRequest.GetRequestStream())
        //                {
        //                    requestStream.Write(fileContents, 0, fileContents.Length);
        //                }

        //                using (FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse())
        //                {
        //                    string q = String.Format(@"insert into MDE_GENEL.dbo.MB_BayiDosyaları values ({0},{1},1,'{1}')", CURID, SALID, newFileName);
        //                    DbQuery.insertquery(q, ConnectionString2);
        //                }

        //                // Delete the local file after upload
        //                //File.Delete(newFilePath);
        //            }
        //            catch (Exception ex)
        //            {
        //                lblUploadResult2.Text = "Bir hata oluştu:" + ex.Message;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        lblUploadResult2.Text = "Yüklenecek dosyayı seçmek için 'Gözat'a tıklayın.";
        //    }
        //    // Display the result of the upload.
        //    frmConfirmation2.Visible = true;
        //}

        protected void btnUpload3_Click(object sender, EventArgs e)
        {
            var loginRes = (List<LoginObj>)Session["Login"];
            string strFileName;
            string strFilePath;
            string strFolder;
            string ftpFolder;
            string ftpFilePath;
            strFolder = Server.MapPath("./UploadedFiles/Mobilya Kurulum/" + CURID + "/" + SALID + "/SSH/");
            // Get the name of the file that is posted.
            strFileName = oFile3.PostedFile.FileName;
            //strFileName = Path.GetFileName(strFileName);
            if (oFile3.Value != "")
            {
                if (!Directory.Exists(Server.MapPath("./UploadedFiles/Mobilya Kurulum/" + CURID)))
                {
                    Directory.CreateDirectory(Server.MapPath("./UploadedFiles/Mobilya Kurulum/" + CURID));
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

                    try
                    {
                        ftpFolder = Server.MapPath("./UploadedFiles/Mobilya Kurulum/" + CURID + "/FTPGiden/SSH/");
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
                            if (!FTPCechkFolder(CURID + "/Mobilya Montaj/FTPGiden"))
                            {
                                CreateFolderFTP(CURID + "/Mobilya Montaj/FTPGiden");
                            }
                            if (!FTPCechkFolder(CURID + "/Mobilya Montaj/FTPGiden/SSH"))
                            {
                                CreateFolderFTP(CURID + "/Mobilya Montaj/FTPGiden/SSH");
                            }
                        }
                        else
                        {
                            if (!FTPCechkFolder(CURID + "/Mobilya Montaj/FTPGiden"))
                            {
                                CreateFolderFTP(CURID + "/Mobilya Montaj/FTPGiden");
                            }
                            if (!FTPCechkFolder(CURID + "/Mobilya Montaj/FTPGiden/SSH"))
                            {
                                CreateFolderFTP(CURID + "/Mobilya Montaj/FTPGiden/SSH");
                            }
                        }
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

        //protected void btnUpload4_Click(object sender, EventArgs e)
        //{
        //    string strFileName;
        //    string strFilePath;
        //    string strFolder;
        //    string ftpFolder;
        //    string ftpFilePath;
        //    strFolder = Server.MapPath("./UploadedFiles/" + CURID + "/");
        //    // Get the name of the file that is posted.
        //    strFileName = oFile4.PostedFile.FileName;
        //    strFileName = Path.GetFileName(strFileName);
        //    if (oFile4.Value != "")
        //    {
        //        if (!Directory.Exists(Server.MapPath("./UploadedFiles/" + CURID)))
        //        {
        //            Directory.CreateDirectory(Server.MapPath("./UploadedFiles/" + CURID));
        //        }
        //        // Create the directory if it does not exist.
        //        if (!Directory.Exists(strFolder))
        //        {
        //            Directory.CreateDirectory(strFolder);
        //        }
        //        // Save the uploaded file to the server.
        //        strFilePath = strFolder + strFileName;
        //        if (File.Exists(strFilePath))
        //        {
        //            lblUploadResult4.Text = strFileName + " Bu Dosya Daha Önce Yüklenmiş.....!";
        //        }
        //        else
        //        {
        //            oFile4.PostedFile.SaveAs(strFilePath);

        //            // Rename the file
        //            lblUploadResult4.Text = strFileName + " dosyası EkBelge olarak kaydedildi";
        //        }
        //        try
        //        {
        //            ftpFolder = Server.MapPath("./UploadedFiles/" + CURID + "/FTPGiden/");
        //            ftpFilePath = ftpFolder + strFileName;
        //            CreateFolderFTP(CURID);
        //            CreateFolderFTP(CURID + "/Bayi Dosyaları");
        //            if (!Directory.Exists(ftpFolder))
        //            {
        //                Directory.CreateDirectory(ftpFolder);
        //            }
        //            if (!File.Exists(ftpFilePath))
        //                oFile4.PostedFile.SaveAs(ftpFilePath);

        //            string sira = "";
        //            string qq = String.Format(@"select FileTypeName,COUNT(*) as adet
        //            from KrediPuan.dbo.MusteriBayiDosyaları d
        //            left outer join KrediPuan.dbo.KrediPuan_DosyaTipi t on t.id = [FileType]
        //            left outer join VDB_OTOBIL01.dbo.CURRENTS c on c.CURID = d.CURID
        //            where d.CURID = {0}
        //            group by FileTypeName,[FileType]
        //            order by [FileType] ", CURID);
        //            dosyalar = DbQuery.Query(qq, ConnectionString2); if (dosyalar == null)
        //            {
        //                sira = "";
        //            }
        //            else
        //            {
        //                if (dosyalar.Rows.Count > 0)
        //                {
        //                    for (int i = 0; i < dosyalar.Rows.Count; i++)
        //                    {
        //                        if (dosyalar.Rows[i]["FileTypeName"].ToString() == "EkResimler")
        //                        {
        //                            sira = (int.Parse(dosyalar.Rows[i]["adet"].ToString()) + 1).ToString();
        //                        }
        //                    }
        //                }
        //            }
        //            string newFileName = "EkBelge" + sira + Path.GetExtension(strFileName); // Örneğin "newFileName.ext"
        //            string newFilePath = Path.Combine(ftpFolder, newFileName);

        //            // Rename the file
        //            File.Move(ftpFilePath, newFilePath);
        //            // Upload the file to FTP server
        //            string ftpFullUrl = ftpUrl + "/" + CURID + "/Bayi Dosyaları/" + newFileName;
        //            FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(ftpFullUrl);
        //            ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
        //            ftpRequest.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

        //            byte[] fileContents = File.ReadAllBytes(newFilePath);
        //            ftpRequest.ContentLength = fileContents.Length;

        //            using (Stream requestStream = ftpRequest.GetRequestStream())
        //            {
        //                requestStream.Write(fileContents, 0, fileContents.Length);
        //            }

        //            using (FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse())
        //            {
        //                string q = String.Format(@"insert into MDE_GENEL.dbo.MB_BayiDosyaları values ({0},{1},1,'{1}')", CURID, SALID, newFileName);
        //                DbQuery.insertquery(q, ConnectionString2);
        //            }

        //            // Delete the local file after upload
        //            //File.Delete(newFilePath);
        //        }
        //        catch (Exception ex)
        //        {
        //            lblUploadResult4.Text = "Bir hata oluştu:" + ex.Message;
        //        }
        //    }
        //    else
        //    {
        //        lblUploadResult4.Text = "Yüklenecek dosyayı seçmek için 'Gözat'a tıklayın.";
        //    }
        //    // Display the result of the upload.
        //    frmConfirmation4.Visible = true;
        //}

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
                if (ddl == null || ddl.SelectedValue == "")
                {
                    tamam = false;
                }
            }
            if (tamam)
            {
                uploadarea.Visible = true;
                grid.Enabled = false;
                tamlandı.Visible = false;
                Kaydet.Visible = true;
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showalert", "alert('Tüm Ürünlerde Sonuç Seçimi Yapınız...');", true);
            }
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
                else if (Filename.EndsWith("jpg") || Filename.EndsWith("jpeg") || Filename.EndsWith("png"))
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

        protected void Kaydet_Click(object sender, EventArgs e)
        {
            var loginRes = (List<LoginObj>)Session["Login"];
            if (loginRes != null)
            {
                if (table.Rows.Count >= int.Parse(resimadet.Attributes["title"].ToString()))
                {
                    foreach (GridViewRow row in grid.Rows)
                    {
                        DropDownList chkSelect = (DropDownList)row.FindControl("chkSelect");
                        var ss = chkSelect.SelectedValue;
                        var _ordchid = row.Cells[1].Text;
                        DbQuery.insertquery($"update MDE_GENEL.dbo.MB_Islemler set MB_Tamamlandi = 1,MB_TamamlanmaTarihi = getdate(),MB_Montajcı = '{loginRes[0].SOCODE.Replace("TT-","")}' where MB_ORDCHID = {_ordchid}", ConnectionString);
                        if (CURID == "0")
                        {
                            DbQuery.insertquery($"update PRODEMAND set PRDESTS = 2 where PRDEID = {_ordchid}", ConnectionString2);
                        }
                    }
                    WebMsgBox.Show("Kayıt Tamamlandı");
                    Response.Redirect("Takvim.aspx");
                }
                else
                {
                    WebMsgBox.Show("Lütfen en az 4 adet resim yükleyiniz");
                }
            }
        }
    }
}