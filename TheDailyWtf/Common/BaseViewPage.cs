using System.Web.Mvc;
using TheDailyWtf.Security;

namespace TheDailyWtf
{
    public abstract class BaseViewPage : WebViewPage
    {
        public virtual new AuthorPrincipal User
        {
            get { return base.User as AuthorPrincipal; }
        }
    }

    public abstract class BaseViewPage<TModel> : WebViewPage<TModel>
    {
        public virtual new AuthorPrincipal User
        {
            get { return base.User as AuthorPrincipal; }
        }
    }
}