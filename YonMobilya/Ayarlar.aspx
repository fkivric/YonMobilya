<%@ Page Title="" Language="C#" MasterPageFile="~/admin.Master" AutoEventWireup="true" CodeBehind="Ayarlar.aspx.cs" Inherits="YonMobilya.Ayarlar" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="page-wrapper">

                <ul class="nav nav-pills bg-nav-pills nav-justified mb-2">
                    <li class="nav-item">
                        <a href="#home1" data-toggle="tab" aria-expanded="false"
                            class="nav-link rounded-0 active">
                            <i class="mdi mdi-home-variant d-lg-none d-block mr-1"></i>
                            <span class="d-none d-lg-block">Mevcut Kullanıcı Güncelle</span>
                        </a>
                    </li>
                    <li class="nav-item">
                        <a href="#profile1" data-toggle="tab" aria-expanded="false"
                            class="nav-link rounded-0">
                            <i class="mdi mdi-account-circle d-lg-none d-block mr-1"></i>
                            <span class="d-none d-lg-block">Kullanıcı Ekle</span>
                        </a>
                    </li>
                </ul>

                <div class="tab-content">
                    <div class="tab-pane show active" id="home1">
                        <div class="card">
                            <div class="row">
                                <div class="col-lg-12">
                                    <iframe runat="server" id="Iframe1" style="width: 100%; height: 810px" src="SettingMevcutKullanıcılar.aspx"></iframe>
                                    <%--<div id="accordion" class="custom-accordion mb-8">
                                                    <div class="card mb-0">
                                                        <div class="card-header" id="headingOne">
                                                            <h5 class="m-0">
                                                                <a class="custom-accordion-title d-block d-block pt-2 pb-2" data-toggle="collapse"
                                                                    href="#collapseOne" aria-expanded="undefined" aria-controls="collapseOne">
                                                                    Mevcut Kullanıcı Güncelle <span class="float-right"><i
                                                                            class="mdi mdi-chevron-down accordion-arrow"></i></span>
                                                                </a>
                                                            </h5>
                                                        </div>
                                                        <div id="collapseOne" class="collapse show" aria-labelledby="headingOne"
                                                            data-parent="#accordion">
                                                            <div class="card-body">
                                                                <iframe runat="server" id="mevcutkullanici" style="width: 100%; height: 810px" src="SettingMevcutKullanıcılar.aspx"></iframe>
                                                            </div>
                                                        </div>
                                                    </div> <!-- end card-->

                                                    <div class="card mb-0">
                                                        <div class="card-header" id="headingTwo">
                                                            <h5 class="m-0">
                                                                <a class="custom-accordion-title collapsed d-block pt-2 pb-2"
                                                                    data-toggle="collapse" href="#collapseTwo" aria-expanded="undefined"
                                                                    aria-controls="collapseTwo">
                                                                    Kullanıcı Ekle <span class="float-right"><i
                                                                            class="mdi mdi mdi-chevron-down accordion-arrow"></i></span>
                                                                </a>
                                                            </h5>
                                                        </div>
                                                        <div id="collapseTwo" class="collapse" aria-labelledby="headingTwo"
                                                            data-parent="#accordion">
                                                            <div class="card-body">
                                                                <iframe runat="server" id="kullaniciekle" style="width: 100%; height: 800px" src="SettingKullanıcı.aspx"></iframe>
                                                            </div>
                                                        </div>
                                                    </div> <!-- end card-->
                                                </div> <!-- end custom accordions-->
                                            </div> <!-- end col -->

                                        </div>--%>
                                    <!-- end row-->
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="tab-pane" id="profile1">
                        <iframe runat="server" id="Iframe2" style="width: 100%; height: 800px" src="SettingKullanıcı.aspx"></iframe>
                    </div>
                    <div class="tab-pane" id="settings1">
                        <p class="mb-0">
                            Daha sonra eklenecek bir çok işlem olacak şu anlık bu kadar
                        </p>
                    </div>
                </div>
    </div>
    <script src="assets/libs/jquery/dist/jquery.min.js"></script>
</asp:Content>
