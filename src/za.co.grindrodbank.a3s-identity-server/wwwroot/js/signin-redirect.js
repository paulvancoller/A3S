var dtf = null;

dtf = setTimeout(function () {
    window.location.href = document.getElementById("RedirectUrl").value;
}, 5000);

// 2FA click event 
var btn2FA = document.getElementById("btnTwoFA")
if (btn2FA) {
    btn2FA.addEventListener("click", function () {
        clearTimeout(dtf);
        window.location.href = document.getElementById("TwoFAUrl").value;
    });
}

// Redirect click event
var btnContinueWithRedirect = document.getElementById("btnContinueWithRedirect")
if (btnContinueWithRedirect) {
    btnContinueWithRedirect.addEventListener("click", function () {
        clearTimeout(dtf);
        window.location.href = document.getElementById("RedirectUrl").value;
    });
}
