using System.Web.Mvc;
using TheDailyWtf.Security;

namespace TheDailyWtf
{
    public abstract class WtfControllerBase : Controller
    {
        protected virtual new AuthorPrincipal User
        {
            get { return base.User as AuthorPrincipal; }
        }
    }
}