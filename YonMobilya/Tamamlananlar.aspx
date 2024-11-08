<%@ Page Title="" Language="C#" MasterPageFile="~/admin.Master" AutoEventWireup="true" CodeBehind="Tamamlananlar.aspx.cs" Inherits="YonMobilya.Tamamlananlar" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .modal {
            position: fixed;
            top: 0;
            left: 0;
            background-color: black;
            z-index: 99;
            opacity: 0.8;
            filter: alpha(opacity=80);
            -moz-opacity: 0.8;
            min-height: 100%;
            width: 100%;
        }

        .ddl {
            border: 2px solid #7d6754;
            border-radius: 5px;
            padding: 3px;
            -webkit-appearance: none;
            background-image: url('Images/Arrowhead-Down-01.png');
            background-position: 88px;
            background-repeat: no-repeat;
            text-indent: 0.01px; /*In Firefox*/
            text-overflow: ''; /*In Firefox*/
        }
        .loading {
            font-family: Arial;
            font-size: 10pt;
            /*border: 5px solid #67CFF5;
            width: 200px;
            height: 100px;*/
            display: none;
            position: fixed;
            background-color: White;
            z-index: 999;
        }
        /* GridView kontrolünüz için genel stil */
        .gridView {
            width: 100%; /* GridView'in tam genişlikte olmasını sağlar */
            table-layout:auto; /* Sütun genişliklerini korur */
            border-collapse:collapse;            
        }

            .gridView .td1 .td2 {
                word-wrap: break-word;
                white-space: normal;
                overflow-wrap: break-word;
            }

        .td1 {
            width:70%;
            table-layout:fixed;
            word-wrap: break-word; /* Uzun kelimeleri satır sonuna sarmak için */
            white-space: normal; /* Satır sonu karakterlerini ve boşlukları işleyin */
            overflow-wrap: break-word; /* Uzun kelimeleri alt satıra geçirin */            
            /*border: solid;*/
            /* max-width: 150px; İsteğe bağlı: sütun genişliğini sınırlandırın */
        }
        .td2 {
            width:10%;
            table-layout:auto;
            /*border: none;*/
        }
        .gridView .pager a,
        .gridView .pager span {
            display: inline-block;
            padding: 5px 10px;
            margin: 2px;
            color: white; /* Beyaz yazı rengi */
            background-color: #007bff; /* Kutucuk rengi */
            border-radius: 4px; /* Kenarları yuvarlama */
            text-decoration: none;
        }

        .gridView .pager a:hover {
            background-color: #0056b3; /* Hover efekti */
        }

        .gridView .pager span {
            font-weight: bold;
            background-color: #00b388; /* Aktif sayfa rengi */
        }
    </style>

    <style type="text/css">
        .profile-picture {
            opacity: 0.75;
            height: 400px;
            width: 100%;
            position: relative;
            overflow: hidden;
            /* default image */
            /*background-image: url("img/arac-ruhsati-500-330.jpg");*/
            background-position: center;
            background-repeat: no-repeat;
            background-size: cover;
            box-shadow: 0 8px 6px -6px black;
        }

        .file-uploader {
            /* make it invisible */
            opacity: 0;
            /* make it take the full height and width of the parent container */
            height: 100%;
            width: 100%;
            cursor: pointer;
            /* make it absolute */
            position: absolute;
            top: 0%;
            left: 0%;
        }

        .upload-icon {
            position: absolute;
            top: 45%;
            left: 50%;
            transform: translate(-50%, -50%);
            /* initial icon state */
            opacity: 1;
            transition: opacity 0.3s ease;
            color: #ccc;
            -webkit-text-stroke-width: 2px;
            -webkit-text-stroke-color: #bbb;
        }
        /* toggle icon state */
        .profile-picture:hover .upload-icon {
            opacity: 1;
        }

        .profile-picture2 {
            opacity: 0.75;
            height: 400px;
            width: 100%;
            position: relative;
            overflow: hidden;
            /* default image */
            /*background-image: url("img/tramer.png");*/
            background-position: center;
            background-repeat: no-repeat;
            background-size: cover;
            box-shadow: 0 8px 6px -6px black;
        }

        .file-uploader2 {
            /* make it invisible */
            opacity: 0;
            /* make it take the full height and width of the parent container */
            height: 100%;
            width: 100%;
            cursor: pointer;
            /* make it absolute */
            position: absolute;
            top: 0%;
            left: 0%;
        }

        .upload-icon2 {
            position: absolute;
            top: 45%;
            left: 50%;
            transform: translate(-50%, -50%);
            /* initial icon state */
            opacity: 1;
            transition: opacity 0.3s ease;
            color: #ccc;
            -webkit-text-stroke-width: 2px;
            -webkit-text-stroke-color: #bbb;
        }
        /* toggle icon state */
        .profile-picture2:hover .upload-icon2 {
            opacity: 1;
        }

        .profile-picture3 {
            opacity: 0.75;
            height: 400px;
            width: 100%;
            position: relative;
            overflow: hidden;
            /* default image */
            /*background-image: url("img/kimlik.png");*/
            background-position: center;
            background-repeat: no-repeat;
            background-size: cover;
            box-shadow: 0 8px 6px -6px black;
        }

        .file-uploader3 {
            /* make it invisible */
            opacity: 0;
            /* make it take the full height and width of the parent container */
            height: 100%;
            width: 100%;
            cursor: pointer;
            /* make it absolute */
            position: absolute;
            top: 0%;
            left: 0%;
        }

        .upload-icon3 {
            position: absolute;
            top: 45%;
            left: 50%;
            transform: translate(-50%, -50%);
            /* initial icon state */
            opacity: 1;
            transition: opacity 0.3s ease;
            color: #ccc;
            -webkit-text-stroke-width: 2px;
            -webkit-text-stroke-color: #bbb;
        }
        /* toggle icon state */
        .profile-picture3:hover .upload-icon3 {
            opacity: 1;
        }


        .profile-picture4 {
            opacity: 0.75;
            height: 250px;
            width: 400px;
            position: relative;
            overflow: hidden;
            /* default image */
            /*background-image: url("img/Ekbelge.png");*/
            background-position: center;
            background-repeat: no-repeat;
            background-size: cover;
            box-shadow: 0 8px 6px -6px black;
        }

        .file-uploader4 {
            /* make it invisible */
            opacity: 0;
            /* make it take the full height and width of the parent container */
            height: 100%;
            width: 100%;
            cursor: pointer;
            /* make it absolute */
            position: absolute;
            top: 0%;
            left: 0%;
        }

        .upload-icon4 {
            position: absolute;
            top: 45%;
            left: 50%;
            transform: translate(-50%, -50%);
            /* initial icon state */
            opacity: 0;
            transition: opacity 0.3s ease;
            color: #ccc;
            -webkit-text-stroke-width: 2px;
            -webkit-text-stroke-color: #bbb;
        }
        /* toggle icon state */
        .profile-picture4:hover .upload-icon4 {
            opacity: 1;
        }
        .image {
            height: 90%;
            width: 90%;
            position: relative;
            /* default image */
            /*background-image: url("img/Ekbelge.png");*/
            background-position: center;
            background-repeat: no-repeat;
            background-size: cover;
            box-shadow: 0 8px 6px -6px black;
        }
        /* GridView kontrolünüz için genel stil */
        .gridView {
            width: 100%; /* GridView'in tam genişlikte olmasını sağlar */
            table-layout: auto; /* Sütun genişliklerini korur */
            border-collapse: collapse;
        }

            .gridView .td1 .td2 {
                word-wrap: break-word;
                white-space: normal;
                overflow-wrap: break-word;
            }

        .td1 {
            width: 30%;
            table-layout: fixed;
            word-wrap: break-word; /* Uzun kelimeleri satır sonuna sarmak için */
            white-space: normal; /* Satır sonu karakterlerini ve boşlukları işleyin */
            overflow-wrap: break-word; /* Uzun kelimeleri alt satıra geçirin */
            /*border: solid;*/
            /* max-width: 150px; İsteğe bağlı: sütun genişliğini sınırlandırın */
        }

        .td2 {
            table-layout: auto;
            /*border: none;*/
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div runat="server" class="page-wrapper">
        <div class="container-fluid">
            <div class="row">
                <div class="col-md-12">
                    <div class="card">
                        <div class="card-body">
                            <div class="ml-auto">
                                <div class="recentOrders" style="overflow: auto">
                                    <div class="cardHeader">
                                        <div class="col-md-12">
                                            <div class="form-group">
                                                <h2 align="center">Tamamlanan İşler</h2>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="scroll">
                                        <div class="table-responsive">
                                            <div class="table-responsive">
                                                <asp:GridView ID="GridView1" runat="server" 
                                                    CssClass="gridView"
                                                    AutoGenerateColumns="False"
                                                    AllowPaging="True" 
                                                    PageSize="10" 
                                                    OnPageIndexChanging="GridView1_PageIndexChanging" 
                                                    OnRowCreated="GridView1_RowCreated" 
                                                    OnSelectedIndexChanged="GridView1_SelectedIndexChanged">
                                                    <Columns>
                                                        <asp:CommandField ShowSelectButton="True" SelectText="Resimleri Gör" HeaderText="Sipariş Bilgisi" ItemStyle-HorizontalAlign="Center" ButtonType="Button" ItemStyle-CssClass="f-icon" />
                                                        <asp:BoundField ItemStyle-CssClass="td" DataField="SALID" HeaderText="CDRSALID" />
                                                        <asp:BoundField ItemStyle-CssClass="td" DataField="CURID" HeaderText="CDRCURID" />
                                                        <asp:BoundField ItemStyle-CssClass="td1" DataField="CURNAME" HeaderText="MÜŞTERİ" />
                                                        <asp:BoundField ItemStyle-CssClass="td1" DataField="PRONAME" HeaderText="ÜRÜN" />
                                                        <asp:BoundField ItemStyle-CssClass="td" DataField="MB_PlanTarih" HeaderText="PLAN TARİHİ" DataFormatString="{0:yyyy/MM/dd}" />
                                                        <asp:BoundField ItemStyle-CssClass="td1" DataField="OFFCURNAME" HeaderText="MONTAJCI" />
                                                        <asp:BoundField ItemStyle-CssClass="td1" DataField="MB_TamamlanmaTarihi" HeaderText="MONTAJ TARİHİ" DataFormatString="{0:yyyy/MM/dd}" />
                                                        <asp:BoundField ItemStyle-CssClass="td1" DataField="PRLPRICE" DataFormatString="{0:C2}" HeaderText="CİRO" />
                                                        <asp:BoundField ItemStyle-CssClass="td1" DataField="Hakedis" DataFormatString="{0:C2}" HeaderText="HAKEDİŞ" />
                                                        <asp:BoundField ItemStyle-CssClass="td" DataField="PROID" HeaderText="PROID" />
                                                    </Columns>
                                                    <PagerSettings Mode="Numeric" Position="Bottom"  />
                                                    <PagerStyle CssClass="pager" ForeColor="White" HorizontalAlign="Center" />
                                                </asp:GridView>
                                            </div>
                                        </div>
                                    </div>
                                    <div id="paginationContainer" class="float-right"></div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="details">
                        <div class="recentOrders" style="overflow: auto">
                            <div class="cardHeader">
                                <%--<h2>Yüklenen Dosyalar</h2>--%>
                            </div>
                            <asp:Label ID="FileLoad" runat="server" Visible="false"></asp:Label>
                            <div class="scroll">
                                <asp:GridView runat="server" ID="table" AutoGenerateColumns="false" CssClass="customers" BorderStyle="Solid" Width="100%" OnSelectedIndexChanged="table_SelectedIndexChanged" OnRowCreated="table_RowCreated">
                                    <Columns>
                                        <asp:CommandField ItemStyle-CssClass="td2" ShowSelectButton="True" SelectText="Resimi Gör" HeaderText="Gör" ItemStyle-HorizontalAlign="Left" ButtonType="Button" />
                                        <asp:BoundField ItemStyle-CssClass="td" DataField="CURID" ReadOnly="True" />
                                        <asp:BoundField ItemStyle-CssClass="td2" DataField="CURNAME" HeaderText="Müşteri" />
                                        <asp:BoundField ItemStyle-CssClass="td" DataField="FileTypeName" HeaderText="Dosya Tipi" />
                                        <asp:BoundField ItemStyle-CssClass="td" DataField="FileName" HeaderText="Dosya Adı" />
                                    </Columns>
                                </asp:GridView>
                            </div>
                        </div>
                        <div runat="server" id="uploadarea" class="row col-12" visible="false">
                            <div runat="server" id="resim1" class="col-md-6">
                                <div class="card">
                                    <div>
                                        <div runat="server" id="Div1" class="number"></div>
                                        <div class="cardName">Montaj Resmi 1</div>
                                    </div>
                                    <div runat="server" id="profilepicture" class="profile-picture">
                                        <h1 class="upload-icon">
                                            <i class="fa fa-plus fa-2x" aria-hidden="true"></i>
                                        </h1>
                                        <input
                                            id="oFile"
                                            type="file"
                                            runat="server"
                                            class="file-uploader"
                                            onchange="upload()"
                                            accept="image/*,application/pdf" />
                                    </div>
                                    <div class="iconBox">
                                        <i class="fa fa-eye" aria-hidden="true"></i>
                                        <label id="uploaderName1"></label>
                                    </div>
                                    <asp:Button ID="btnUpload" type="submit" Text="Kurulum Resmini Aktar" runat="server" OnClick="btnUpload_Click"></asp:Button>
                                    <asp:Panel ID="frmConfirmation" Visible="False" runat="server">
                                        <asp:Label ID="lblUploadResult" runat="server"></asp:Label>
                                    </asp:Panel>
                                </div>
                            </div>
                            <div runat="server" id="resim3" class="col-md-6">
                                <div class="card">
                                    <div>
                                        <div runat="server" id="Div3" class="number"></div>
                                        <div class="cardName">SSH RESİM</div>
                                    </div>
                                    <div class="profile-picture3">
                                        <h1 class="upload-icon3">
                                            <i class="fa fa-plus fa-2x" aria-hidden="true"></i>
                                        </h1>
                                        <input
                                            id="oFile3"
                                            type="file"
                                            runat="server"
                                            class="file-uploader3"
                                            onchange="upload3()"
                                            accept="image/*,application/pdf" />
                                    </div>
                                    <div class="iconBox">
                                        <i class="fa fa-eye" aria-hidden="true"></i>
                                        <label id="uploaderName3"></label>
                                    </div>
                                    <asp:Button ID="btnUpload3" type="submit" Text="SSH Resmini Aktar" runat="server" OnClick="btnUpload3_Click"></asp:Button>
                                    <asp:Panel ID="frmConfirmation3" Visible="False" runat="server">
                                        <asp:Label ID="lblUploadResult3" runat="server"></asp:Label>
                                    </asp:Panel>
                                </div>
                            </div>
                        </div>
                        <div runat="server" id="Div2" class="row col-12">
                            <div runat="server" id="Div4" class="col-md-10">
                                <asp:Panel ID="panelViewer" runat="server" CssClass="viewer-panel">
                                    <asp:Image ID="imgViewer" runat="server" Visible="False" CssClass="image" />
                                    <iframe runat="server" id="iframe" src="#" frameborder="0" visible="false"></iframe>
                                    <asp:PlaceHolder ID="pdfViewerPlaceHolder" runat="server"></asp:PlaceHolder>
                                </asp:Panel>
                            </div>
                        </div>
                        <div class="card">
                            <div class="loading" align="center">
                                <img src="img/islem_gerceklestiriliyor.gif" alt="" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <script src="assets/libs/jquery/dist/jquery.min.js"></script>
    <script type="text/javascript">
        function ShowProgress() {
            setTimeout(function () {
                var modal = $('<div />');
                modal.addClass("modal");
                $('body').append(modal);
                var loading = $(".loading");
                loading.show();
                var top = Math.max($(window).height() / 2 - loading[0].offsetHeight / 2, 0);
                var left = Math.max($(window).width() / 2 - loading[0].offsetWidth / 2, 0);
                loading.css({ top: top, left: left });
            }, 200);
        }
        $('form').live("submit", function () {
            ShowProgress();
        });
    </script>

    <script>
        function upload() {

            const fileUploadInput = document.querySelector('.file-uploader');
            const filetext = document.getElementById('uploaderName1');
            /// Validations ///
            if (!fileUploadInput.value) {
                return;
            }

            // using index [0] to take the first file from the array
            const image = fileUploadInput.files[0];
            // check if the file selected is not an image file
            if (!image.type.includes('image') && image.type != 'application/pdf') {
                return alert('Sadece resim Dosyası Seçimine izin verilir!');
            }

            // check if size (in bytes) exceeds 10 MB
            if (image.size > 10_000_000) {
                return alert('Maksimum yükleme boyutu 10MB!');
            }

            /// Display the image on the screen ///
            if (image.type === 'application/pdf') {
                const profilePicture = document.querySelector('.profile-picture');
                profilePicture.style.backgroundImage = `url('img/pdf.jpg')`; // Adjust the path as needed
                filetext.textContent = `Seçilen PDF dosyası: ${file.name}`;
            }
            else if (image.type.includes('image')) {
                const fileReader = new FileReader();
                fileReader.readAsDataURL(image);

                fileReader.onload = (fileReaderEvent) => {
                    const profilePicture = document.querySelector('.profile-picture');
                    profilePicture.style.backgroundImage = `url(${fileReaderEvent.target.result})`;
                    nodes.text = fileUploadInput.value;
                }
                filetext.text = fileUploadInput.files[0];
                // upload image to the server or the cloud
            }
        }
        function upload2() {

            const fileUploadInput = document.querySelector('.file-uploader2');
            const filetext = document.getElementById('uploaderName2');

            /// Validations ///

            if (!fileUploadInput.value) {
                return;
            }

            // using index [0] to take the first file from the array
            const image = fileUploadInput.files[0];

            // check if the file selected is not an image file
            if (!image.type.includes('image') && image.type != 'application/pdf') {
                return alert('Sadece resim Dosyası Seçimine izin verilir!');
            }

            // check if size (in bytes) exceeds 10 MB
            if (image.size > 10_000_000) {
                return alert('Maksimum yükleme boyutu 10MB!');
            }

            /// Display the image on the screen ///

            if (image.type === 'application/pdf') {
                const profilePicture = document.querySelector('.profile-picture2');
                profilePicture.style.backgroundImage = `url('img/pdf.jpg')`; // Adjust the path as needed
                filetext.textContent = `Seçilen PDF dosyası: ${file.name}`;
            }
            else if (image.type.includes('image')) {
                const fileReader = new FileReader();
                fileReader.readAsDataURL(image);

                fileReader.onload = (fileReaderEvent) => {
                    const profilePicture = document.querySelector('.profile-picture2');
                    profilePicture.style.backgroundImage = `url(${fileReaderEvent.target.result})`;
                    nodes.text = fileUploadInput.value;
                }
                filetext.text = fileUploadInput.files[0];

                // upload image to the server or the cloud
            }
        }
        function upload3() {

            const fileUploadInput = document.querySelector('.file-uploader3');
            const filetext = document.getElementById('uploaderName3');

            /// Validations ///

            if (!fileUploadInput.value) {
                return;
            }

            // using index [0] to take the first file from the array
            const image = fileUploadInput.files[0];

            // check if the file selected is not an image file
            if (!image.type.includes('image') && image.type != 'application/pdf') {
                return alert('Sadece resim Dosyası Seçimine izin verilir!');
            }

            // check if size (in bytes) exceeds 10 MB
            if (image.size > 10_000_000) {
                return alert('Maksimum yükleme boyutu 10MB!');
            }

            /// Display the image on the screen ///
            if (image.type === 'application/pdf') {
                const profilePicture = document.querySelector('.profile-picture3');
                profilePicture.style.backgroundImage = `url('img/pdf.jpg')`; // Adjust the path as needed
                filetext.textContent = `Seçilen PDF dosyası: ${file.name}`;
            }
            else if (image.type.includes('image')) {
                const fileReader = new FileReader();
                fileReader.readAsDataURL(image);

                fileReader.onload = (fileReaderEvent) => {
                    const profilePicture = document.querySelector('.profile-picture3');
                    profilePicture.style.backgroundImage = `url(${fileReaderEvent.target.result})`;
                    nodes.text = fileUploadInput.value;
                }
                filetext.text = fileUploadInput.files[0];
            }
        }
        function upload4() {

            const fileUploadInput = document.querySelector('.file-uploader4');
            const filetext = document.getElementById('uploaderName4');

            /// Validations ///

            if (!fileUploadInput.value) {
                return;
            }

            // using index [0] to take the first file from the array
            const image = fileUploadInput.files[0];

            // check if the file selected is not an image file
            if (!image.type.includes('image') && image.type != 'application/pdf') {
                return alert('Sadece resim Dosyası Seçimine izin verilir!');
            }

            // check if size (in bytes) exceeds 10 MB
            if (image.size > 10_000_000) {
                return alert('Maksimum yükleme boyutu 10MB!');
            }

            /// Display the image on the screen ///
            if (image.type === 'application/pdf') {
                const profilePicture = document.querySelector('.profile-picture4');
                profilePicture.style.backgroundImage = `url('img/pdf.jpg')`; // Adjust the path as needed
                filetext.textContent = `Seçilen PDF dosyası: ${file.name}`;
            }
            else if (image.type.includes('image')) {
                const fileReader = new FileReader();
                fileReader.readAsDataURL(image);

                fileReader.onload = (fileReaderEvent) => {
                    const profilePicture = document.querySelector('.profile-picture4');
                    profilePicture.style.backgroundImage = `url(${fileReaderEvent.target.result})`;
                    nodes.text = fileUploadInput.value;
                }
                filetext.text = fileUploadInput.files[0];
            }
            // upload image to the server or the cloud
        }
    </script>
</asp:Content>
