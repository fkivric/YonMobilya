<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="NewLogin.aspx.cs" Inherits="YonMobilya.NewLogin" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <!-- Tell the browser to be responsive to screen width -->
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <meta name="description" content="" />
    <meta name="author" content="" />
    <!-- Favicon icon -->
    <link rel="icon" type="image/png" sizes="16x16" href="assets/images/Yon%20Logo.png" />
    <title>Yön Avm</title>
    <!-- Custom CSS -->
    <link href="dist/css/style.min.css" rel="stylesheet" />
    <!-- HTML5 Shim and Respond.js IE8 support of HTML5 elements and media queries -->
    <!-- WARNING: Respond.js doesn't work if you view the page via file:// -->
    <!--[if lt IE 9]>
    <script src="https://oss.maxcdn.com/libs/html5shiv/3.7.0/html5shiv.js"></script>
    <script src="https://oss.maxcdn.com/libs/respond.js/1.4.2/respond.min.js"></script>
    <![endif]-->
</head>
<body>
    <div class="main-wrapper">
        <div class="auth-wrapper d-flex no-block justify-content-center align-items-center position-relative"
            style="background: url(assets/images/big/auth-bg2.jpg) no-repeat center center;">
            <div class="auth-box row">
                <div class="col-lg-7 col-md-5" style="background-image: url(img/yonavmlogo.png); background-repeat:no-repeat; background-size:initial;min-height:300px; background-position:center">
                </div>
                <div class="col-lg-5 col-md-7 bg-white">
                    <div class="p-3">
                        <div class="text-center">
                            
                        </div>
                        <h2 class="mt-3 text-center">Oturum Aç</h2>
                        <p class="text-center">Yönetici paneline erişmek için Giriş Yapınız.</p>
                        <form id="form1" runat="server">
                            <div class="row">
                                <div class="col-lg-12">
                                    <div class="form-group">
                                        <label class="text-dark" for="uname">Kullanıcı Adı</label>
                                        <input runat="server" id="uname" type="text"  class="form-control" placeholder="Kullanıcı Adı girin" />
                                    </div>
                                </div>
                                <div class="col-lg-12">
                                    <div class="form-group">
                                        <label class="text-dark" for="pwd">Şifre</label>
                                        <input runat="server" id="pwd" type="password" class="form-control" placeholder="Şifre girin" />
                                    </div>
                                </div>
                                <div class="col-lg-12 text-center">
                                    <asp:Button ID="btnGiris" OnClick="btnGiris_Click" runat="server" CssClass="btn btn-block btn-dark" Text="GİRİŞ YAP"></asp:Button>
                                </div>
                                <div class="col-lg-12 text-center mt-5">
                                    Kullanıcı Talebi için :  <a href="#" class="text-danger"> Kayıt Ol </a>
                                </div>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
        <!-- ============================================================== -->
        <!-- Login box.scss -->
        <!-- ============================================================== -->
    </div>
    <!-- ============================================================== -->
    <!-- All Required js -->
    <!-- ============================================================== -->
    <script src="NewAdminDashboard/assets/libs/jquery/dist/jquery.min.js "></script>
    <!-- Bootstrap tether Core JavaScript -->
    <script src="NewAdminDashboard/assets/libs/popper.js/dist/umd/popper.min.js "></script>
    <script src="NewAdminDashboard/assets/libs/bootstrap/dist/js/bootstrap.min.js "></script>
    <!-- ============================================================== -->
    <!-- This page plugin js -->
    <!-- ============================================================== -->
    <script>
        $(".preloader ").fadeOut();
    </script>
</body>
</html>
