<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SettingKullanıcı.aspx.cs" Inherits="YonMobilya.SettingKullanıcı" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta charset="utf-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <!-- Tell the browser to be responsive to screen width -->
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <meta name="description" content="">
    <meta name="author" content="">
    <!-- Favicon icon -->
    <link rel="icon" type="image/png" sizes="16x16" href="assets/images/Yon%20Logo.png" />
    <title>Yön Avm</title>
    <!-- Custom CSS -->
    <link href="assets/extra-libs/c3/c3.min.css" rel="stylesheet">
    <%--    <link href="assets/libs/chartist/dist/chartist.min.css" rel="stylesheet">--%>
    <%--<link href="assets/extra-libs/jvector/jquery-jvectormap-2.0.2.css" rel="stylesheet" />--%>
    <!-- Custom CSS -->
    <link href="dist/css/style.min.css" rel="stylesheet">
    <%--<link href="assets/libs/fullcalendar/css/fullcalendar.min.css" rel="stylesheet" />--%>
    <!-- HTML5 Shim and Respond.js IE8 support of HTML5 elements and media queries -->
    <!-- WARNING: Respond.js doesn't work if you view the page via file:// -->
    <!--[if lt IE 9]>
    <script src="https://oss.maxcdn.com/libs/html5shiv/3.7.0/html5shiv.js"></script>
    <script src="https://oss.maxcdn.com/libs/respond.js/1.4.2/respond.min.js"></script>
<![endif]-->

    <script type="text/javascript">
        function SadeceRakam(e, allowedchars) {
            var key = e.charCode == undefined ? e.keyCode : e.charCode;
            if ((/^[0-9]+$/.test(String.fromCharCode(key))) || key == 0 || key == 13 || isPassKey(key, allowedchars)) { return true; }
            else { return false; }
        }
        function isPassKey(key, allowedchars) {
            if (allowedchars != null) {
                for (var i = 0; i < allowedchars.length; i++) {
                    if (allowedchars[i] == String.fromCharCode(key))
                        return true;
                }
            }
            return false;
        }
        function SadeceRakamBlur(e, clear) {
            var nesne = e.target ? e.target : e.srcElement;
            var val = nesne.value;
            val = val.replace(/^\s+|\s+$/g, "");
            if (clear) val = val.replace(/\s{2,}/g, " ");
            nesne.value = val;

        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <div class="details">
            <div class="card">
                <div class="card-body">
                    <div class="row mb-3">
                        <label for="inputText" class="col-sm-2 ">Kullanıcı Ad Soyad</label>
                        <div class="col-sm-10">
                            <input type="text" runat="server" id="OFFICURNAME" class="form-control" />
                        </div>
                    </div>
                    <div class="row mb-3">
                        <label for="inputEmail" class="col-sm-2 col-form-label">E-mail</label>
                        <div class="col-sm-10">
                            <input type="email" runat="server" id="OFFCUREMAIL" class="form-control" placeholder="@" required />
                        </div>
                    </div>
                    <div class="row mb-3">
                        <label for="inputPassword" class="col-sm-2">Parola</label>
                        <div class="col-sm-10">
                            <input type="password" runat="server" id="SOENTERKEY" class="form-control" />
                        </div>
                    </div>
                    <div class="row mb-3">
                        <label for="inputNumber" class="col-sm-2 col-form-label">Telefon Numası</label>
                        <div class="col-sm-10">
                            <input type="number" runat="server" id="OFFCURPHONE" class="form-control" maxlength="10" placeholder="5xxxxxxxxx" required />
                        </div>
                    </div>
                    <div class="row mb-3">
                        <label for="inputNumber" class="col-sm-2 col-form-label">GSM Numası</label>
                        <div class="col-sm-10">
                            <input type="number" runat="server" id="OFFCURGSM" class="form-control" maxlength="10" placeholder="5xxxxxxxxx" required />
                        </div>
                    </div>
                    <div class="row mb-3">
                        <label for="inputPassword" class="col-sm-2 col-form-label">Kullanıcı Notu</label>
                        <div class="col-sm-10">
                            <textarea runat="server" id="OFFCURNOTE" class="form-control" style="height: 100px"></textarea>
                        </div>
                    </div>
                    <fieldset class="row mb-3">
                        <label class="col-sm-2 col-form-label">Pozisyon</label>
                        <div class="col-sm-10">
                            <label class="form-check-label" for="gridRadios3">Seçiniz..</label>
                            <select runat="server" id="OFFCURPOSITION" class="form-select form-control" multiple-aria-label="multiple select example">
                                <option selected="selected" value="MONTAJCI">MONTAJCI</option>
                                <option value="YETKİLİ">YETKİLİ</option>
                                <option value="SAHİBİ">FİRMA SAHİBİ</option>
                            </select>
                        </div>
                    </fieldset>
                    <div class="row mb-3">
                        <label class="col-sm-2 col-form-label"></label>
                        <div class="col-sm-10">
                            <asp:Button runat="server" ID="Kaydet" CssClass="btn btn-outline-primary form-control" OnClick="Kaydet_Click" Text="Kullanıcı Ekle" />
                        </div>
                    </div>
                    <!-- End General Form Elements -->
                </div>
            </div>
        </div>
    </form>
</body>
</html>
