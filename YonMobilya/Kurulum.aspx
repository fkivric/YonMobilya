<%@ Page Title="" Language="C#" MasterPageFile="~/admin.Master" AutoEventWireup="true" CodeBehind="Kurulum.aspx.cs" Inherits="YonMobilya.Kurulum" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="dist/css/core.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <script>
        $(document).ready(function () {
            // DropDownList'e arama özelliği ekleme
            $("#search-box").autocomplete({
                source: function (request, response) {
                    var matches = $.map($("#Depo option"), function (option) {
                        var text = $(option).text();
                        if (text.toLowerCase().indexOf(request.term.toLowerCase()) !== -1) {
                            return {
                                label: text,
                                value: $(option).val()
                            };
                        }
                    });
                    response(matches);
                },
                select: function (event, ui) {
                    $("#Depo").val(ui.item.value).trigger('change');
                }
            });
        });
    </script>
    <div class="page-wrapper">
        <!-- Container fluid  -->
        <!-- ============================================================== -->
        <div class="container-fluid">
            <!-- ============================================================== -->
            <!-- Start Page Content -->
            <!-- ============================================================== -->
            <div class="row">
                <div class="col-md-12">
                    <div class="card">
                        <div class="card-body">
                            <h4 class="card-title">Mağaza Teşhir Kurulum Talebi</h4>
                            <div class="card mb-4">
                                <h5 class="card-header">Harici veya Kendi Lokasyonundanki Envanterden Ürün Seçimi</h5>
                                <div class="card-body">
                                    <div class="col-md-3">
                                        <asp:CheckBox runat="server" ID="filtreaktif" Checked="false" Text="   Ürün filitresi ile Ara" OnCheckedChanged="filtreaktif_CheckedChanged" AutoPostBack="true" />
                                    </div>
                                    <div class="row gx-3 gy-2 align-items-center">
                                        <div class="col-md-3">
                                            <label class="form-label" for="Depo">Alım Yapılacak Depo</label>
                                            <asp:DropDownList runat="server" ID="Depo" class="form-select color-dropdown" OnSelectedIndexChanged="Depo_SelectedIndexChanged" AutoPostBack="true">
                                            </asp:DropDownList>
                                        </div>
                                        <div class="col-md-3" runat="server" id="MarkaFiltresi" visible="false">
                                            <label class="form-label" for="Marka">Marka</label>
                                            <asp:DropDownList runat="server" ID="Marka" class="form-select color-dropdown" OnSelectedIndexChanged="Marka_SelectedIndexChanged" AutoPostBack="true">
                                                <asp:ListItem Value="">Seçiniz...</asp:ListItem>
                                            </asp:DropDownList>
                                        </div>
                                        <div class="col-md-3" runat="server" id="ModelFiltresi" visible="false">
                                            <label class="form-label" for="Model">Model</label>
                                            <asp:DropDownList runat="server" ID="Model" class="form-select color-dropdown" OnSelectedIndexChanged="Model_SelectedIndexChanged" AutoPostBack="true">
                                                <asp:ListItem Value="">Seçiniz...</asp:ListItem>
                                            </asp:DropDownList>
                                        </div>
                                        <div class="col-md-3">
                                            <label class="form-label" for="Listele" runat="server" id="filitremetini">Seçili Depo Envanterini Göster</label>
                                            <asp:Button runat="server" ID="Listele" CssClass="d-block btn waves-effect waves-light btn-primary" Text="Listele" Enabled="false" OnClick="Listele_Click" OnClientClick="" />
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <!-- end card-body-->
                        <div class="details">
                            <div class="recentOrders" style="overflow: auto">
                                <div class="cardHeader">
                                    <div class="col-md-12">
                                        <div class="form-group">
                                            <label>Ürün Ara <strong class="">(Enter)</strong></label>
                                            <asp:TextBox runat="server" ID="Search" CssClass="form-control" placeholder="Ürün Adı" OnTextChanged="Search_TextChanged"></asp:TextBox>
                                        </div>
                                        <h2>İlk 20 Ürün Listesi</h2>
                                    </div>
                                </div>
                                <asp:GridView ID="grid" runat="server" CssClass="table table-striped table-bordered no-wrap"
                                    AutoGenerateColumns="False"
                                    AllowPaging="True" PageSize="20"
                                    ShowFooter="True" ShowHeaderWhenEmpty="True"
                                    OnRowCommand="grid_RowCommand" 
                                    OnPageIndexChanging="grid_PageIndexChanging" 
                                    OnRowCreated="grid_RowCreated" 
                                    OnRowDataBound="grid_RowDataBound">
                                    <Columns>
                                        <asp:ButtonField ButtonType="Button" CommandName="Select" Text="Seç" />
                                        <asp:BoundField DataField="PROID" ReadOnly="true" />
                                        <asp:BoundField DataField="PROVAL" HeaderText="Ürün Kodu" />
                                        <asp:BoundField DataField="PRONAME" HeaderText="Ürün Adı" />
                                        <asp:BoundField DataField="adet" HeaderText="Envanter Adet" />
                                        <asp:TemplateField HeaderText="Envanter Adet" ItemStyle-Width="100px">
                                            <ItemTemplate>
                                                    <%# Eval("adet") %>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    Toplam: <asp:Label ID="lblTotalAdet" runat="server" Text="0"></asp:Label>
                                                </FooterTemplate>
                                            </asp:TemplateField>
                                        <%--<asp:BoundField DataField="Size" HeaderText="Size" ItemStyle-Width="80px" />--%>
                                    </Columns>
                                    <FooterStyle BackColor="#CCCCCC" />
                                    <PagerSettings Mode="Numeric" Position="Bottom" PageButtonCount="10" />                                    
                                </asp:GridView>
                            </div>
                        </div>

                        <div runat="server" id="secililiste" class="details">
                            <div class="recentOrders" style="overflow: auto">
                                <div class="cardHeader">
                                    <div class="form-group">
                                        <h2>seçili liste</h2>
                                    </div>
                                </div>
                                <div class="col-md-12">
                                    <asp:GridView ID="Selected" runat="server" CssClass="table table-striped table-bordered no-wrap"
                                        AutoGenerateColumns="False"
                                        AllowPaging="True"
                                        OnRowCreated="Selected_RowCreated">
                                        <Columns>
                                            <asp:TemplateField HeaderText="Seçim" ItemStyle-Width="100px">
                                                <ItemTemplate>
                                                    <asp:CheckBox ID="chkSelect" runat="server" />
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:ButtonField ButtonType="Button" CommandName="sil" Text="Sil" />
                                            <asp:BoundField DataField="PROID" ReadOnly="true" />
                                            <asp:BoundField DataField="PROVAL" HeaderText="Ürün Kodu" />
                                            <asp:BoundField DataField="PRONAME" HeaderText="Ürün Adı" />
                                            <asp:BoundField DataField="adet" HeaderText="Seçili Adet" />
                                        </Columns>
                                    </asp:GridView>
                                </div>
                            </div>
                        </div>
                        <asp:Button runat="server" ID="GorevAta" Text="Secili Listeyi Onayla" CssClass="btn btn-success" />
                    </div>
                    <!-- end card-->
                </div>
                <!-- end col-->
            </div>
        </div>
    </div>
    <script src="assets/libs/jquery/dist/jquery.min.js"></script>
</asp:Content>
