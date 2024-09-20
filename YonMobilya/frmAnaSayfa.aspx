<%@ Page Title="" Language="C#" MasterPageFile="~/admin.Master" AutoEventWireup="true" CodeBehind="frmAnaSayfa.aspx.cs" Inherits="YonMobilya.frmAnaSayfa" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css" rel="stylesheet" />
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="https://code.jquery.com/ui/1.12.1/jquery-ui.js"></script>
    <style>
        #calendar {
            width: 100%;
            height: 100%;
            box-sizing: border-box;
            padding: 10px;
        }
        /* Tüm takvim hücrelerini aynı yükseklikte tutmak için CSS */
        #calendar td {
            vertical-align: top; /* İçeriğin üstten hizalanmasını sağlar */
            position: relative; /* İçerik hizalaması için pozisyon ayarı */
        }
        .ui-datepicker {
            width: 100%;
            height: 100%;
        }
        
        .ui-datepicker th {
            padding: 2px;
            position: relative;
            width: 17%;
            height: 100%;
        }
        .ui-datepicker td {
            padding: 2px;
            position: relative;
        }
        .event-count-container {
            margin-top: 5px;
        }

        .event-count {
            background-color: white;
            color: white;
            font-size: 10px;
            padding: 2px 5px;
            border-radius: 3px;
            margin-bottom: 2px; /* Her ID arasında boşluk bırakmak için */
        }
        .cur-name-link {
            text-decoration: none;
            color: blue;
            font-size: 20px;
        }

        .cur-name-link:hover {
            text-decoration: underline;
        }
        /*.event-count {
            position: absolute;
            top: 5px;
            right: 5px;
            background-color: blue;
        }*/

        .ui-state-disabled {
            background-color: #e0e0e0 !important; /* Pasif günlerin rengi */
            color: #a0a0a0 !important;
            cursor: not-allowed;
        }

        .button {
            background-color: #1c87c9;
            border: none;
            color: white;
            padding: 20px 34px;
            text-align: center;
            text-decoration: none;
            display: inline-block;
            font-size: 20px;
            margin: 4px 2px;
            cursor: pointer;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="page-wrapper">
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
                                <!-- Takvim burada olacak -->
                                <div id="calendar"></div>
                            </div>
                            <div id="selected-event-details"></div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <!--End content-wrapper-->
<script type="text/javascript">
    $(document).ready(function () {
        // MSSQL'den gelen işlem adetleri verisi eventCounts değişkenine aktarılmış durumda.

        // Takvim için Türkçe dil ayarlarını yapıyoruz
        $.datepicker.setDefaults({
            closeText: "Kapat",
            prevText: "&#x3C;geri",
            nextText: "ileri&#x3e",
            currentText: "Bugün",
            monthNames: ["Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran",
                "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık"],
            monthNamesShort: ["Oca", "Şub", "Mar", "Nis", "May", "Haz",
                "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara"],
            dayNames: ["Pazar", "Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma", "Cumartesi"],
            dayNamesShort: ["Pazar", "Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma", "Cumartesi"],
            dayNamesMin: ["Pazar", "Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma", "Cumartesi"],
            weekHeader: "Hf",
            dateFormat: "dd.mm.yy",
            firstDay: 1
        });

        // Takvimi oluşturuyoruz
        $("#calendar").datepicker({
            beforeShowDay: function(date) {
                var formattedDate = $.datepicker.formatDate('yy-mm-dd', date);
                var ids = eventCounts[formattedDate] || []; // İlgili tarihte ID var mı kontrol ediliyor
                
                if (ids.length > 0) {
                    return [true, "", ""]; // Aktif gün, CSS sınıfı boş ve açıklama yok
                } else {
                    return [false, "ui-state-disabled", "Pasif gün"]; // Eğer ID yoksa, günü pasif yapıyoruz
                }
            },
            onSelect: function(dateText, inst) {
                var selectedDate = $.datepicker.parseDate("dd.mm.yy", dateText);
                var formattedDate = $.datepicker.formatDate("yy-mm-dd", selectedDate);
                var ids = eventCounts[formattedDate]; // MSSQL'den gelen JSON verisi (birden fazla ID ve CURNAME olabilir)

                if (ids && ids.length > 0) {
                    // IDs dizisindeki her bir nesnenin ID ve CURNAME değerini ekliyoruz
                    $("#selected-event-details").empty(); // Önceki seçimleri temizle
                    ids.forEach(function(item) {
                        var curName = item.CURNAME;  // İlgili kişinin adı
                        var id = item.ID;  // İlgili kişinin ID'si
                        
                        // CURNAME'i tıklanabilir yapıyoruz, tıklayınca ID'yi kurulum.aspx'e gönderiyoruz
                        $("#selected-event-details").append(
                            '<div class="event-count">' +
                                '<a class="button btn btn-block btn-success" href="Montaj.aspx?curid=&salid=' + encodeURIComponent(id) + '" class="cur-name-link">'
                                     + curName + 
                                '</a>' +
                            '</div>'
                        );
                    });
                } else {
                    alert("Seçilen tarih için bir ID bulunamadı.");
                }
            }
        });

// Takvim render edildikten sonra hücrelere ID ve CURNAME ekliyoruz
        function addEventCounts() {
            setTimeout(function() {
                $("#calendar td").each(function() {
                    var cell = $(this);
                    var date = cell.data("year") + "-" +
                               ("0" + (cell.data("month") + 1)).slice(-2) + "-" +
                               ("0" + cell.text()).slice(-2);

                    if (eventCounts[date]) {
                        var data = eventCounts[date]; // Tarihe bağlı olarak gelen veriler (ID ve CURNAME)
                        cell.append('<div class="event-count-container"></div>');

                        // Her bir veriyi ayrı bir div içine ekleyip alt alta gösteriyoruz
                        data.forEach(function(item) {
                            var curName = item.CURNAME;  // İlgili kişinin adı
                            var id = item.ID;  // İlgili kişinin ID'si

                            // CURNAME'i tıklanabilir yapıyoruz, tıklayınca ID'yi kurulum.aspx'e gönderiyoruz
                            cell.find('.event-count-container').append(
                                '<div class="event-count">' +
                                '<h6 class="text-black-50">' + curName + '</h6>' +
                                '</div>'
                            );
                        });
                    }
                });
            }, 0);
        }
        var maxHeight = 0; // Maksimum yüksekliği saklayacağız

        // İlk olarak hücrelerin yüksekliğini ölç
        $("#calendar td").each(function() {
            var cell = $(this);
            var cellHeight = cell.outerHeight(); // Hücrenin yüksekliğini al
            if (cellHeight > maxHeight) {
                maxHeight = cellHeight; // Maksimum yüksekliği güncelle
            }
        });

        // Daha sonra tüm hücrelerin yüksekliğini eşitle
        $("#calendar td").each(function() {
            var cell = $(this);
            cell.css('height', maxHeight + 'px'); // Hücrelerin yüksekliğini ayarla
        });
        // İlk yüklemede event count'ları ekleyelim
        addEventCounts();
    });
    function onSelectDate(dateText) {
        var selectedDate = $.datepicker.parseDate("dd.mm.yy", dateText);
        var formattedDate = $.datepicker.formatDate("yy-mm-dd", selectedDate);
        var ids = eventCounts[formattedDate]; // Seçilen tarihe ait veriler

        if (ids && ids.length > 0) {
            var detailsHtml = ids.map(function (item) {
                return '<div>' + item.CURNAME + ' (ID: ' + item.ID + ')</div>';
            }).join('');

            // Seçilen tarihin detaylarını göstermek için
            $('#selected-event-details').html(detailsHtml);

            // Takvimi yeniden yüklemek için AJAX tetikleyin
            __doPostBack('UpdatePanel1', ''); // UpdatePanel'i tetikleyerek takvimi yeniler
        }
    }

</script>
</asp:Content>
