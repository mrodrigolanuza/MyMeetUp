function ManageUserGroupMembership() {
    var registerGroupButton = document.getElementById('register-group-button');
    if (registerGroupButton.innerText == "Unirse a este Grupo") {
        RegisterNewMemberToGroup();
    }
    else {
        UnregisterMemberFromGroup();
    }
}

function RegisterNewMemberToGroup() {
    console.log("Entrada en la función RegisterNewMemberToGroup..");
    $.ajax({
        url: "/Groups/RegisterNewMember",
        type: "POST",
        data: {
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]',
                $("#__GroupDetailsButtonsAntiForgeryToken")).val(),
            userId: $("#userId").val(),
            groupId: $("#groupId").val()
        },
        success: function (response) {
            console.log("Entrando en Success..");
            //Cambiar el aspecto del botón para poder liberar la plaza
            document.getElementById("register-group-button").className = "btn btn-danger float-right";
            document.getElementById("register-group-button").innerText = "Abandonar este Grupo";
        },
        error: function (response) {
            console.error("Error producido en RegisterNewMemberToGroup!");
        },
        complete: function () {
            console.log("Llamada AJAX completada");
            //UpdateNumTotalGroupMembers();
        }
    });
}

function UnregisterMemberFromGroup() {
    console.log("Entrada en la función UnregisterMemberFromGroup..");
    $.ajax({
        url: "/Groups/UnregisterMember",
        type: "POST",
        data: {
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]',
                $("#__GroupDetailsButtonsAntiForgeryToken")).val(),
            userId: $("#userId").val(),
            groupId: $("#groupId").val()
        },
        success: function (response) {
            console.log("Entrando en Success..");
            //Cambiar aspecto de botón para poder asistir al evento nuevamente.
            document.getElementById("register-group-button").className = "btn btn-info float-right";
            document.getElementById("register-group-button").innerText = "Unirse a este Grupo";
        },
        error: function (response) {
            console.error("Error producido en LeaveMySeat!");
        },
        complete: function () {
            console.log("Llamada AJAX completada");
            //UpdateNumTotalGroupMembers();
        }
    });
}