using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using TheDailyWtf.Data;
using TheDailyWtf.Forum;

namespace TheDailyWtf.Models
{
    public sealed class ArticleModel
    {
        public ArticleModel()
        {
            this.Author = new AuthorModel();
            this.Series = new SeriesModel();
        }
        
        public int? Id { get; set; }
        [Required]
        public AuthorModel Author { get; set; }
        public string Status { get; set; }
        public string SummaryHtml { get; set; }
        [AllowHtml]
        public string BodyHtml { get; set; }
        [AllowHtml]
        public string BodyAndAdHtml { get; set; }
        [Required]
        public string Title { get; set; }
        public string RssTitle
        {
            get
            {
                if (this.Series.Title.Equals("Feature Articles", StringComparison.OrdinalIgnoreCase))
                    return this.Title;
                else
                    return string.Format("{0}: {1}", this.Series.Title, this.Title);
            }
        }
        public int CachedCommentCount { get; set; }
        public DateTime? LastCommentDate { get; set; }
        public string LastCommentDateDescription 
        { 
            get 
            {
                if (LastCommentDate == null)
                    return "-none-";
                if (LastCommentDate.Value.Date == DateTime.Now.Date)
                    return this.LastCommentDate.Value.ToShortTimeString();
                return this.LastCommentDate.Value.ToShortDateString();
            } 
        }
        public int? DiscourseTopicId { get; set; }
        public bool DiscourseTopicOpened { get; set; }
        public DateTime? PublishedDate { get; set; }
        public string DisplayDate { get { return this.PublishedDate == null ? "(unpublished)" : string.Format("{0:MMMM d}{1} {0:yyyy}", this.PublishedDate.Value,
            (this.PublishedDate.Value.Day % 10 == 1 && this.PublishedDate.Value.Day != 11) ? "st" :
            (this.PublishedDate.Value.Day % 10 == 2 && this.PublishedDate.Value.Day != 12) ? "nd" :
            (this.PublishedDate.Value.Day % 10 == 3 && this.PublishedDate.Value.Day != 13) ? "rd" :
            "th"); } }
        [Required]
        public SeriesModel Series { get; set; }
        [AllowHtml]
        public string FooterAdHtml { get; set; }
        public string Url { get { return string.Format("http://{0}/articles/{1}", Config.Wtf.Host, this.Slug); } }
        public string CommentsUrl { get { return string.Format("http://{0}/articles/comments/{1}", Config.Wtf.Host, this.Slug); } }
        public string Slug { get; set; }
        public string TwitterUrl { get { return string.Format("//www.twitter.com/home?status={0}+-+{1}+-+The+Daily+WTF", HttpUtility.UrlEncode(this.Url), HttpUtility.UrlEncode(this.Title)); } }
        public string FacebookUrl { get { return string.Format("//www.facebook.com/sharer.php?u={0}&t={1}+-+The+Daily+WTF", HttpUtility.UrlEncode(this.Url), HttpUtility.UrlEncode(this.Title)); } }
        public string EmailUrl 
        { 
            get 
            {
                return string.Format(
                    "mailto:%20?subject={0}&body={1}",
                    HttpUtility.UrlPathEncode("Check out this article on The Daily WTF..."),
                    HttpUtility.UrlPathEncode(string.Format("{0}: {1}", this.Title, this.Url))
                );
            } 
        }
        public string GooglePlusUrl { get { return string.Format("//plus.google.com/share?url={0}", HttpUtility.UrlEncode(this.Url)); } }

        public int? PreviousArticleId { get; set; }
        public string PreviousArticleTitle { get; set; }
        public string PreviousArticleSlug { get; set; }
        public string PreviousArticleUrl { get { return string.Format("//{0}/articles/{1}", Config.Wtf.Host, this.PreviousArticleSlug); } }

        public int? NextArticleId { get; set; }
        public string NextArticleTitle { get; set; }
        public string NextArticleSlug { get; set; }
        public string NextArticleUrl { get { return string.Format("//{0}/articles/{1}", Config.Wtf.Host, this.NextArticleSlug); } }

        public static IEnumerable<ArticleModel> GetAllArticlesBySeries(string series)
        {
            var articles = StoredProcs.Articles_GetArticles(series, Domains.PublishedStatus.Published, null, null).Execute();
            return articles.Select(a => ArticleModel.FromTable(a));
        }

        private static IEnumerable<ArticleModel> GetArticlesByMonth(DateTime month, string series = null, string author = null)
        {
            var monthStart = new DateTime(month.Year, month.Month, 1);
            var articles = StoredProcs.Articles_GetArticles(
                Series_Slug: series,
                PublishedStatus_Name: Domains.PublishedStatus.Published,
                RangeStart_Date: monthStart,
                RangeEnd_Date: monthStart.AddMonths(1).AddSeconds(-1.0),
                Author_Slug: author
              ).Execute();

            return articles.Select(a => ArticleModel.FromTable(a));
        }

        public static IEnumerable<ArticleModel> GetAllArticlesByMonth(DateTime month)
        {
            return GetArticlesByMonth(month);
        }

        public static IEnumerable<ArticleModel> GetSeriesArticlesByMonth(string series, DateTime month)
        {
            return GetArticlesByMonth(month, series: series);
        }

        public static IEnumerable<ArticleModel> GetAuthorArticlesByMonth(string author, DateTime month)
        {
            return GetArticlesByMonth(month, author: author);
        }

        public static IEnumerable<ArticleModel> GetRecentArticles()
        {
            return GetRecentArticles(8);
        }

        public static IEnumerable<ArticleModel> GetRecentArticles(int count)
        {
            var articles = StoredProcs.Articles_GetRecentArticles(Domains.PublishedStatus.Published, Article_Count: count).Execute();
            return articles.Select(a => ArticleModel.FromTable(a));
        }

