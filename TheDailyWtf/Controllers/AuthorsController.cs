using System.Web.Mvc;
using TheDailyWtf.ViewModels;

namespace TheDailyWtf.Controllers
{
    public class AuthorsController : Controller
    {
        //
        // GET: /Authors/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ViewAuthor(string authorSlug)
        {
            return View(new ViewAuthorViewModel(authorSlug));
        }    
    }
}
