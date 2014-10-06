using System;
using System.Web.Mvc;
using TheDailyWtf.Discourse;
using TheDailyWtf.Models;
using TheDailyWtf.ViewModels;

namespace TheDailyWtf.Controllers
{
    public class ArticlesController : WtfControllerBase
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

        public ActionResult ViewArticleComments(string articleSlug)
        {
            var article = ArticleModel.GetArticleBySlug(articleSlug);
                        
            bool commentsPulled = DiscourseHelper.PullCommentsFromDiscourse(article);
            if (commentsPulled)
                article = ArticleModel.GetArticleBySlug(articleSlug); // reload article with cached comments

            return View(new ViewCommentsViewModel(article));
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

        public ActionResult RandomArticle()
        {
            var article = ArticleModel.GetRandomArticle();
            return RedirectToAction("ViewArticle", new { articleSlug = article.Slug });
        }
    }
}