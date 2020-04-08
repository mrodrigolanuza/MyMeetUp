function ManageUserEventAttendance() {
    var registerEventButton = document.getElementById('register-event-button');
    if (registerEventButton.innerText == "Quiero Asistir!") {
        RegisterNewAttendeeToEvent();
    }
    else {
        LeaveMySeat();
    }
}

function RegisterNewAttendeeToEvent() {
    console.log("Entrada en la función RegisterNewAttendeeToEvent..");
    $.ajax({
        url: "/Events/AddNewAttendee",
        type: "POST",
        data: {
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]',
                $("#__EventAttendanceAntiForgeryToken")).val(),
            userId: $("#userId").val(),
            eventId: $("#eventId").val()
        },
        success: function (response) {
            console.log("Entrando en Success..");
            //Cambiar el aspecto del botón para poder liberar la plaza
            document.getElementById("register-event-button").className = "btn btn-danger float-right";
            document.getElementById("register-event-button").innerText = "Liberar Mi Plaza!";
        },
        error: function (response) {
            console.error("Error producido en RegisterNewAttendeeToEvent!");
        },
        complete: function () {
            console.log("Llamada AJAX completada");
            UpdateAttendeesToEvent();
        }
    });
}

function LeaveMySeat() {
    console.log("Entrada en la función LeaveMySeat..");
    $.ajax({
        url: "/Events/LeaveMySeat",
        type: "POST",
        data: {
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]',
                $("#__EventAttendanceAntiForgeryToken")).val(),
            userId: $("#userId").val(),
            eventId: $("#eventId").val()
        },
        success: function (response) {
            console.log("Entrando en Success..");
            //Cambiar aspecto de botón para poder asistir al evento nuevamente.
            document.getElementById("register-event-button").className = "btn btn-info float-right";
            document.getElementById("register-event-button").innerText = "Quiero Asistir!";
        },
        error: function (response) {
            console.error("Error producido en LeaveMySeat!");
        },
        complete: function () {
            console.log("Llamada AJAX completada");
            UpdateAttendeesToEvent();
        }
    });
}

function UpdateAttendeesToEvent() {
    console.log("Actualizar listado de asistentes");
    $.ajax({
        url: "/Events/AttendeesList",
        type: "GET",
        data: {
            eventId: $("#eventId").val()
        },
        success: function (response) {
            console.log("Entrando en Success..");
            $("#attendees-to-event").html(response);
        },
        error: function (response) {
            console.error("Error producido en UpdateAttendeesList!");
        },
        complete: function (response) {
            console.log("Llamada AJAX completada");
        }
    });
}

function SendComment() {
    console.log("Enviando comentario..");
    $.ajax({
        url: "/Events/NewComment",
        type: "POST",
        data: {
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]',
                $("#__EventCommentAntiForgeryToken")).val(),
            userId: $("#userId").val(),
            eventId: $("#eventId").val(),
            commentMessage: $("#commentMessage").val()
        },
        success: function (response) {
            console.log("Entrando en Success.. SendComment");
            $("#event-comments").html(response);
        },
        error: function (response) {
            console.log("Entrando en Error..");
        },
        complete: function (response) {
            console.log("Entrando en Complete.. SendComment");
        }
    });
}