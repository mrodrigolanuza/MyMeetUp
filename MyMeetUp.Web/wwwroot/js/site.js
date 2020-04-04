// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.


$(document).ready(function () {
    console.log("Documento preparado..");

    //DATATABLES
    // Añadir controles de interacción avanzada a las tablas HTML mediante "www.datatables.net"
    $('.datatable').DataTable();

    // Configuración del carrusel de imágenes de la pantalla Home principal
    $('#homeCarousel').carousel({
        interval: 3000
    });

    $('#homeCarousel').on('slid', function () {
        alert("Trasnsición realizada!");
    }); 

    //CARD-SHADOW
    $(".card").hover(
        function () {
            $(this).addClass('shadow-lg').css('cursor', 'pointer');
            console.log("Hover OUT");
        }, function () {
            $(this).removeClass('shadow-lg');
            console.log("Hover IN");
        }
    );
});