        public static IEnumerable<ArticleModel> GetRecentArticlesBySeries(string slug, int? articleCount = 8)
        {
            var articles = StoredProcs.Articles_GetRecentArticles(Domains.PublishedStatus.Published, Series_Slug: slug, Article_Count: articleCount).Execute();
            return articles.Select(a => ArticleModel.FromTable(a));
        }

        public static IEnumerable<ArticleModel> GetRecentArticlesByAuthor(string slug, int? articleCount = 8)
        {
            var articles = StoredProcs.Articles_GetRecentArticles(Domains.PublishedStatus.Published, Author_Slug: slug, Article_Count: articleCount).Execute();
            return articles.Select(a => ArticleModel.FromTable(a));
        }

        public IEnumerable<ArticleModel> GetSimilarArticles()
        {
            yield break;
        }

        public static IEnumerable<ArticleModel> GetUnpublishedArticles(string authorSlug = null)
        {
            var articles = StoredProcs.Articles_GetUnpublishedArticles(authorSlug).Execute();
            return articles.Select(a => ArticleModel.FromTable(a));
        }

        public IEnumerable<CommentModel> GetFeaturedComments()
        {
            return CommentModel.GetFeaturedCommentsForArticle(this);
        }

        public static ArticleModel GetArticleById(int id)
        {
            var article = StoredProcs.Articles_GetArticleById(id).Execute();
            if (article == null)
                return null;
            return ArticleModel.FromTable(article);
        }

        public static ArticleModel GetArticleBySlug(string slug)
        {
            var article = StoredProcs.Articles_GetArticleBySlug(slug).Execute();
            if (article == null)
                return null;
            return ArticleModel.FromTable(article);
        }

        public static ArticleModel GetArticleByLegacyPost(int postId)
        {
            var article = StoredProcs.Articles_GetArticleByLegacyPost(postId).Execute();
            if (article == null)
                return null;
            return ArticleModel.FromTable(article);
        }

        public static ArticleModel GetRandomArticle()
        {
            var article = StoredProcs.Articles_GetRandomArticle().Execute();
            return ArticleModel.FromTable(article);
        }

        public static ArticleModel FromTable(Tables.Articles_Extended article)
        {
            // add microdata to take advantage of Google's rich snippets for articles:
            // https://developers.google.com/structured-data/rich-snippets/articles
            if (article.Body_Html.Contains("<img ") && !article.Body_Html.Contains(" itemprop=\"image\" "))
            {
                // only modify the first image
                int index = article.Body_Html.IndexOf("<img ") + "<img ".Length;
                article.Body_Html = article.Body_Html.Substring(0, index) + "itemprop=\"image\" " + article.Body_Html.Substring(index);
                // assume the body+ad contains the entire body
                index = article.BodyAndAd_Html.IndexOf("<img ") + "<img ".Length;
                article.BodyAndAd_Html = article.BodyAndAd_Html.Substring(0, index) + "itemprop=\"image\" " + article.BodyAndAd_Html.Substring(index);
            }

            return new ArticleModel()
            {
                Id = article.Article_Id,
                Slug = article.Article_Slug,
                Author = AuthorModel.FromTable(article),
                BodyHtml = article.Body_Html,
                BodyAndAdHtml = article.BodyAndAd_Html,
                CachedCommentCount = (int)article.Cached_Comment_Count,
                DiscourseTopicId = article.Discourse_Topic_Id,
                DiscourseTopicOpened = article.Discourse_Topic_Opened == "Y",
                LastCommentDate = article.Last_Comment_Date,
                PublishedDate = article.Published_Date,
                Series = SeriesModel.FromTable(article),
                FooterAdHtml = article.Ad_Html,
                Status = article.PublishedStatus_Name,
                SummaryHtml = ArticleModel.ExtractSummary(article.Body_Html),
                Title = article.Title_Text,
                PreviousArticleId = article.Previous_Article_Id,
                PreviousArticleSlug = article.Previous_Article_Slug,
                PreviousArticleTitle = article.Previous_Title_Text,
                NextArticleId = article.Next_Article_Id,
                NextArticleSlug = article.Next_Article_Slug,
                NextArticleTitle = article.Next_Title_Text
            };
        }

        private static string ExtractSummary(string articleText)
        {
            return ExtractSummary(articleText, 2, false);
        }

        private static string ExtractSummary(string articleText, int paragraphCount, bool skipRule)
        {
            string summary = string.Empty; 
            int index = 0;

            //Skip past first HR
            if (skipRule)
            {
                var hrMatch = Regexes.Hr.Match(articleText);
                if (hrMatch != null) 
                    index += hrMatch.Index + hrMatch.Length;
            }

            //Skip past the first paragraph
            for (int i = 0; i <= paragraphCount; i++)
            {
                var pMatch = Regexes.P.Match(articleText.Substring(index));
                if (pMatch == null) 
                    break;

                index += pMatch.Index;
                if (i < paragraphCount) 
                    index += pMatch.Length;
            }

            summary += (index == 0) ? articleText : articleText.Substring(0, index);

            return HtmlCleaner.CloseTags(summary);
        }

        private static class Regexes
        {
            private static readonly RegexOptions Options = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

            public static readonly Regex Hr = new Regex(@"\<hr\s*\/?\s*\>", Regexes.Options);
            public static readonly Regex P = new Regex(@"\<(p)[^>]*\>", Regexes.Options);

            public static readonly Regex BlockQuoteStart = new Regex(@"\<blockquote[^>]*\>", Regexes.Options);
            public static readonly Regex BlockQuoteEnd = new Regex(@"\<\/blockquote[^>]*\>", Regexes.Options);
        }
    }
}