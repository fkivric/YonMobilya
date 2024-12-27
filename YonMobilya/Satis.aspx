<%@ Page Title="" Language="C#" MasterPageFile="~/admin.Master" AutoEventWireup="true" CodeBehind="Satis.aspx.cs" Inherits="YonMobilya.Satis" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta charset="UTF-8" lang="tr">
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
    <script type="text/javascript">
        function openDatePicker() {
            var dateInput = document.getElementById('<%= customerDATE.ClientID %>');
            dateInput.click(); // Takvimi açmak için tıklama simülasyonu
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="loading" id="loadingDiv" align="center" style="display: none;">
        <img src="img/islem_gerceklestiriliyor.gif" alt="Loading..." />
    </div>
    <script src="https://code.jquery.com/jquery-3.5.1.slim.min.js"></script>
    <script src="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/js/bootstrap.min.js"></script>
    <div runat="server" class="page-wrapper">
        <div class="container-fluid">
            <!-- ============================================================== 
            <!-- Start Page Content -->
            <!-- ============================================================== -->
            <div class="row">
                <div class="col-12">
                    <div class="card">
                        <div class="card-body">
                            <div class="row">
                                <!-- Column -->
                                <div class="col-md-6 col-lg-4 col-xlg-4">
                                    <div class="card card-hover">
                                        <div class="p-2 bg-primary text-center">
                                            <h6 class="text-white">Atama Bekleyen İşlem Adeti</h6>
                                            <h1 runat="server" id="toplamadet" class="font-light text-white"></h1>
                                        </div>
                                    </div>
                                </div>
                                <!-- Column -->
                                <div class="col-md-6 col-lg-4 col-xlg-4">
                                    <div class="card card-hover">
                                        <div class="p-2 bg-cyan text-center">
                                            <h6 class="text-white">Atama Bekleyen İşlem Ciirosu</h6>
                                            <h1 runat="server" id="toplamciro" class="font-light text-white"></h1>
                                        </div>
                                    </div>
                                </div>
                                <!-- Column -->
                                <div class="col-md-6 col-lg-4 col-xlg-4">
                                    <div class="card card-hover">
                                        <div class="p-2 bg-success text-center">
                                            <h6 class="text-white">Atama Bekleyen İşlem Hakedişi</h6>
                                            <h1 runat="server" id="hakedis" class="font-light text-white"></h1>
                                        </div>
                                    </div>
                                </div>
                                <!-- Column -->
                            </div>
                            <div class="row">
                                <!-- Column -->
                                <div class="col-md-6 col-lg-4 col-xlg-3">
                                    <div class="card card-hover">
                                        <div class="p-2 bg-success text-center">
                                            <h6 class="text-white">Atanan İşlem Adeti</h6>
                                            <h1 runat="server" id="tamamlananadet" class="font-light text-white"></h1>
                                        </div>
                                    </div>
                                </div>
                                <!-- Column -->
                                <div class="col-md-6 col-lg-4 col-xlg-3">
                                    <div class="card card-hover">
                                        <div class="p-2 bg-success text-center">
                                            <h6 class="text-white">Atanan İşlem Ciirosu</h6>
                                            <h1 runat="server" id="tamamalananciro" class="font-light text-white"></h1>
                                        </div>
                                    </div>
                                </div>
                                <!-- Column -->
                                <div class="col-md-6 col-lg-4 col-xlg-3">
                                    <div class="card card-hover">
                                        <div class="p-2 bg-success text-center">
                                            <h6 class="text-white">Atanan İşlem Hakedişi</h6>
                                            <h1 runat="server" id="tamamlananhakedis" class="font-light text-white"></h1>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="ml-auto">
                                <div class="recentOrders" style="overflow: auto">
                                    <div class="cardHeader">
                                        <div class="col-md-12">
                                            <div class="form-group">
                                                <h2 align="center">Bekleyen Müşteriler</h2>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="scroll">
                                        <asp:GridView runat="server" ID="GridView1" CssClass="gridView" BorderStyle="Solid" Width="100%" 
                                            AutoGenerateColumns="false" 
                                            AllowPaging="True" 
                                            PageSize="10"
                                            OnPageIndexChanging="GridView1_PageIndexChanging"
                                            OnRowCommand="GridView1_RowCommand" 
                                            OnRowCreated="GridView1_RowCreated"
                                            OnRowDataBound="GridView1_RowDataBound">
                                            <Columns>
                                                <asp:CommandField ItemStyle-CssClass="td2" ShowSelectButton="True" SelectText="Siparişi İncele" HeaderText="Sipariş Bilgisi" ItemStyle-HorizontalAlign="Center" ButtonType="Button"/>
                                                <asp:BoundField ItemStyle-CssClass="td" DataField="SALID" ReadOnly="True" />
                                                <asp:BoundField ItemStyle-CssClass="td" DataField="SALCURID" ReadOnly="True" />
                                                <asp:BoundField ItemStyle-CssClass="td" DataField="CURVAL" HeaderText="Müşteri Kodu" />
                                                <asp:BoundField ItemStyle-CssClass="td" DataField="CURNAME" HeaderText="Müşteri Adı" />
                                                <asp:BoundField ItemStyle-CssClass="td" DataField="CURCHCOUNTY" HeaderText="İlçe" />
                                                <asp:BoundField ItemStyle-CssClass="td" DataField="SALDATE" DataFormatString="{0:d}" HeaderText="Satış Tarihi" />
                                                <asp:BoundField ItemStyle-CssClass="td" DataField="SATISDIVNAME" HeaderText="SATIŞ Mağaza" />
                                                <asp:BoundField ItemStyle-CssClass="td" DataField="TESLIMDIVNAME" HeaderText="Alım Noktası" />
                                            </Columns>
                                            <PagerSettings Mode="Numeric" Position="Bottom"  />
                                            <PagerStyle CssClass="pager" ForeColor="White" HorizontalAlign="Center" />  
                                        </asp:GridView>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="card">
                        <div class="card-body">
                            <div class="ml-auto">
                                <div class="recentOrders" style="overflow: auto">
                                    <div class="cardHeader">
                                        <div class="col-md-12">
                                            <div class="form-group">
                                                <h2 align="center">Atanan Müşteriler</h2>
                                            </div>
                                        </div>
                                        <div class="card-title-elements">
                                            <div class="form-group">
                                                <h4 align="center">Tarih Filitresi</h4>
                                                <input class="col-md-4" type="date" id="StartDate" runat="server" />
                                                <input class="col-md-4" type="date" id="EndDate" runat="server" />
                                                <asp:Button runat="server" ID="Listele" CssClass="col-md-3" Text="Listele" OnClick="Listele_Click" />
                                            </div>
                                        </div>
                                    </div>
                                    <div class="scroll">
                                        <asp:GridView runat="server" ID="GridView2" CssClass="gridView" BorderStyle="Solid" Width="100%" 
                                            AutoGenerateColumns="false" 
                                            AllowPaging="True" 
                                            PageSize="10"
                                            OnPageIndexChanging="GridView2_PageIndexChanging" 
                                            OnRowCreated="GridView2_RowCreated" 
                                            OnRowCommand="GridView2_RowCommand">
                                            <Columns>
                                                <asp:CommandField ItemStyle-CssClass="td2" ShowSelectButton="True" SelectText="Siparişi İncele" HeaderText="Sipariş Bilgisi" ItemStyle-HorizontalAlign="Center" ButtonType="Button"/>
                                                <asp:BoundField ItemStyle-CssClass="td" DataField="MB_SALID" ReadOnly="True" />
                                                <asp:BoundField ItemStyle-CssClass="td" DataField="CURID" ReadOnly="True" />
                                                <asp:BoundField ItemStyle-CssClass="td" DataField="OFFCURID" ReadOnly="True" />
                                                <asp:BoundField ItemStyle-CssClass="td" DataField="CURVAL" HeaderText="Müşteri Kodu" />
                                                <asp:BoundField ItemStyle-CssClass="td" DataField="CURNAME" HeaderText="Müşteri Adı" />
                                                <asp:BoundField ItemStyle-CssClass="td" DataField="MB_PlanTarih" DataFormatString="{0:d}" HeaderText="Plan Tarihi" />
                                                <asp:BoundField ItemStyle-CssClass="td" DataField="OFFCURNAME" HeaderText="Montajcı Adı" />
                                                <asp:BoundField ItemStyle-CssClass="td" DataField="PRLPRICE" DataFormatString="{0:n2}" HeaderText="Tutar" />
                                            </Columns>
                                            <PagerSettings Mode="Numeric" Position="Bottom"  />
                                            <PagerStyle CssClass="pager" ForeColor="White" HorizontalAlign="Center" />
                                        </asp:GridView>
                                    </div>
                                </div>
                            </div>
                            <div class="loading" align="center">
                                <img src="img/islem_gerceklestiriliyor.gif" alt="" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <!-- Modal -->
    <div class="modal fade" id="full-width-modal" tabindex="-1" role="dialog" aria-labelledby="customerCURNAME" aria-hidden="true">
        <div class="modal-dialog modal-full-width">
            <div class="modal-content">
                <div class="modal-header text-center">
                    <a href="javascript:void(0)" class="text-success">
                        <span>
                            <img class="mr-2" src="assets/images/Yon%20Logo.png"
                                alt="" height="40">
                            <img src="assets/images/Yon%20Text.png" alt=""
                                height="40">
                        </span>
                    </a>
                </div>
                <div runat="server" id="NewOrOld" hidden="hidden"></div>
                <div class="modal-body">

                    <div class="form-group">
                        <label for="customerCURNAME">Müşteri Sistem Numarası</label>
                        <input class="form-control" type="text" id="customerCURNAME" required="" readonly="readonly">
<%--                    </div>

                    <div class="form-group">--%>
                        <label for="customerCURVAL">Müşteri Adı</label>
                        <input class="form-control" type="text" id="customerCURVAL" required=""  readonly="readonly"/>
                    </div>

                    <div class="form-group" style="border:solid">
                        <h3 align="center">Lütfen ürünleri seçin</h3>
                        <%--<label class="form-control" for="customerSALID" style="text-align:center;align-items:center">Ürünler</label>--%>
                        <asp:GridView runat="server" ID="Musteri" AutoGenerateColumns="false" CssClass="gridView" Width="100%" CellPadding="0" CellSpacing="0" OnRowCreated="Musteri_RowCreated" OnRowDataBound="Musteri_RowDataBound">
                            <HeaderStyle HorizontalAlign="Center" VerticalAlign="Middle" />
                            <Columns>
                            <asp:TemplateField ItemStyle-CssClass="align-items-lg-center">
                                    <ItemTemplate>
                                        <asp:CheckBox ID="chkSelect" runat="server" CssClass="td2 align-items-center fa-align-center"/>
                                    </ItemTemplate>
                                    <HeaderTemplate>
                                        <asp:label runat="server" ID="secmetin" Text="Seç" CssClass="td2"></asp:label>
                                        <%--<asp:CheckBox ID="chkSelectAll" runat="server" Text="Seç" />--%>
                                    </HeaderTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField ="CDRSALID" />
                                <asp:BoundField DataField ="ORDCHID" />
                                <asp:BoundField DataField ="PROID" />
                                <asp:BoundField ItemStyle-CssClass="td2" DataField="PROVAL" HeaderText="Ürün kodu" ItemStyle-VerticalAlign="Middle" ItemStyle-HorizontalAlign="Left"/>
                                <asp:BoundField ItemStyle-CssClass="td1" DataField="PRONAME" HeaderText="Ürün adı" ItemStyle-VerticalAlign="Middle"  ItemStyle-HorizontalAlign="Left"/>
                                <asp:BoundField ItemStyle-CssClass="td2" DataField="ORDCHBALANCEQUAN" HeaderText="Adet"  ItemStyle-VerticalAlign="Middle" ItemStyle-HorizontalAlign="Center"/>
                                <asp:BoundField ItemStyle-CssClass="td2" DataField="PRLPRICE" DataFormatString="{0:C2}" HeaderText="Tutar"  ItemStyle-VerticalAlign="Middle" ItemStyle-HorizontalAlign="Right"/>
                            </Columns>
                        </asp:GridView>
                            <div class="row gx-3 gy-2 align-items-center">
                                <div class="col-md-3">
                                    <label for="City">Teslimat Notu</label>
                                    <input runat="server" class="form-control" type="text" id="CDRPLNNOTES" required=""  readonly="readonly"/>
                                </div>
                                <div class="col-md-3">
                                    <label for="City">Teslimat İl</label>
                                    <input class="form-control" type="text" id="customerCity" required=""  readonly="readonly"/>
                                </div>
                                <div class="col-md-3">
                                    <label for="County">Teslimat İlçe</label>
                                    <input class="form-control" type="text" id="customerCounty" required=""  readonly="readonly"/>
                                </div>
                                <div class="col-md-3">
                                    <label for="Adres">Teslimat Adres</label>
                                    <textarea class="form-control" style="vertical-align:unset" id="customerAdres" rows="4" required="" readonly="readonly"></textarea>
                                </div>
                            </div>
                    <div class="form-group">
                        <label for="customerDATE">Plananlanan Kurulum Tarihi</label><br />
                        <input class="col-md-4" type="date" id="customerDATE" runat="server" />
                        <label for="smsvar">Müşteri Randevu Bilgi Mesajı Gidecek</label>
                        <asp:CheckBox runat="server" ID="smsvar" Checked="true"  CssClass="col-md-3" />
                        <label for="Montajci">Gidecek Montajcı</label>
                        <asp:DropDownList runat="server" ID="Montajci" CssClass="ddl col-md-4"></asp:DropDownList>
                    </div>
                </div>
                <div class="row gx-3 gy-2 align-items-center form-group text-center">
                    <asp:Button runat="server" ID="Onayla" CssClass="btn btn-outline-success" Width="50%" Text="Onayla" OnClick="Onayla_Click" />
                    <asp:Button runat="server" ID="Kapat" CssClass="btn btn-outline-danger"  Width="50%" Text="Kapat" OnClick="Kapat_Click" />
                </div>
            </div>
            <!-- /.modal-content -->
            </div>
        <!-- /.modal-dialog -->
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
