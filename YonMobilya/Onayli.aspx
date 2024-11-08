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
    <style>
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
    <script type="text/javascript">
        function showLoading() {
            document.getElementById('loadingDiv').style.display = 'block'; // Loading alanını göster
        }

        function hideLoading() {
            document.getElementById('loadingDiv').style.display = 'none'; // Loading alanını gizle
        }
    </script>
    <div class="page-wrapper">
        <div class="loading" id="loadingDiv" align="center" style="display: none;">
            <img src="img/islem_gerceklestiriliyor.gif" alt="Loading..." />
        </div>
        <div class="container-fluid">
            <div class="row">
                <div class="col-12">
                    <div class="card">
                        <h2 class="card-text">Montaj Bekleyen İşlemler</h2>
                        <div class="card-body">
                            <div class="row">
                                <!-- Column -->
                                <div class="col-md-12 col-lg-12 col-xlg-12">
                                    <div class="card card-hover">
                                        <div class="p-2 bg-primary text-center">
                                            <h6 class="text-white">Toplam İşlem Adeti</h6>
                                            <h1 runat="server" id="toplamadet" class="font-light text-white"></h1>
                                        </div>
                                    </div>
                                </div>
                                <!-- Column -->
<%--                                <div class="col-md-6 col-lg-4 col-xlg-4">
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
                                <!-- Column -->--%>
                            </div>

                            <div class="row">
                                <div class="table-responsive">
                                    <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" CssClass="gridView"
                                        AllowPaging="True" PageSize="20" OnPageIndexChanging="GridView1_PageIndexChanging" OnRowCreated="GridView1_RowCreated" OnSelectedIndexChanged="GridView1_SelectedIndexChanged">
                                        <Columns>
                                            <asp:CommandField ShowSelectButton="True" SelectText="Kurulumu Başlat" HeaderText="Sipariş Bilgisi" ItemStyle-HorizontalAlign="Center" ButtonType="Button" ItemStyle-CssClass="f-icon" />
                                            <asp:BoundField ItemStyle-CssClass="td2" DataField="CDRSALID" HeaderText="CDRSALID" />
                                            <asp:BoundField ItemStyle-CssClass="td2" DataField="CDRCURID" HeaderText="CDRCURID" />
                                            <asp:BoundField ItemStyle-CssClass="td2" DataField="CURVAL" HeaderText="CURVAL" />
                                            <asp:BoundField ItemStyle-CssClass="td2" DataField="CURNAME" HeaderText="MÜŞTERİ" />
                                            <asp:BoundField ItemStyle-CssClass="td2" DataField="CURCHCOUNTY" HeaderText="SEMT" />
                                            <asp:BoundField ItemStyle-CssClass="td2" DataField="DIVNAME" HeaderText="ALINACAK YER" />
                                            <asp:BoundField ItemStyle-CssClass="td2" DataField="ORDDATE" HeaderText="SATIŞ TARİHİ" DataFormatString="{0:yyyy/MM/dd}" />
                                            <asp:BoundField ItemStyle-CssClass="td2" DataField="ORDCHBALANCEQUAN" HeaderText="ADET" />
                                            <asp:BoundField ItemStyle-CssClass="td1" DataField="ADRESS" HeaderText="ADRES" />
                                            <asp:BoundField ItemStyle-CssClass="td" DataField="TELEFON" HeaderText="TELEFON" />
                                        </Columns>
                                        <PagerSettings Mode="Numeric" Position="Bottom" />
                                    </asp:GridView>
                                </div>
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
