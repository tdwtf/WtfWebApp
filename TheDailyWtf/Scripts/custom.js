
$(document).ready(function () {

    //$('.date-pair .time').timepicker({
    //    'showDuration': false,
    //    'timeFormat': 'g:ia'
    //});

    //$('.date-pair .date').datepicker({
    //    'format': 'yyyy-m-d',
    //    'autoclose': true
    //});

    $('aside h5 a').click(function () {
        $(this).parent().next('div').toggleClass('hideNonDesktop');
        return false;
    });

});