$(document).ready(function () {
    $('#layout').click(function () {
        HideForm( $("#form_login") );
        HideForm( $("#form_registration") );
        HideForm( $("#list_services_login") );
    })
});

function ShowServicesLogin() {
    $("#list_services_login").css("display", "block");
    $("#list_services_login").animate(
        {
            opacity: 1
        }, 250);
}
function ShowFormLogin() {
    ShowServicesLogin()
    $("#form_login").css("display", "block");
    $("#form_login").animate(
        {
            opacity: 1
        }, 250);
    HideForm( $("#form_registration") );
}
function ShowFormRegistration() {
    ShowServicesLogin()
    $("#form_registration").css("display", "block");
    $("#form_registration").animate(
        {
            opacity: 1
        }, 250);
    HideForm($("#form_login"));
}
function HideForm( form ) {
    form.css("display", "none");
    form.css("opacity", "0");
}
