using System.Web.Mvc;
using System.Web.Security;
using TheDailyWtf.Data;
using TheDailyWtf.Models;
using TheDailyWtf.ViewModels;

namespace TheDailyWtf.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        //
        // GET: /Admin/

        public ActionResult Index()
        {
            return View(new AdminViewModel());
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            bool validLogin = StoredProcs.Authors_ValidateLogin(username, password).Execute().Value;

            if (validLogin)
            {
                FormsAuthentication.RedirectFromLoginPage(username, true);
            }

            return View();
        }

        public ActionResult CreateArticle()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateArticle(ArticleModel article)
        {
            return View();
        }
    }
}
