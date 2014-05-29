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
    qtipInit()
});