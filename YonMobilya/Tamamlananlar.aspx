<%@ Page Title="" Language="C#" MasterPageFile="~/admin.Master" AutoEventWireup="true" CodeBehind="Tamamlananlar.aspx.cs" Inherits="YonMobilya.Tamamlananlar" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div runat="server" class="page-wrapper">
        <div class="container-fluid">
            <div class="Row">
                <div class="col-12">
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
                                                <asp:Repeater ID="ProductRepeater" runat="server">
                                                    <HeaderTemplate>
                                                        <table id="zero_config" class="table table-hover table-flush" border="1">
                                                            <thead>
                                                                <tr>
                                                                    <th>Yön Stok Adı (Seç)</th>
                                                                    <th>Sitede Aktif</th>
                                                                    <th>Tanımlı Resim Adeti</th>
                                                                </tr>
                                                            </thead>
                                                            <tbody>
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <tr>
                                                            <td data-label="Adı (Seç)=">
                                                            </td>
                                                            <td data-label="Aktif="></td>
                                                            <td data-label="Resim Adeti="></td>
                                                        </tr>
                                                    </ItemTemplate>
                                                    <FooterTemplate>
                                                        </tbody>
                                                        </table>               
                                                    </FooterTemplate>
                                                </asp:Repeater>
                                                <div class="pagination" id="pagination">
                                                    <asp:Literal ID="litPagination" runat="server"></asp:Literal>
                                                </div>
                                            </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
