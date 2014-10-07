using System.Web.Mvc;
using TheDailyWtf.Security;

namespace TheDailyWtf
{
    /// <summary>
    /// Indicates that an action requires Administrator privileges.
    /// </summary>
    public sealed class RequiresAdminAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var principal = filterContext.HttpContext.User as AuthorPrincipal;
            if (principal == null || !principal.IsAdmin)
                filterContext.Result = new HttpUnauthorizedResult("Administrator privileges are required to access this page.");
        }
    }
}