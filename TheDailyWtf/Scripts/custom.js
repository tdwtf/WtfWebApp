
$(document).ready(function () {

    $('.date-pair .time').timepicker({
        'showDuration': false,
        'timeFormat': 'g:ia'
    });

    $('.date-pair .date').datepicker({
        'format': 'yyyy-m-d',
        'autoclose': true
    });
});