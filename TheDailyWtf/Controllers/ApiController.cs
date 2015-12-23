using System;
using TheDailyWtf.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace TheDailyWtf.Controllers
{
    public class ApiController : WtfControllerBase
    {
        public string ViewArticleById(int id, bool onlyBodyAndAdHtml = false)
        {
            var article = ArticleModel.GetArticleById(id);
            if (article == null)
                return ErrorStatus("Invalid Id");            

            return FormatOutput(article, onlyBodyAndAdHtml);
        }
        
        public string ViewArticleBySlug(string articleSlug, bool onlyBodyAndAdHtml = false)
        {
            var article = ArticleModel.GetArticleBySlug(articleSlug);
            if (article == null)
                return ErrorStatus("Invalid Article Slug");

            return FormatOutput(article, onlyBodyAndAdHtml);
        }

        public string ViewRandomArticle()
        {
            var article = ArticleModel.GetRandomArticle();
            if (article == null)
                return ErrorStatus("Service Unavailable");

            return FormatOutput(article, false);
        }

        public string ViewArticlesByDate(int year, int month)
        {
            var date = new DateTime(year, month, 1);
            var articles = ArticleModel.GetAllArticlesByMonth(date);
            if (IsEmpty(articles))
                return ErrorStatus("No articles found for the current date range");

            return FormatOutput(articles);
        }
        
        public string ViewRecentArticlesByCount(int count = 8)
        {
            if(count > 100)
                return ErrorStatus("Count cannot be greater than 100");

            var articles = ArticleModel.GetRecentArticles(count);
            if (IsEmpty(articles))
                return ErrorStatus("Service Unavailable");            

            return FormatOutput(articles);
        }
        
        public string ViewRecentArticlesBySeriesAndCount(string slug, int count = 8)
        {
            if (count > 100)
                return ErrorStatus("Count cannot be greater than 100");

            var articles = ArticleModel.GetRecentArticlesBySeries(slug, count);
            if (IsEmpty(articles))
                return ErrorStatus("Invalid Series");

            return FormatOutput(articles);
        }

        public string ViewArticlesBySeriesAndDate(string slug, int year, int month)
        {
            var date = new DateTime(year, month, 1);
            var articles = ArticleModel.GetSeriesArticlesByMonth(slug, date);
            if (IsEmpty(articles))
                return ErrorStatus("No articles found for the current date range or Invalid Series");

            return FormatOutput(articles);
        }
        
        public string ViewRecentArticlesByAuthorAndCount(string slug, int count = 8)
        {
            if (count > 100)
                return ErrorStatus("Count cannot be greater than 100");

            var articles = ArticleModel.GetRecentArticlesByAuthor(slug, count);
            if (IsEmpty(articles))
                return ErrorStatus("Invalid Author");

            return FormatOutput(articles);
        }

        private bool IsEmpty(IEnumerable<ArticleModel> enumerable)
        {
            return (!enumerable.Any() || enumerable == null);
        }

        private string FormatOutput(ArticleModel article, bool onlyBodyAndAdHtml)
        {
            Response.ContentType = "application/json";
            try
            {
                if (onlyBodyAndAdHtml)
                {
                    JObject data = new JObject();
                    data["BodyHtml"] = article.BodyHtml;
                    data["FooterAdHtml"] = article.FooterAdHtml;
                    data["Status"] = "OK";
                    return JsonConvert.SerializeObject(data);
                }
                else
                {
                    article.BodyAndAdHtml = "";
                    return JsonConvert.SerializeObject(article);
                }
            }
            catch (JsonException je)
            {
                return ErrorStatus("JSON Serialization Error : " + je.Message);
            }
        }

        private string FormatOutput(IEnumerable<ArticleModel> articles)
        {
            Response.ContentType = "application/json";
            List<ArticleModel> updatedArticles = articles.ToList<ArticleModel>();
            foreach (ArticleModel article in updatedArticles)
            {
                article.BodyHtml = "";
                article.BodyAndAdHtml = "";
                article.FooterAdHtml = "";
            }

            try
            {
                return JsonConvert.SerializeObject(updatedArticles);
            }
            catch (JsonException je)
            {
                return ErrorStatus("JSON Serialization Error : " + je.Message);
            }
        }

        private string ErrorStatus(string status)
        {
            Response.ContentType = "application/json";
            return "{\"Status\":\"" + status + "\"}";
        }
    }
}