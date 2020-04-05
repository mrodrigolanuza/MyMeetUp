function anotarNuevoAsistenteAlEvento() {
    console.info("Entrada en la función..");
    $.ajax({
        url: "/Events/AddNewAttendant",
        type: "POST",
        data: {
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]',
                $("#__EventAttendanceAntiForgeryToken")).val(),
            userId: $("#userId").val(),
            eventId: $("#eventId").val()
        },
        success: function (data) {
            alert("Te esperamos!!");    
            if (data !== null) {
                
            }
        },
        error: function (xhr) {
            alert("error");
        },
        complete: function () {
            alert("Completado!!");
        }
    });
}