<%@ Page Title="" Language="C#" MasterPageFile="~/admin.Master" AutoEventWireup="true" CodeBehind="MonajFormu.aspx.cs" Inherits="YonMobilya.MonajFormu" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        @media print {
            @page {
                size: A4;
                margin: 5mm;
            }

            body {
                margin: 0;
            }
        }
        .header, .footer {
            text-align: center;
            padding: 0;
        }

        .content {
            text-align: justify;
        }

        .right-align {
            display: flex;
            justify-content: flex-end;
        }

        table {
            width: 100%;
            border-collapse: collapse;
        }

        table, th, td {
            border: 1px solid black;
        }

        th, td {
            padding: 6px;
            text-align: left;
        }

        .table-container {
            overflow: hidden;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div runat="server" class="page-wrapper">
        <img src="assets/images/Yon%20Logo.png" style="resize: initial" alt="homepage" class="light-logo" />
        <img src="assets/images/Yon%20Text.png" style="resize: initial" class="light-logo" alt="homepage" />
        <div class="header">
            <h1>ÜRÜN TESLİM FORMU</h1>
        </div>
        <div class="content">
            <div class="right-align">
                <table style="width: 300px">
                    <tr>
                        <td><strong>SATIŞ MAĞAZA:</strong></td>
                        <td>
                            <p><span runat="server" id="magaza"></span></p>
                        </td>
                    </tr>
                    <tr>
                        <td><strong>MONTAJ TARİHİ:</strong></td>
                        <td>
                            <p><span runat="server" id="tarih"></span></p>
                        </td>
                    </tr>
                </table>
            </div>
            <p><strong>MÜŞTERİ ADI:</strong> <span runat="server" id="musteri"></span></p>
            <p><strong>ADRES:</strong> <span runat="server" id="adres"></span></p>
            <p><strong>TEL:</strong> <span runat="server" id="tel"></span></p>
            <br />
        </div>
        <br />
        <asp:Repeater ID="ProductRepeater" runat="server">
            <HeaderTemplate>
                <table>
                    <thead>
                        <tr>
                            <th>ÜRÜN ADI</th>
                            <th>KODU</th>
                            <th>ADET</th>
                            <th>AÇIKLAMA</th>
                            <th>SONUÇ</th>
                        </tr>
                    </thead>
                    <tbody>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td data-label="ÜRÜN KODU"><%# Eval("Kodu") %></td>
                    <td data-label="ÜRÜN ADI"><%# Eval("UrunAdi") %></td>
                    <td data-label="Adet"><%# Eval("Adet") %></td>
                    <td data-label="AÇIKLAMA"><%# Eval("Aciklama") %></td>
                    <td></td>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                </tbody>
                    </table>               
            </FooterTemplate>
        </asp:Repeater>
        <br />
        <div class="tab-content">
            <table>
                <tr>
                    <th colspan="2" align="center">
                        <p align="center">Yön AVM® Aldığım Ürünlerin Sorunsuz ve Eksiksiz Tarafımdan Teslim Alınmıştır.</p>
                    </th>
                </tr>
                <tr>
                    <td>
                        <p><strong>MÜŞTERİ ADI SOYADI:</strong> <span runat="server" id="musteriimza"></span></p>
                    </td>
                    <td>
                        <p><strong>EKİP ADI SOYADI:</strong> <span runat="server" id="kurlumcu"></span></p>
                    </td>
                </tr>
                <tr>
                    <td>
                        <p><strong>MÜŞTERİ İMZASI:</strong></p>
                    </td>
                    <td>
                        <p><strong>EKİP İMZASI:</strong>></p>
                    </td>
                </tr>
            </table>
        </div>
        <%--<asp:Literal ID="Literal1" runat="server"></asp:Literal>--%>
    </div>
    <script src="assets/libs/jquery/dist/jquery.min.js"></script>
</asp:Content>
