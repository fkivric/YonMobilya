using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
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
        public static double Oran = 0;
        DataTable dosyalar = new DataTable();
        DataTable UrunSorgu = new DataTable();
        List<UrunReismleri> IDList = new List<UrunReismleri>();
        public static List<UrunReismleri> OnaylıListe = new List<UrunReismleri>();
        public class UrunReismleri
        {
            public string PRONAME { get; set; }
            public long PROID { get; set; }
        }
        HttpClient httpClient = new HttpClient();

        public static string SmsUrl = "https://restapi.ttmesaj.com/";
        public static string SmsToken = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var loginRes = (List<LoginObj>)Session["Login"];
                if (loginRes != null)
                {
                    Oran = double.Parse(loginRes[0].CURCHDISCRATE) / 100;
                    ScriptManager.RegisterStartupScript(this, GetType(), "showLoading", "showLoading();", true);

                    string[] salidArray = Request.QueryString["salid"].Split(',');

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
                    string Token = Session["Smstoken"] as string;
                    if (Token == "" || Token == null)
                    {
                        List<SmstokenObj> smstokenObjs = new List<SmstokenObj>();
                        SMSToken();
                        Session.Add("Smstoken", SmsToken);
                        SmstokenObj obj = new SmstokenObj();
                        obj.SmsUrl = SmsUrl;
                        obj.SmsUser = "yon.kara";
                        obj.SmsPassword = "N6K9L3A55";
                        obj.TokenSMS = SmsToken;
                        smstokenObjs.Add(obj);
                        Session.Add("SMS", smstokenObjs);
                        Session.Add("Smstoken", SmsToken);
                    }
                    //string imageUrl = "img/profile.jpg";
                    //profilepicture.Style["background-image"] = $"url('{ResolveUrl(imageUrl)}')";
                    BindGridView();
                    int adet = 0;
                    //if (CURID == "0" || CURID == "")
                    //{
                    if (UrunSorgu != null)
                    {
                        // 2. LINQ kullanarak gruplama işlemi yapın.
                        var groupedData = UrunSorgu.AsEnumerable()
                                        .GroupBy(row => row.Field<string>("PRONAME").Split(' ').Take(3).Aggregate((a, b) => a + " " + b))
                                        .Select((grp, index) => new
                                        {
                                            ProductName = grp.Key,
                                            TotalCount = grp.Count(),
                                            NewID = grp.First().Field<long>("ORDCHID"), // Gruptaki ilk öğenin ORDCHID değeri //UrunSorgu.Rows[index+1+ grp.Count()]["ORDCHID"].ToString() // Sıra numarası olarak grubun indeksi
                                        }).ToList();

                        // 3. Sonuçları yazdırma
                        foreach (var item in groupedData)
                        {
                            UrunReismleri urun = new UrunReismleri();
                            //Console.WriteLine($"Ürün Adı: {item.ProductName}, Toplam Adet: {item.TotalCount}, Yeni ID: {item.NewID}");
                            adet++;
                            urun.PRONAME = item.ProductName;
                            urun.PROID = item.NewID;
                            IDList.Add(urun);
                        }
                    }
                    //}
                    //else
                    //{
                    //    for (int i = 0; i < UrunSorgu.Rows.Count; i++)
                    //    {
                    //        UrunReismleri urun = new UrunReismleri(); 
                    //        adet++;
                    //        urun.PRONAME = UrunSorgu.Rows[i]["PRONAME"].ToString();
                    //        urun.PROID = long.Parse(UrunSorgu.Rows[i]["PROID"].ToString());
                    //        IDList.Add(urun);
                    //    }
                    //}
                    Urunler.DataValueField = "PROID";
                    Urunler.DataTextField = "PRONAME";
                    Urunler.DataSource=IDList;
                    Urunler.DataBind();
                    if (CURID == "0" || CURID == "")
                    {
                        if (salidArray.Length > 1)
                        {
                            if (adet == 0)
                            {
                                adet = grid.Rows.Count;
                            }
                            resimadet.Attributes["title"] = adet.ToString();
                            resimadet.InnerHtml = @"Mağaza Montajı için Ürün Başına 1 Resim Yeterli,<br>Yüklenecek Ürünü Seçerek İlerleyin.....<br>Toplam : " + adet.ToString() + " Resim Yükleyiniz";
                            //resimadet.InnerText = @"Mağaza Montajı için Ürün Başına 1 Resim Yeterli,\n\rResim YÜKELEME SIRASI LÜTFEN YUKARDAKİ ÜRÜN SIRASINA GÖRE OLSUN.....\n\rToplam : " + adet.ToString() + " Resim Yükleyiniz";
                        }
                        else
                        {
                            adet = grid.Rows.Count;
                            resimadet.Attributes["title"] = adet.ToString();
                            resimadet.InnerText = "Mağaza Montajı için Ürün Başına 1 Resim Yeterli,<br>Yüklenecek Ürünü Seçerek İlerleyin.....<br> Toplam : " + adet.ToString() + " Resim Yükleyiniz";
                        }
                    }
                    else
                    {
                        resimadet.Attributes["title"] = adet.ToString();
                        resimadet.InnerText = "İşlem Tamamlamak için Her Ürüne 1 Resim ekleyiniz,<br>Yüklenecek Ürünü Seçerek İlerleyin.....<br>Topla, Toplam : " + adet.ToString() + " Resim Yükleyiniz";
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
                ScriptManager.RegisterStartupScript(this, GetType(), "hideLoading", "hideLoading();", true);
            }
        }
        private void SMSToken()
        {
            var login = new Dictionary<string, string>
               {
                   {"grant_type", "password"},
                   {"username", "ttapiuser1"},//TT Mesaj Tarfından Size Verilen Api Kullanıcı Adı
                   {"password", "ttapiuser1123"},//TT Mesaj Tarfından Size Verilen Api Şifre
               };


            using (HttpClient httpClient = new HttpClient())
            {

                var response = httpClient.PostAsync(SmsUrl + "ttmesajToken", new FormUrlEncodedContent(login)).Result;

                if (response.IsSuccessStatusCode)
                {
                    Dictionary<string, string> tokenDetails = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Content.ReadAsStringAsync().Result);

                    SmsToken = tokenDetails.FirstOrDefault().Value;
                }
                else
                {
                    WebMsgBox.Show("Sms Token Bilgisi Okunamadı Çıkış Yapıp Tekrar Girin");
                }
            }
        }
        private string SmmsToken = "";
        private async Task SMS(string gsm, string Message)
        {
            var SMSRes = (List<SmstokenObj>)Session["SMS"];
            var loginRes = (List<LoginObj>)Session["Login"];
            SmmsToken = SMSRes[0].TokenSMS;
            try
            {
                string description = string.Empty;

                //isNotification parametresinin doldurulması
                bool? isNotificationValue = null;


                //recipentType parametresinin doldurulması
                string recipentTypeValue = string.Empty;
                var data = new
                {
                    username = SMSRes[0].SmsUser,
                    password = SMSRes[0].SmsPassword,
                    numbers = "0" + gsm,
                    message = Message,
                    origin = "YON AVM",
                    sd = "0",
                    ed = "0",
                    isNotification = isNotificationValue,
                    recipentType = recipentTypeValue,
                    brandCode = ""
                };

                var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

                using (HttpClient httpClient = new HttpClient())
                {
                    if (string.IsNullOrEmpty(SmmsToken))
                    {
                        WebMsgBox.Show("Token Hatalı");
                    }
                    else
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", SmmsToken);
                        var httppost = await httpClient.PostAsync(SMSRes[0].SmsUrl + "api/SendSms/SendSingle", content);
                        string responseContent = await httppost.Content.ReadAsStringAsync();

                        if (httppost.IsSuccessStatusCode)
                        {
                            try
                            {
                                var response = JsonConvert.DeserializeObject<SmsSonuc>(responseContent);

                                if (response.Sonuc.Contains("*OK*")) // success
                                {
                                    description = response.Sonuc.Replace("*OK*", "");
                                    List<SmsGiden> smsSonucs = new List<SmsGiden>();
                                    SmsGiden sonuc = new SmsGiden();
                                    sonuc.SOCODE = loginRes[0].SOCODE;
                                    sonuc.Sonuc = description;
                                    smsSonucs.Add(sonuc);
                                    Session.Add("SMSGONDERIM", smsSonucs);
                                }
                                else
                                {
                                    WebMsgBox.Show("SMS gönderimi başarısız: " + response.Sonuc);
                                }
                            }
                            catch (JsonException ex)
                            {
                                WebMsgBox.Show("JSON parse hatası: " + ex.Message);
                            }
                        }
                        else
                        {
                            WebMsgBox.Show("HTTP Hatası: " + httppost.StatusCode + " - " + responseContent);
                        }
                    }
                    //else
                    //{
                    //    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", SmmsToken);
                    //    var httppost = httpClient.PostAsync(ApiUrl + "api/SendSms/SendSingle", content).Result;
                    //    var response = JsonConvert.DeserializeObject<SmsSonuc>(httppost.Content.ReadAsStringAsync().Result);

                    //    if (response.Sonuc.Contains("*OK*")) // success
                    //    {
                    //        description = response.Sonuc.Replace("*OK*", "");                            
                    //    }
                    //    else
                    //    {

                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {

            }
        }
        private void BindGridView()
        {
            string DIVNAME = "";
            if (Session["DIVNAME"].ToString()== null)
            {
                DIVNAME = Session["DIVNAME"].ToString();
            }
            string query = "";
            if (CURID != "0" && CURID != "")
            {
                var ss = Session["DIVNAME"].ToString();
                query = String.Format(@"select distinct CDRSALID,ORDCHID,PROID, PROVAL,PRONAME,Convert(int,ORDCHQUAN) as ORDCHBALANCEQUAN,Convert(numeric(18,2),ORDCHQUAN*PRLPRICE) as PRLPRICE
			     ,TESLIM.DIVNAME
                FROM MDE_GENEL.dbo.MB_Islemler 
                left outer join CUSDELIVER t on MB_SALID = CDRSALID and CDRORDCHID = MB_ORDCHID
                left outer join CURRENTS on CURID = CDRCURID
                left outer join ORDERSCHILD on ORDCHID = CDRORDCHID
                left outer join PRODUCTS on PROID = ORDCHPROID
                left outer join WAVEPRODUCTS on WPROID = PROID and WPROUNIQ = 4 and WPROVAL in ('DD','EE','YY')
                left outer join DEFSTORAGE WITH (NOLOCK) ON CDRSTORID = DSTORID
                left outer join DIVISON TESLIM WITH (NOLOCK) ON TESLIM.DIVVAL = DSTORVAL AND ORDCHCOMPANY = TESLIM.DIVCOMPANY
                outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = PROID and PRLDPRID = 740) as pesinfiyat
                where CDRSALID =  '{0}' AND CDRSHIPVAL = 'ANTMOB' and CDRBASECANID is NULL
			    and MB_Tamamlandi = 0  and CDRBASECANID is NULL
                ", SALID);
                if (DIVNAME != "")
                {
                    query += String.Format("and TESLIM.DIVNAME = '{0}'", DIVNAME);
                }
                query += @"and not exists (select top 1 * from CUSDELIVER i WITH (NOLOCK) where i.CDRBASECANID = t.CDRID and MB_SALID = i.CDRSALID and i.CDRORDCHID = MB_ORDCHID)
                order by 1 desc";
            }
            else
            {
                query = String.Format(@"select 0 as CDRSALID,PRDEID as ORDCHID,PRDEPROID,PROVAL,PRONAME,Convert(int,PRDEQUAN) as ORDCHBALANCEQUAN,Convert(numeric(18,2),PRDEQUAN*PRLPRICE) as PRLPRICE
				FROM MDE_GENEL.dbo.MB_Islemler i
				left outer join PRODEMAND on PRDEID = MB_ORDCHID
				left outer join PRODUCTS on PROID = MB_PROID
                outer apply (select PRLPRICE from PRICELIST prl where prl.PRLPROID = PRDEPROID and PRLDPRID = 740) as pesinfiyat
				where MB_Tamamlandi = 0 and MB_SUPCURVAL = 'T003387' and PRDEKIND= 1 and PRDESTS = 0
				and PRDEID in ({0})
			    and MB_Tamamlandi = 0
				and not exists (select * from MDE_GENEL..MB_BayiDosyaları d where d.MB_SALID = i.MB_ORDCHID)
                order by 1 desc", SALID);
            }
            UrunSorgu = DbQuery.Query(query, ConnectionString);
            if (UrunSorgu != null)
            {
                grid.DataSource = UrunSorgu;
                grid.DataBind();
                //resimadet.Attributes["title"] = UrunSorgu.Rows.Count.ToString();
                //resimadet.InnerText = "İşlem Tamamlamak için Her Ürüne 1 Resim ekleyiniz Toplam :" + UrunSorgu.Rows.Count.ToString() + " Resim Yükleyiniz";
            }
            else
            {
                Tamamlama.Visible = true;
                tamlandı.Visible = false;
                uploadarea.Visible = false;
                Kaydet.Visible = true;
            }
        }
        internal void Dosyalar()
        {
            if (CURID != "0" && CURID != "")
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
                where d.MB_CURID = 0 and MB_SALID in ({1})
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
            string[] salidArray = Request.QueryString["salid"].Split(',');
            string sira = "";
            string qq = String.Format(@"select COUNT(*) as adet
                            from MDE_GENEL.dbo.MB_BayiDosyaları d
                            left outer join MDE_GENEL.dbo.MB_DosyaTipi t on t.id = MB_FileType
                            left outer join VDB_YON01.dbo.CURRENTS c on c.CURID = d.MB_CURID
                            where d.MB_CURID = {0} and MB_SALID in ({1}) and t.id = 1", CURID, SALID);
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
            var NewSALID = Urunler.SelectedValue;
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
                    lblUploadResult.Text = strFileName + " Bu Resim Daha Önce Yüklenmiş.....!";
                }
                else
                {
                    oFile.PostedFile.SaveAs(strFilePath);
                    try
                    {
                        ftpFolder = Server.MapPath("./UploadedFiles/Mobilya Kurulum/" + CURID + "/" + NewSALID + "/FTPGiden/");
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
                            if (!FTPCechkFolder(CURID + "/Mobilya Montaj/TAMAMLANAN/" + NewSALID))
                            {
                                CreateFolderFTP(CURID + "/Mobilya Montaj/TAMAMLANAN/" + NewSALID);
                            }
                            var PROID  = Urunler.SelectedValue;
                            string newFileName = PROID + "_" + sira + Path.GetExtension(strFileName); // Örneğin "newFileName.ext"

                            string newFilePath = Path.Combine(ftpFolder, newFileName);

                            // Rename the file
                            File.Move(ftpFilePath, newFilePath);
                            // Upload the file to FTP server
                            string ftpFullUrl = ftpUrl + "/" + CURID + "/Mobilya Montaj/TAMAMLANAN/" + NewSALID + "/"+ newFileName;
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
                                string q = String.Format(@"insert into MDE_GENEL.dbo.MB_BayiDosyaları values ({0},{1},1,'{2}','{3}')", CURID, NewSALID, newFileName, loginRes[0].SOCODE);
                                DbQuery.insertquery(q, ConnectionString2);
                                lblUploadResult.Text = strFileName + " Dosya Kaydedildi";
                            }
                            // Delete the local file after upload
                            //File.Delete(newFilePath);
                        }
                        else
                        {

                            //string sira = "";
                            //string qq = String.Format(@"select COUNT(*) as adet
                            //from MDE_GENEL.dbo.MB_BayiDosyaları d
                            //left outer join MDE_GENEL.dbo.MB_DosyaTipi t on t.id = MB_FileType
                            //left outer join VDB_YON01.dbo.CURRENTS c on c.CURID = d.MB_CURID
                            //where d.MB_CURID = {0} and t.id = 1", CURID);
                            //dosyalar = DbQuery.Query(qq, ConnectionString2);
                            //if (dosyalar == null)
                            //{
                            //    sira = "0";
                            //}
                            //else
                            //{
                            //    if (dosyalar.Rows[0][0].ToString() == "1")
                            //    {
                            //        sira = "1";
                            //    }
                            //    else
                            //    {
                            //        sira = (int.Parse(dosyalar.Rows[0]["adet"].ToString())).ToString();
                            //    }
                            //}
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
                        var eklenen = new UrunReismleri();
                        eklenen.PROID = long.Parse(Urunler.SelectedValue);
                        eklenen.PRONAME = Urunler.SelectedItem.Text;
                        OnaylıListe.Add(eklenen);
                        Urunler.Items.Remove(Urunler.SelectedItem);
                    }
                    catch (Exception ex)
                    {
                        lblUploadResult.Text = lblUploadResult.Text + "\r\n" + ex.Message;
                    }
                }
            }
            else
            {
                lblUploadResult.Text = "Yüklenecek resimi seçmek için 'Gözat'a tıklayın.";
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
                Secim.Visible = true;
                UrunMontaj.Visible = false;
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
                else if (Filename.EndsWith("jpg") || Filename.EndsWith("jpeg") || Filename.EndsWith("png") || Filename.EndsWith("jfif"))
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
                if (Urunler.Items.Count == 0)
                //if (table.Rows.Count >= int.Parse(resimadet.Attributes["title"].ToString()))
                {
                    foreach (GridViewRow row in grid.Rows)
                    {
                        DropDownList chkSelect = (DropDownList)row.FindControl("chkSelect");
                        var ss = chkSelect.SelectedValue;
                        var _ordchid = row.Cells[1].Text;

                        DbQuery.insertquery($"update MB_Islemler set MB_Tamamlandi = {int.Parse(ss)},MB_TamamlanmaTarihi = getdate(),MB_Montajcı = '{loginRes[0].SOCODE.Replace("TT-", "")}' where MB_ORDCHID = {_ordchid}", ConnectionString2);
                        if (CURID == "0")
                        {
                            DbQuery.insertquery($"update PRODEMAND set PRDESTS = 2 where PRDEID = {_ordchid}", ConnectionString);
                        }
                        else
                        {
                            var Data = DbQuery.Query($@"select TESLIM.DIVVAL as TESLIMDEPO,SATIS.DIVVAL as SATISDEPO,PRONAME,CURVAL,CURNAME from CUSDELIVER
                            left outer join MDE_GENEL.dbo.MB_Islemler on CDRORDCHID = MB_ORDCHID
                            left outer join PRODUCTS on PROID = MB_PROID
                            left outer join CURRENTS on CURID = CDRCURID
                            LEFT OUTER JOIN DEFSTORAGE WITH (NOLOCK) ON CDRSTORID = DSTORID
                            LEFT OUTER JOIN DIVISON TESLIM WITH (NOLOCK) ON TESLIM.DIVVAL = DSTORVAL
                            LEFT OUTER JOIN DIVISON SATIS WITH (NOLOCK) ON SATIS.DIVVAL = CDRSALEDIV
                            where MB_ORDCHID = {_ordchid}", ConnectionString);
                            var TESLIMDEPO = Data.Rows[0]["TESLIMDEPO"].ToString();
                            var SATISDEPO = Data.Rows[0]["SATISDEPO"].ToString();

                            var TESLIMGSM = DbQuery.GetValue($"select DIVPHN1 from DIVISON where DIVVAL = '{TESLIMDEPO}'");
                            var TESLIMSMS = SMS(TESLIMGSM, "ÜRÜN TESLİM EDİLDİ...!" + Environment.NewLine + Data.Rows[0]["CURNAME"].ToString() + " Müşterinin " + Data.Rows[0]["PRONAME"].ToString() + " ürünü ");
                            if (TESLIMDEPO != SATISDEPO)
                            {
                                var SATISGSM = DbQuery.GetValue($"select DIVPHN1 from DIVISON where DIVVAL = '{SATISDEPO}'");
                                var SATISSMS = SMS(SATISGSM, "ÜRÜN TESLİM EDİLDİ....!" + Environment.NewLine + Data.Rows[0]["CURNAME"].ToString() + " Müşterinin " + Data.Rows[0]["PRONAME"].ToString() + " ürünü ");
                            }
                        }

                    }
                    WebMsgBox.Show("Kayıt Tamamlandı");
                    Response.Redirect("Takvim.aspx");
                }
                else
                {
                    if (CURID == "0")
                    {
                        WebMsgBox.Show("Lütfen en az 1 adet resim yükleyiniz");
                    }
                    else if (int.Parse(resimadet.Attributes["title"].ToString()) > 4)
                    {
                        WebMsgBox.Show("Lütfen her ürün için 1 resim yükleyiniz");
                    }
                    else
                    {
                        WebMsgBox.Show("Lütfen en az 4 adet resim yükleyiniz");
                    }
                }
            }
        }
    }
}