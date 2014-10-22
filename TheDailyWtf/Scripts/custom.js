
$(document).ready(function () {

    $('aside h5 a').click(function () {
        $(this).parent().next('div').toggleClass('hideNonDesktop');
        return false;
    });

    // show admin-only tasks if the author has logged in
    if (getCookie('IS_ADMIN') == '1') {
        $('.admin-only').show();
    }

});

//http://www.w3schools.com/js/js_cookies.asp
function getCookie(cname) {
    var name = cname + "=";
    var ca = document.cookie.split(';');
    for(var i=0; i<ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0)==' ') c = c.substring(1);
        if (c.indexOf(name) != -1) return c.substring(name.length,c.length);
    }
    return "";
}
