<%@ Page Title="" Language="C#" MasterPageFile="~/admin.Master" AutoEventWireup="true" CodeBehind="KullaniciProfili.aspx.cs" Inherits="YonMobilya.KullaniciProfili" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

  <!-- Google Fonts -->
  <link href="https://fonts.gstatic.com" rel="preconnect">
  <link href="https://fonts.googleapis.com/css?family=Open+Sans:300,300i,400,400i,600,600i,700,700i|Nunito:300,300i,400,400i,600,600i,700,700i|Poppins:300,300i,400,400i,500,500i,600,600i,700,700i" rel="stylesheet">

  <!-- Vendor CSS Files -->
  <link href="assets/vendor/bootstrap/css/bootstrap.min.css" rel="stylesheet">
  <link href="assets/vendor/bootstrap-icons/bootstrap-icons.css" rel="stylesheet">
  <link href="assets/vendor/boxicons/css/boxicons.min.css" rel="stylesheet">
  <link href="assets/vendor/quill/quill.snow.css" rel="stylesheet">
  <link href="assets/vendor/quill/quill.bubble.css" rel="stylesheet">
  <link href="assets/vendor/remixicon/remixicon.css" rel="stylesheet">
  <link href="assets/vendor/simple-datatables/style.css" rel="stylesheet">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div runat="server" class="page-wrapper">
        <div class="container-fluid">
            <div class="row">
                <div class="col-xl-4">
                    <div class="card">
                        <div class="card-body profile-card pt-4 d-flex flex-column align-items-center">
                            <img src="assets/images/users/profile-pic.jpg" alt="Profile" class="rounded-circle" width="290">
                            <h2 runat="server" id="ProfildeAdi">Kevin Anderson</h2>
                            <h3 runat="server" id="ProfildeGorevi">Web Designer</h3>
                            <div class="pt-2">
                                <a href="#" class="btn btn-primary btn-sm" title="Upload new profile image"><i class="bi bi-upload"></i></a>
                                <a href="#" class="btn btn-danger btn-sm" title="Remove my profile image"><i class="bi bi-trash"></i></a>
                            </div>
                            <div class="social-links mt-2">
                                <a href="#" class="twitter"><i class="bi bi-twitter"></i></a>
                                <a href="#" class="facebook"><i class="bi bi-facebook"></i></a>
                                <a href="#" class="instagram"><i class="bi bi-instagram"></i></a>
                                <a href="#" class="linkedin"><i class="bi bi-linkedin"></i></a>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="col-xl-8">
                    <div class="card">
                        <div class="card-body pt-3">
                            <!-- Bordered Tabs -->
                            <ul class="nav nav-tabs nav-pills bg-nav-pills nav-justified">
                                <li class="nav-item">
                                    <a runat="server" id="profileoverview" href="#profile_overview" data-toggle="tab" aria-expanded="false" class="nav-link active">
                                        <i class="mdi mdi-home-variant d-lg-none d-block mr-1"></i>
                                        <span class="d-none d-lg-block">Genel Bakış</span>
                                    </a>
                                </li>
                                <li class="nav-item">
                                    <a runat="server" id="profileedit" href="#profile_edit" data-toggle="tab" aria-expanded="false" class="nav-link">
                                        <i class="mdi mdi-home-variant d-lg-none d-block mr-1"></i>
                                        <span class="d-none d-lg-block">Profil Düzenle</span>
                                    </a>
                                </li>
                                <li class="nav-item">
                                    <a runat="server" id="profilesettings" href="#profile_settings" data-toggle="tab" aria-expanded="false" class="nav-link">
                                        <i class="mdi mdi-home-variant d-lg-none d-block mr-1"></i>
                                        <span class="d-none d-lg-block">Ayarlar</span>
                                    </a>
                                </li>
                                <li class="nav-item">
                                    <a runat="server" id="profilepassword" href="#profile_changepassword" data-toggle="tab" aria-expanded="false" class="nav-link">
                                        <i class="mdi mdi-home-variant d-lg-none d-block mr-1"></i>
                                        <span class="d-none d-lg-block">Şifre Değiştir</span>
                                    </a>
                                </li>
                            </ul>
                            <div class="tab-content">
                                <div class="tab-pane fade show active" id="profile_overview">
                                    <h5 class="card-title">Hakkında</h5>
                                    <p class="small fst-italic"></p>

                                    <h5 class="card-title">Profile Details</h5>

                                    <div class="row">
                                        <div class="col-lg-3 col-md-4 label">Şirket</div>
                                        <div runat="server" id="ProfildeSirket" class="col-lg-9 col-md-8">Lueilwitz, Wisoky and Leuschke</div>
                                    </div>

                                    <div class="row">
                                        <div class="col-lg-3 col-md-4 label">Adres</div>
                                        <div runat="server" id="ProfildeAdres"  class="col-lg-9 col-md-8">A108 Adam Street, New York, NY 535022</div>
                                    </div>

                                    <div class="row">
                                        <div class="col-lg-3 col-md-4 label">Telefon</div>
                                        <div  runat="server" id="ProfildeTelefon" class="col-lg-9 col-md-8">(436) 486-3538 x29071</div>
                                    </div>

                                    <div class="row">
                                        <div class="col-lg-3 col-md-4 label">E-mail</div>
                                        <div runat="server" id="ProfildeMail" class="col-lg-9 col-md-8">k.anderson@example.com</div>
                                    </div>

                                </div>
                                <div class="tab-pane fade pt-3" id="profile_edit">

                                    <!-- Profile Edit Form -->

                                    <div class="row mb-3">
                                        <label for="fullName" class="col-md-4 col-lg-3 col-form-label">Ad Soyad</label>
                                        <div class="col-md-8 col-lg-9">
                                            <input runat="server" id="EditAdi" name="fullName" type="text" class="form-control" value="Kevin Anderson">
                                        </div>
                                    </div>

                                    <div class="row mb-3">
                                        <label for="about" class="col-md-4 col-lg-3 col-form-label">Hakkında</label>
                                        <div class="col-md-8 col-lg-9">
                                            <textarea runat="server" id="EditHakkinda" readonly="readonly" name="about" class="form-control" style="height: 100px">Sunt est soluta temporibus accusantium neque nam maiores cumque temporibus. Tempora libero non est unde veniam est qui dolor. Ut sunt iure rerum quae quisquam autem eveniet perspiciatis odit. Fuga sequi sed ea saepe at unde.</textarea>
                                        </div>
                                    </div>

                                    <div class="row mb-3">
                                        <label for="company" class="col-md-4 col-lg-3 col-form-label">Şirket</label>
                                        <div class="col-md-8 col-lg-9">
                                            <input runat="server" id="EditSirket" name="company" type="text" class="form-control" value="Lueilwitz, Wisoky and Leuschke" />
                                        </div>
                                    </div>

                                    <div class="row mb-3">
                                        <label for="Job" class="col-md-4 col-lg-3 col-form-label">İş</label>
                                        <div class="col-md-8 col-lg-9">
                                            <input runat="server" id="EditIs" name="job" type="text" class="form-control" value="Web Designer">
                                        </div>
                                    </div>

                                    <div class="row mb-3">
                                        <label for="Country" class="col-md-4 col-lg-3 col-form-label">İl</label>
                                        <div class="col-md-8 col-lg-9">
                                            <input runat="server" id="EditUlke" name="country" type="text" class="form-control" value="USA">
                                        </div>
                                    </div>

                                    <div class="row mb-3">
                                        <label for="Address" class="col-md-4 col-lg-3 col-form-label">Adres</label>
                                        <div class="col-md-8 col-lg-9">
                                            <input runat="server" id="EditAdres" name="address" type="text" class="form-control" value="A108 Adam Street, New York, NY 535022">
                                        </div>
                                    </div>

                                    <div class="row mb-3">
                                        <label for="Phone" class="col-md-4 col-lg-3 col-form-label">Telefon</label>
                                        <div class="col-md-8 col-lg-9">
                                            <input runat="server" id="EditTelefon" name="phone" type="text" class="form-control" value="(436) 486-3538 x29071">
                                        </div>
                                    </div>

                                    <div class="row mb-3">
                                        <label for="Email" class="col-md-4 col-lg-3 col-form-label">E-mail</label>
                                        <div class="col-md-8 col-lg-9">
                                            <input runat="server" id="EditMail" name="email" type="email" class="form-control" value="k.anderson@example.com">
                                        </div>
                                    </div>

                                    <div class="row mb-3">
                                        <label for="Twitter" class="col-md-4 col-lg-3 col-form-label">Twitter Profile</label>
                                        <div class="col-md-8 col-lg-9">
                                            <input name="twitter" type="text" class="form-control" id="Twitter" value="https://twitter.com/#">
                                        </div>
                                    </div>

                                    <div class="row mb-3">
                                        <label for="Facebook" class="col-md-4 col-lg-3 col-form-label">Facebook Profile</label>
                                        <div class="col-md-8 col-lg-9">
                                            <input name="facebook" type="text" class="form-control" id="Facebook" value="https://facebook.com/#">
                                        </div>
                                    </div>

                                    <div class="row mb-3">
                                        <label for="Instagram" class="col-md-4 col-lg-3 col-form-label">Instagram Profile</label>
                                        <div class="col-md-8 col-lg-9">
                                            <input name="instagram" type="text" class="form-control" id="Instagram" value="https://instagram.com/#">
                                        </div>
                                    </div>

                                    <div class="row mb-3">
                                        <label for="Linkedin" class="col-md-4 col-lg-3 col-form-label">Linkedin Profile</label>
                                        <div class="col-md-8 col-lg-9">
                                            <input name="linkedin" type="text" class="form-control" id="Linkedin" value="https://linkedin.com/#">
                                        </div>
                                    </div>

                                    <div class="text-center">                                        
                                        <asp:button runat="server" id="EditKaydet" onclick="EditKaydet_Click" type="submit" class="btn btn-primary" Text="Değişiklikleri Kaydet"/>
                                    </div>
                                    <!-- End Profile Edit Form -->

                                </div>
                                <div class="tab-pane fade pt-3" id="profile_settings">

                                    <!-- Settings Form -->


                                    <div class="row mb-3">
                                        <label for="fullName" class="col-md-4 col-lg-3 col-form-label">E-posta Bildirimleri</label>
                                        <div class="col-md-8 col-lg-9">
                                            <div class="form-check">
                                                <input class="form-check-input" type="checkbox" id="changesMade" checked>
                                                <label class="form-check-label" for="changesMade">
                                                    Hesabınızda yapılan değişiklikler
                                                </label>
                                            </div>
                                            <div class="form-check">
                                                <input class="form-check-input" type="checkbox" id="newProducts" checked>
                                                <label class="form-check-label" for="newProducts">
                                                    Yeni ürün ve hizmetler hakkında bilgi
                                                </label>
                                            </div>
                                            <div class="form-check">
                                                <input class="form-check-input" type="checkbox" id="proOffers">
                                                <label class="form-check-label" for="proOffers">
                                                    Pazarlama ve promosyon teklifleri
                                                </label>
                                            </div>
                                            <div class="form-check">
                                                <input class="form-check-input" type="checkbox" id="securityNotify" checked disabled>
                                                <label class="form-check-label" for="securityNotify">
                                                    Güvenlik uyarıları
                                                </label>
                                            </div>
                                        </div>
                                    </div>

                                    <div class="text-center">
                                        <asp:button runat="server" ID="SettingsKaydet" OnClick="SettingsKaydet_Click" type="submit" class="btn btn-primary" Text="Değişiklikleri Kaydet"/>
                                    </div>
                                    <!-- End settings Form -->

                                </div>
                                <div class="tab-pane fade pt-3" id="profile_changepassword">
                                    <!-- Change Password Form -->


                                    <div class="row mb-3">
                                        <label for="currentPassword" class="col-md-4 col-lg-3 col-form-label">Geçerli Şifre</label>
                                        <div class="col-md-8 col-lg-9">
                                            <input name="password" type="password" class="form-control" id="currentPassword">
                                        </div>
                                    </div>

                                    <div class="row mb-3">
                                        <label for="newPassword" class="col-md-4 col-lg-3 col-form-label">Yeni Şifre Girin</label>
                                        <div class="col-md-8 col-lg-9">
                                            <input runat="server" name="newpassword" type="password" class="form-control" id="newPassword">
                                        </div>
                                    </div>

                                    <div class="row mb-3">
                                        <label for="renewPassword" class="col-md-4 col-lg-3 col-form-label">Yeni Şifreyi Yeniden Girin</label>
                                        <div class="col-md-8 col-lg-9">
                                            <input runat="server" name="renewpassword" type="password" class="form-control" id="renewPassword">
                                        </div>
                                    </div>

                                    <div class="text-center">
                                        <asp:button runat="server" ID="ParolaKaydet" OnClick="ParolaKaydet_Click" type="submit" class="btn btn-primary" Text="Şifre Değiştir"/>
                                    </div>
                                    <!-- End Change Password Form -->

                                </div>
                            </div>
                            <!-- End Bordered Tabs -->
                        </div>
                    </div>

                </div>
            </div>
        </div>
    </div>
    <script>
        function activateTab(tabId) {
            // Tüm sekmeleri devre dışı bırak
            $('.nav-link').removeClass('active');
            $('.tab-pane').removeClass('active show');

            // Belirtilen sekmeyi aktif et
            $('#' + tabId).addClass('active');
            $('#' + tabId).addClass('show');
        }
    </script>
    <script src="assets/libs/jquery/dist/jquery.min.js"></script>
</asp:Content>
