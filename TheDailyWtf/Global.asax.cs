using System;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Newtonsoft.Json;
using TheDailyWtf.Security;

namespace TheDailyWtf
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : HttpApplication
    {
        public MvcApplication()
        {
            this.AuthenticateRequest += MvcApplication_AuthenticateRequest;
            this.BeginRequest += MvcApplication_BeginRequest;
        }

        private void MvcApplication_BeginRequest(object sender, EventArgs e)
        {
            SetCustomDateFormat();
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            
            AdRotator.Initialize(WebConfigurationManager.AppSettings["Wtf.AdsBaseDirectory"]);
        }

        private void SetCustomDateFormat()
        {
            var culture = new CultureInfo("en-US");

            culture.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
            culture.DateTimeFormat.LongDatePattern = "yyyy-MM-dd";
            culture.DateTimeFormat.ShortTimePattern = "HH:mm";
            culture.DateTimeFormat.LongTimePattern = "HH:mm";

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        private void MvcApplication_AuthenticateRequest(object sender, EventArgs e)
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie == null)
                return;

            var app = (HttpApplication)sender;
            
            var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
            var renewedTicket = FormsAuthentication.RenewTicketIfOld(authTicket);
            if (renewedTicket != authTicket)
            {
                string cookieValue = FormsAuthentication.Encrypt(renewedTicket);
                {
                    if (renewedTicket.IsPersistent)
                    {
                        authCookie.Expires = renewedTicket.Expiration;
                    }
                    authCookie.Value = cookieValue;
                    authCookie.Secure = FormsAuthentication.RequireSSL;
                    authCookie.HttpOnly = true;
                    if (FormsAuthentication.CookieDomain != null)
                    {
                        authCookie.Domain = FormsAuthentication.CookieDomain;
                    }
                    app.Context.Response.Cookies.Remove(authCookie.Name);
                    app.Context.Response.Cookies.Add(authCookie);
                }
            }

            var deserialized = JsonConvert.DeserializeObject<AuthorPrincipalSerializeModel>(authTicket.UserData);

            app.Context.User = new AuthorPrincipal(deserialized);
        }
    }
}