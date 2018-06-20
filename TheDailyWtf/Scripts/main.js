console.log("Main.js Loaded");

function qtipInit(){
    $('.tooltip').each(function() {
        var message = $(this).data("tooltip");
        $(this).qtip({
            content: {
                text: message
            },
            position: {
                target: 'mouse',
                adjust: { x: 5, y:5 }
            },
            style: 'qtip-tipsy'
        });
    });
}

$(window).ready(function(){
    qtipInit();

    $('.approve-comment-button').parent().on('submit', function (e) {
        e.preventDefault();
        var form = $(this);
        $.post(form.attr('action') + '?no-redirect=1', form.serialize(), function () {
            form.parents('li').find('.comment-moderation').slideUp();
        }).fail(function () {
            form.off('submit').submit();
        });
    });
});
