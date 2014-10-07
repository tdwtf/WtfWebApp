using System.Security.Principal;
using System.Web.Mvc;
using TheDailyWtf.ViewModels;

namespace TheDailyWtf.Controllers
{
    public class AuthorsController : WtfControllerBase
    {
        //
        // GET: /Authors/

        [OutputCache(CacheProfile = CacheProfile.Timed5Minutes)]
        public ActionResult ViewAuthor(string authorSlug)
        {
            return View(new ViewAuthorViewModel(authorSlug));
        }
    }
}
