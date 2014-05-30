using System;
using System.Web.Mvc;
using TheDailyWtf.ViewModels;

namespace TheDailyWtf.Controllers
{
    public class ArticlesController : Controller
    {
        //
        // GET: /Articles/

        public ActionResult Index()
        {
            return View(new ArticlesIndexViewModel());
        }

        public ActionResult ViewArticle(string articleSlug)
        {
            return View(new ViewArticleViewModel(articleSlug));
        }

        public ActionResult ViewLegacyArticle(string articleSlug)
        {
            return RedirectToActionPermanent("ViewArticle", new { articleSlug });
        }

        public ActionResult ViewArticlesByMonth(int year, int month)
        {
            var date = new DateTime(year, month, 1);
            return View(Views.Articles.Index, new ArticlesIndexViewModel() { ReferenceDate = new ArticlesIndexViewModel.DateInfo(date) });
        }

        public ActionResult ViewArticlesBySeries(string series)
        {
            return View(Views.Articles.Index, new ArticlesIndexViewModel() { Series = series });
        }

        public ActionResult ViewArticlesBySeriesAndMonth(int year, int month, string series)
        {
            var date = new DateTime(year, month, 1);
            return View(
                Views.Articles.Index, 
                new ArticlesIndexViewModel() 
                { 
                    Series = series, 
                    ReferenceDate = new ArticlesIndexViewModel.DateInfo(date) 
                }
            );
        }
    }
}
