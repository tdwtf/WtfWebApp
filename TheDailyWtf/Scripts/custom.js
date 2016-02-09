
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

$(document).ready(function () {
    if ($("#comment-form").length) {
        if (getCookie("tdwtf_token_name") === "") {
            if ("tdwtf_anon_name" in localStorage) {
                $("#comment-name").val(localStorage["tdwtf_anon_name"]);
            }
            $("#comment-form").submit(function () {
                if (!$("#g-recaptcha-response").val()) {
                    $(".field.g-recaptcha").addClass("error message");
                    return false;
                }
                localStorage["tdwtf_anon_name"] = $("#comment-name").val();
            });
        } else {
            // logged-in users don't need to solve the captcha.
            var name = getCookie("tdwtf_token_name");
            $("#comment-name").val(name).attr("disabled", "");
            $(".field.g-recaptcha").hide();
            $(".comment-anonymous-only").hide();
            $(".comment-edit-link").filter(function () {
                if ($(this).attr("data-user") == name) {
                    $(this).removeClass("hide");
                }
            });
        }
    }

    var lastCommentDelete = null;
    $(".comment-delete").click(function (e) {
        var currentComment = this;
        if (e.shiftKey && lastCommentDelete) {
            var seen = 0;
            $(".comment-delete").each(function () {
                if (this == currentComment || this == lastCommentDelete) {
                    seen++;
                } else if (seen == 1) {
                    this.checked = !this.checked;
                }
            });
        }
        lastCommentDelete = currentComment;
    });
    $("#real-delete-comment-form").submit(function () {
        $(".comment-delete").each(function () {
            $("#real-" + this.id).prop("checked", this.checked);
        });
    });
});
