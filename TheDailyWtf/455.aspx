<%@ Page Language="C#" %>
<%@ Import Namespace="System.Reflection" %>
<% Response.StatusCode = 404; %>
<script runat="server">
    string copyright = typeof(WtfViewModelBase).Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)
                .Cast<AssemblyCopyrightAttribute>()
                .First()
                .Copyright;
</script>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>404 - Not Found</title>
    <meta charset="utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge,chrome=1" />
    <link rel="icon" href="/favicon.ico" />
    <link rel="stylesheet" href="/content/css/gumby.css" />
    <link rel="stylesheet" href="/content/css/custom.css" />
</head>
<body>
    <div class="navcontain">
        <div class="navbar dailywtf" id="nav3">
            <div class="row">
                <div class="three columns">
                    <a href="/">
                        <img src="/content/images/wtf-logo.png" style="padding-top:15px;" />
                    </a>
                </div>
            </div>
        </div>
    </div>
    <div id="wrapper">
        <div class="row">
            <div class="twelve columns">
                <h1>455 - User is a Jackass</h1>
                <p>
                    We don't want to fulfill this request because you're a 455hole.
                </p>
                <p>
                    <a href="/">&laquo; Return to Home Page</a>
                </p>
            </div>
        </div>
    </div>

    <div class="container">
        <footer>
            <div class="row horizontal">
                <hr />
            </div>
            <div class="row">
                <p>
                    <%= copyright %>
                </p>
            </div>
        </footer>
    </div>
</body>
</html>
