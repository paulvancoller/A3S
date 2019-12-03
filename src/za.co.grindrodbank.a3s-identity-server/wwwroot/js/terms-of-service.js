// Agree click event
var chkAccepted = document.getElementById("Accepted")
if (chkAccepted) {
    chkAccepted.addEventListener("click", function () {

        var btnAccept = document.getElementById("btnAccept")
        if (chkAccepted) {
            btnAccept.disabled = !chkAccepted.checked;
        }

    });
}
