<%@ Page Title="" Language="C#" MasterPageFile="~/admin.Master" AutoEventWireup="true" CodeBehind="Onayli.aspx.cs" Inherits="YonMobilya.Onayli" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
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
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <script type="text/javascript">
        function showLoading() {
            document.getElementById('loadingDiv').style.display = 'block'; // Loading alanını göster
        }

        function hideLoading() {
            document.getElementById('loadingDiv').style.display = 'none'; // Loading alanını gizle
        }
    </script>
    <div class="page-wrapper">
    <div class="loading" id="loadingDiv" align="center" style="display:none;">
        <img src="img/islem_gerceklestiriliyor.gif" alt="Loading..." />
    </div>
        <div class="container-fluid">
            <div class="row">
                <div class="col-12">
                    <div class="card">
                        <h2 class="card-text">Ay içi İşlem Adeti</h2>
                        <div class="card-body">
                            <div class="row">
                                <!-- Column -->
                                <div class="col-md-6 col-lg-4 col-xlg-4">
                                    <div class="card card-hover">
                                        <div class="p-2 bg-primary text-center">
                                            <h6 class="text-white">Toplam İşlem Adeti</h6>
                                            <h1 runat="server" id="toplamadet" class="font-light text-white"></h1>
                                        </div>
                                    </div>
                                </div>
                                <!-- Column -->
                                <div class="col-md-6 col-lg-4 col-xlg-4">
                                    <div class="card card-hover">
                                        <div class="p-2 bg-cyan text-center">
                                            <h6 class="text-white">Toplam İşlem Ciirosu</h6>
                                            <h1 runat="server" id="toplamciro" class="font-light text-white"></h1>
                                        </div>
                                    </div>
                                </div>
                                <!-- Column -->
                                <div class="col-md-6 col-lg-4 col-xlg-4">
                                    <div class="card card-hover">
                                        <div class="p-2 bg-success text-center">
                                            <h6 class="text-white">Toplam İşlem Hakedişi</h6>
                                            <h1 runat="server" id="hakedis" class="font-light text-white"></h1>
                                        </div>
                                    </div>
                                </div>
                                <!-- Column -->
                            </div>
                            <div class="table-responsive">
                                <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" CssClass="table table-striped table-bordered no-wrap"
                                    AllowPaging="True" PageSize="20" OnPageIndexChanging="GridView1_PageIndexChanging" OnRowCreated="GridView1_RowCreated" OnSelectedIndexChanged="GridView1_SelectedIndexChanged">
                                    <Columns>
                                        <asp:CommandField ShowSelectButton="True" SelectText="Kurulumu Başlat" HeaderText="Sipariş Bilgisi" ItemStyle-HorizontalAlign="Center" ButtonType="Button" ItemStyle-CssClass="f-icon" />
                                        <asp:BoundField DataField="CDRSALID" HeaderText="CDRSALID" />
                                        <asp:BoundField DataField="CDRCURID" HeaderText="CDRCURID" />
                                        <asp:BoundField DataField="CURVAL" HeaderText="CURVAL" />
                                        <asp:BoundField DataField="CURNAME" HeaderText="MÜŞTERİ" />
                                        <asp:BoundField DataField="ORDDATE" HeaderText="SATIŞ TARİHİ" DataFormatString="{0:yyyy/MM/dd}" />
                                        <asp:BoundField DataField="ORDCHBALANCEQUAN" HeaderText="ADET" />
                                        <asp:BoundField DataField="PRLPRICE" HeaderText="FİYAT" />
                                        <asp:BoundField DataField="CURCHCOUNTY" HeaderText="SEMT" />
                                    </Columns>
                                    <PagerSettings Mode="Numeric" Position="Bottom" />
                                </asp:GridView>
                            </div>
                            <div id="paginationContainer" class="float-right"></div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
<script src="assets/libs/jquery/dist/jquery.min.js"></script>
</asp:Content>
