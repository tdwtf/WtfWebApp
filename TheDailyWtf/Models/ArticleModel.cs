using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using TheDailyWtf.Data;
using TheDailyWtf.Discourse;

namespace TheDailyWtf.Models
{
    public sealed class ArticleModel
    {
        public ArticleModel()
        {
            this.Author = new AuthorModel();
            this.Series = new SeriesModel();
        }

        public int Id { get; set; }
        public AuthorModel Author { get; set; }
        public string Status { get; set; }
        public string SummaryHtml { get; set; }
        [AllowHtml]
        public string BodyHtml { get; set; }
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
        public int DiscourseCommentCount { get; set; }
        public int CachedCommentCount { get; set; }
        public int CoalescedCommentCount { get { return Math.Max(this.DiscourseCommentCount, this.CachedCommentCount); } }
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
        public string DiscourseTopicSlug { get; set; }
        public string DiscourseThreadUrl { get { return string.Format("http://what.thedailywtf.com/t/{0}/{1}", this.DiscourseTopicSlug, this.DiscourseTopicId); } }
        public DateTime? PublishedDate { get; set; }
        public string DisplayDate { get { return this.PublishedDate == null ? "(unpublished)" : this.PublishedDate.Value.ToShortDateString(); } }
        public SeriesModel Series { get; set; }
        public string Url { get { return string.Format("http://{0}/articles/{1}", Config.Wtf.Host, this.Slug); } }
        public string CommentsUrl { get { return string.Format("http://{0}/articles/comments/{1}", Config.Wtf.Host, this.Slug); } }
        public string Slug { get; set; }
        public string TwitterUrl { get { return string.Format("//www.twitter.com/home?status=http:{0}+-+{1}+-+The+Daily+WTF", HttpUtility.UrlEncode(this.Url), HttpUtility.UrlEncode(this.Title)); } }
        public string FacebookUrl { get { return string.Format("//www.facebook.com/sharer.php?u=http:{0}&t={1}+-+The+Daily+WTF", HttpUtility.UrlEncode(this.Url), HttpUtility.UrlEncode(this.Title)); } }
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

        public static IEnumerable<ArticleModel> GetAllArticlesByMonth(DateTime month)
        {
            return GetSeriesArticlesByMonth(null, month);
        }

        public static IEnumerable<ArticleModel> GetSeriesArticlesByMonth(string series, DateTime month)
        {
            var monthStart = new DateTime(month.Year, month.Month, 1);
            var articles = StoredProcs.Articles_GetArticles(
                series, 
                Domains.PublishedStatus.Published, 
                monthStart, 
                monthStart.AddMonths(1).AddSeconds(-1.0)
              ).Execute();

            return articles.Select(a => ArticleModel.FromTable(a));
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

        public static IEnumerable<ArticleModel> GetRecentArticlesBySeries(string slug)
        {
            var articles = StoredProcs.Articles_GetRecentArticles(Domains.PublishedStatus.Published, Series_Slug: slug, Article_Count: 8).Execute();
            return articles.Select(a => ArticleModel.FromTable(a));
        }

        public static IEnumerable<ArticleModel> GetRecentArticlesByAuthor(string slug)
        {
            var articles = StoredProcs.Articles_GetRecentArticles(Domains.PublishedStatus.Published, Author_Slug: slug, Article_Count: 8).Execute();
            return articles.Select(a => ArticleModel.FromTable(a));
        }

        public IEnumerable<ArticleModel> GetSimilarArticles()
        {
            yield break;
        }

        public static IEnumerable<ArticleModel> GetUnpublishedArticles()
        {
            var articles = StoredProcs.Articles_GetUnpublishedArticles().Execute();
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

        public static ArticleModel GetRandomArticle()
        {
            var article = StoredProcs.Articles_GetRandomArticle().Execute();
            return ArticleModel.FromTable(article);
        }

        public static ArticleModel FromTable(Tables.Articles_Extended article)
        {
            DateTime lastCommentDate = DateTime.Now;
            
            var model = new ArticleModel()
            {
                Id = article.Article_Id,
                Slug = article.Article_Slug,
                Author = AuthorModel.FromTable(article),
                BodyHtml = article.Body_Html,
                DiscourseCommentCount = (int)article.Cached_Comment_Count,
                CachedCommentCount = (int)article.Cached_Comment_Count,
                DiscourseTopicId = article.Discourse_Topic_Id,
                DiscourseTopicOpened = article.Discourse_Topic_Opened == "Y",
                LastCommentDate = lastCommentDate,
                PublishedDate = article.Published_Date,
                Series = SeriesModel.FromTable(article),
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

            if (article.Discourse_Topic_Id != null)
            {
                var topic = DiscourseHelper.GetDiscussionTopic((int)article.Discourse_Topic_Id);
                model.LastCommentDate = topic.LastPostedAt;
                model.DiscourseCommentCount = topic.PostsCount;
                model.DiscourseTopicSlug = topic.Slug;
            }

            return model;
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

            //Close Blockquotes
            var quotMatches = Regexes.BlockQuoteStart.Matches(summary);
            var quotClosMatches = Regexes.BlockQuoteEnd.Matches(summary);
            for (int i = 0; i < quotMatches.Count - quotClosMatches.Count; i++)
                summary += "</blockquote>";

            return summary;
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