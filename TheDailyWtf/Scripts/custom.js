
$(document).ready(function () {

    $('aside h5 a').click(function () {
        $(this).parent().next('div').toggleClass('hideNonDesktop');
        return false;
    });

});