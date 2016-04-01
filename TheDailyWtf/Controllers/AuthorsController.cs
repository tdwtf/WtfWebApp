using System;
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

        [OutputCache(CacheProfile = CacheProfile.Timed5Minutes)]
        public ActionResult ViewAuthorByMonth(int year, int month, string authorSlug)
        {
            var date = new DateTime(year, month, 1);
            return View(new ViewAuthorViewModel(authorSlug, date));
        }
    }
}
