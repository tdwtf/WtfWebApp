using System;
using TheDailyWtf.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using TheDailyWtf.Data;

namespace TheDailyWtf.Controllers
{
    public class ApiController : WtfControllerBase
    {
        [OutputCache(CacheProfile = CacheProfile.Timed5Minutes)]
        public void ViewApiDocumentation()
        {
            Response.Redirect("https://github.com/tdwtf/WtfWebApp/blob/master/Docs/API.md");
        }

        [OutputCache(CacheProfile = CacheProfile.Timed5Minutes)]
        public ActionResult ViewArticleById(int id, bool onlyBodyAndAdHtml = false)
        {
            var article = ArticleModel.GetArticleById(id);
            if (article == null)
            {
                return ErrorStatus(HttpStatusCode.NotFound, "Invalid Id");
            }

            return FormatOutput(article, onlyBodyAndAdHtml);
        }

        [OutputCache(CacheProfile = CacheProfile.Timed5Minutes)]
        public ActionResult ViewArticleBySlug(string articleSlug, bool onlyBodyAndAdHtml = false)
        {
            var article = ArticleModel.GetArticleBySlug(articleSlug);
            if (article == null)
            {
                return ErrorStatus(HttpStatusCode.NotFound, "Invalid Article Slug");
            }

            return FormatOutput(article, onlyBodyAndAdHtml);
        }

        public ActionResult ViewRandomArticle()
        {
            var articleSlim = this.GetRandomArticleInternal();

            if (articleSlim == null)
            {
                return ErrorStatus(HttpStatusCode.ServiceUnavailable, "Service Unavailable");
            }

            var article = ArticleModel.FromTable(DB.Articles_GetArticleById(articleSlim.Article_Id));

            return FormatOutput(article, false);
        }

        [OutputCache(CacheProfile = CacheProfile.Timed5Minutes)]
        public ActionResult ViewArticlesByDate(int year, int month)
        {
            DateTime date;
            try
            {
                date = new DateTime(year, month, 1);
            }
            catch (ArgumentOutOfRangeException)
            {
                return ErrorStatus(HttpStatusCode.BadRequest, "Invalid date");
            }
            var articles = ArticleModel.GetAllArticlesByMonth(date);
            if (IsEmpty(articles))
            {
                return ErrorStatus(HttpStatusCode.NotFound, "No articles found for the current date range");
            }

            return FormatOutput(articles);
        }

        [OutputCache(CacheProfile = CacheProfile.Timed5Minutes)]
        public ActionResult ViewRecentArticlesByCount(int count = 8)
        {
            if (count > 100)
            {
                return ErrorStatus(HttpStatusCode.BadRequest, "Count cannot be greater than 100");
            }

            var articles = ArticleModel.GetRecentArticles(count);
            if (IsEmpty(articles))
            {
                return ErrorStatus(HttpStatusCode.ServiceUnavailable, "Service Unavailable");
            }

            return FormatOutput(articles);
        }

        [OutputCache(CacheProfile = CacheProfile.Timed5Minutes)]
        public ActionResult ViewRecentArticlesBySeriesAndCount(string slug, int count = 8)
        {
            if (count > 100)
            {
                return ErrorStatus(HttpStatusCode.BadRequest, "Count cannot be greater than 100");
            }

            var articles = ArticleModel.GetRecentArticlesBySeries(slug, count);
            if (IsEmpty(articles))
            {
                return ErrorStatus(HttpStatusCode.NotFound, SeriesModel.GetSeriesBySlug(slug) == null ? "Invalid Series" : "No articles found");
            }

            return FormatOutput(articles);
        }

        [OutputCache(CacheProfile = CacheProfile.Timed5Minutes)]
        public ActionResult ViewArticlesBySeriesAndDate(string slug, int year, int month)
        {
            DateTime date;
            try
            {
                date = new DateTime(year, month, 1);
            }
            catch (ArgumentOutOfRangeException)
            {
                return ErrorStatus(HttpStatusCode.BadRequest, "Invalid date");
            }
            var articles = ArticleModel.GetSeriesArticlesByMonth(slug, date);
            if (IsEmpty(articles))
            {
                return ErrorStatus(HttpStatusCode.NotFound, SeriesModel.GetSeriesBySlug(slug) == null ? "Invalid Series" : "No articles found");
            }

            return FormatOutput(articles);
        }

        [OutputCache(CacheProfile = CacheProfile.Timed5Minutes)]
        public ActionResult ViewSeries()
        {
            var series = SeriesModel.GetAllSeries();
            if (IsEmpty(series))
            {
                return ErrorStatus(HttpStatusCode.InternalServerError, "Error getting series listing");
            }
            return FormatOutput(series);
        }

        [OutputCache(CacheProfile = CacheProfile.Timed5Minutes)]
        public ActionResult ViewRecentArticlesByAuthorAndCount(string slug, int count = 8)
        {
            if (count > 100)
            {
                return ErrorStatus(HttpStatusCode.BadRequest, "Count cannot be greater than 100");
            }

            var articles = ArticleModel.GetRecentArticlesByAuthor(slug, count);
            if (IsEmpty(articles))
            {
                return ErrorStatus(HttpStatusCode.NotFound, "Invalid Author");
            }

            return FormatOutput(articles);
        }

        private bool IsEmpty<T>(IEnumerable<T> enumerable)
        {
            return enumerable == null || !enumerable.Any();
        }

        private ActionResult FormatOutput(ArticleModel article, bool onlyBodyAndAdHtml)
        {
            if (onlyBodyAndAdHtml)
            {
                return Json(new { BodyHtml = article.BodyHtml, FooterAdHtml = article.FooterAdHtml, Status = article.Status }, JsonRequestBehavior.AllowGet);
            }

            article.BodyAndAdHtml = "";
            return Json(article, JsonRequestBehavior.AllowGet);
        }

        private ActionResult FormatOutput(IEnumerable<ArticleModel> articles)
        {
            foreach (var article in articles)
            {
                article.BodyHtml = "";
                article.BodyAndAdHtml = "";
                article.FooterAdHtml = "";
            }

            return Json(articles, JsonRequestBehavior.AllowGet);
        }

        private ActionResult FormatOutput(IEnumerable<SeriesModel> series)
        {
            return Json(series, JsonRequestBehavior.AllowGet);
        }

        private ActionResult ErrorStatus(HttpStatusCode code, string status)
        {
            var result = new HttpStatusCodeResult(code);
            Response.StatusCode = result.StatusCode;
            Response.StatusDescription = result.StatusDescription;
            return Json(new { Status = status }, JsonRequestBehavior.AllowGet);
        }
    }
}
