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
        .ddl
        {
            border:2px solid #7d6754;
            border-radius:5px;
            padding:3px;
            -webkit-appearance: none; 
            background-image:url('Images/Arrowhead-Down-01.png');
            background-position:88px;
            background-repeat:no-repeat;
            text-indent: 0.01px;/*In Firefox*/
            text-overflow: '';/*In Firefox*/
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
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="loading" id="loadingDiv" align="center" style="display: none;">
        <img src="img/islem_gerceklestiriliyor.gif" alt="Loading..." />
    </div>
    <script src="https://code.jquery.com/jquery-3.5.1.slim.min.js"></script>
    <script src="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/js/bootstrap.min.js"></script>
    <div class="page-wrapper">
        <div class="container-fluid">
            <div class="row">
                <div class="col-12">
                    <div class="card">
                        <h2 class="card-text">Montaj Bekleyen İşlemler</h2>
                        <div class="card-body">
                            <div class="row">
                                <div class="table-responsive">
                                    <asp:GridView ID="GridView1" runat="server" CssClass="gridView"
                                        AutoGenerateColumns="False" 
                                        AllowPaging="True" 
                                        PageSize="10" 
                                        OnPageIndexChanging="GridView1_PageIndexChanging" 
                                        OnRowCreated="GridView1_RowCreated" 
                                        OnSelectedIndexChanged="GridView1_SelectedIndexChanged">
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
                                            <PagerSettings Mode="Numeric" Position="Bottom"  />
                                            <PagerStyle CssClass="pager" ForeColor="White" HorizontalAlign="Center" />
                                    </asp:GridView>
                                </div>
                            </div>
                            <div class="loading" align="center">
                                <img src="img/islem_gerceklestiriliyor.gif" alt="" />
                            </div>
                            <%--<div id="paginationContainer" class="float-right"></div>--%>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <script type="text/javascript" src="https://ajax.googleapis.com/ajax/libs/jquery/1.8.3/jquery.min.js"></script>
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
    <style>
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
    </style>
    <script type="text/javascript">
        function showModal() {
            // Modal'ı göster
            $('#full-width-modal').modal('show');
        }
    </script>
    <script type="text/javascript">
        function HideModal() {
            // Modal'ı göster
            $('#full-width-modal').modal('hide');
        }
    </script>
</asp:Content>
