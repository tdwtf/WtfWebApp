using System;
using System.Web.Mvc;
using TheDailyWtf.Discourse;
using TheDailyWtf.Legacy;
using TheDailyWtf.Models;
using TheDailyWtf.ViewModels;

namespace TheDailyWtf.Controllers
{
    public class ArticlesController : WtfControllerBase
    {
        //
        // GET: /Articles/

        [OutputCache(CacheProfile = CacheProfile.Timed1Minute)]
        public ActionResult Index()
        {
            return View(new ArticlesIndexViewModel());
        }

        public ActionResult ViewArticleById(int id)
        {
            var article = ArticleModel.GetArticleById(id);
            if (article == null)
                return HttpNotFound();

            return Redirect(article.Url);
        }

        [OutputCache(CacheProfile = CacheProfile.Timed1Minute)]
        public ActionResult ViewArticle(string articleSlug)
        {
            var article = ArticleModel.GetArticleBySlug(articleSlug);
            if (article == null)
                return HttpNotFound();

            return View(new ViewArticleViewModel(article));
        }

        [OutputCache(CacheProfile = CacheProfile.Timed1Minute)]
        public ActionResult ViewArticleComments(string articleSlug)
        {
            var article = ArticleModel.GetArticleBySlug(articleSlug);
            if (article == null)
                return HttpNotFound();

            if (!article.DiscourseTopicOpened && article.DiscourseTopicId != null && article.PublishedDate < DateTime.Now)
                DiscourseHelper.OpenCommentDiscussion((int)article.Id, (int)article.DiscourseTopicId);
                        
            bool commentsPulled = DiscourseHelper.PullCommentsFromDiscourse(article);
            if (commentsPulled)
                article = ArticleModel.GetArticleBySlug(articleSlug); // reload article with cached comments

            return View(new ViewCommentsViewModel(article));
        }

        public ActionResult ViewLegacyArticle(string articleSlug)
        {
            return RedirectToActionPermanent("ViewArticle", new { articleSlug });
        }

        public ActionResult ViewLegacyPost(int? postId)
        {
            if (postId == null)
                return HttpNotFound();

            var article = ArticleModel.GetArticleByLegacyPost((int)postId);
            if (article == null)
                return HttpNotFound();

            return RedirectToActionPermanent("ViewArticle", new { articleSlug = article.Slug });
        }

        public ActionResult ViewLegacyArticleComments(string articleSlug)
        {
            return RedirectToActionPermanent("ViewArticleComments", new { articleSlug });
        }

        [OutputCache(CacheProfile = CacheProfile.Timed1Minute)]
        public ActionResult ViewArticlesByMonth(int year, int month)
        {
            var date = new DateTime(year, month, 1);
            return View(Views.Articles.Index, new ArticlesIndexViewModel() { ReferenceDate = new ArticlesIndexViewModel.DateInfo(date) });
        }

        public ActionResult ViewLegacySeries(string legacySeries)
        {
            var legacyPart = LegacyEncodedUrlPart.CreateFromEncodedUrl(legacySeries);

            SeriesModel series;
            if (SeriesModel.LegacySeriesMap.TryGetValue(legacyPart.DecodedValue, out series))
                return RedirectToActionPermanent("ViewArticlesBySeries", new { seriesSlug = series.Slug });

            return HttpNotFound();
        }

        [OutputCache(CacheProfile = CacheProfile.Timed5Minutes)]
        public ActionResult ViewArticlesBySeries(string seriesSlug)
        {
            var series = SeriesModel.GetSeriesBySlug(seriesSlug);
            if (series == null)
                return HttpNotFound();

            return View(Views.Articles.Index, new ArticlesIndexViewModel() { Series = series });
        }

        [OutputCache(CacheProfile = CacheProfile.Timed5Minutes)]
        public ActionResult ViewArticlesBySeriesAndMonth(int year, int month, string seriesSlug)
        {
            var date = new DateTime(year, month, 1);
            var series = SeriesModel.GetSeriesBySlug(seriesSlug);
            if (series == null)
                return HttpNotFound();

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