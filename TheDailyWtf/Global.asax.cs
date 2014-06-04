using System;
using System.Web;
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
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
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