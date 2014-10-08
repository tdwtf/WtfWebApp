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
        public string Summary { get; set; }
        [AllowHtml]
        public string BodyHtml { get; set; }
        public string Title { get; set; }
        public int DiscourseCommentCount { get; set; }
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
        public string DiscourseTopicSlug { get; set; }
        public string DiscourseThreadUrl { get { return string.Format("http://what.thedailywtf.com/t/{0}/{1}", this.DiscourseTopicSlug, this.DiscourseTopicId); } }
        public DateTime? PublishedDate { get; set; }
        public SeriesModel Series { get; set; }
        public string Url { get { return string.Format("//{0}/articles/{1}", Config.Wtf.Host, this.Slug); } }
        public string CommentsUrl { get { return string.Format("//{0}/articles/comments/{1}", Config.Wtf.Host, this.Slug); } }
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
            var articles = StoredProcs.Articles_GetRecentArticles(Domains.PublishedStatus.Published, Article_Count: 8).Execute();
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
            return ArticleModel.FromTable(article);
        }

        public static ArticleModel GetArticleBySlug(string slug)
        {
            var article = StoredProcs.Articles_GetArticleBySlug(slug).Execute();
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
                LastCommentDate = lastCommentDate,
                PublishedDate = article.Published_Date,
                Series = SeriesModel.FromTable(article),
                Status = article.PublishedStatus_Name,
                Summary = ArticleModel.ExtractAndStripFirstParagram(article.Body_Html),
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

        private static string ExtractAndStripFirstParagram(string articleText)
        {
            string extracted = ExtractSummary(articleText, 1, false);
            return StripHtml(extracted);
        }

        private static string ExtractSummary(string articleText, int paragraphCount, bool skipRule)
        {
            const string HR_PATTERN = @"\<hr\s*\/?\s*\>";
            const string P_PATTERN = @"\<(p)[^>]*\>";

            string summary = string.Empty; int idx = 0;

            //Skip past first HR
            if (skipRule)
            {
                Match hrMatch = Regex.Match(articleText, HR_PATTERN, RegexOptions.IgnoreCase);
                if (hrMatch != null) idx += hrMatch.Index + hrMatch.Length;
            }

            //Skip past the first paragraph
            for (int i = 0; i <= paragraphCount; i++)
            {
                Match pMatch = Regex.Match(articleText.Substring(idx), P_PATTERN, RegexOptions.IgnoreCase);
                if (pMatch == null) break;

                idx += pMatch.Index;
                if (i < paragraphCount) idx += pMatch.Length;
            }

            //Get Summary
            summary += (idx == 0) ? articleText : articleText.Substring(0, idx);

            //Close Blockquotes
            MatchCollection quotMatches = Regex.Matches(summary, @"\<blockquote[^>]*\>", RegexOptions.IgnoreCase);
            MatchCollection quotClosMatches = Regex.Matches(summary, @"\<\/blockquote[^>]*\>", RegexOptions.IgnoreCase);
            for (int i = 0; i < quotMatches.Count - quotClosMatches.Count; i++)
                summary += "</blockquote>";

            return summary;
        }

        private static string StripHtml(string text)
        {
            return Regex.Replace(text, @"<(.|\n)*?>", string.Empty);
        }

        private static string FormatComment(string text)
        {
            return BbCodeToHtml(HttpUtility.HtmlEncode(text));
        }

        private static string BbCodeToHtml(string encodedString)
        {
            // Bold, Italic, Underline
            encodedString = BbCodeRegexes.Bold.Replace(encodedString, "<b>$1</b>");
            encodedString = BbCodeRegexes.Italic.Replace(encodedString, "<i>$1</i>");
            encodedString = BbCodeRegexes.Underline.Replace(encodedString, "<u>$1</u>");

            // Quote
            if (BbCodeRegexes.QuoteEndBbCode.Matches(encodedString).Count ==
                BbCodeRegexes.QuoteStartBbCode.Matches(encodedString).Count + BbCodeRegexes.EmptyQuoteStartBbCode.Matches(encodedString).Count)
            {
                encodedString = BbCodeRegexes.QuoteStartBbCode.Replace(encodedString, "<BLOCKQUOTE class=\"Quote\"><div><img src=\"/Resources/images/buttons/comments-quote.png\"> <strong>$1:</strong></div><div>");
                encodedString = BbCodeRegexes.QuoteEndBbCode.Replace(encodedString, "</div></BLOCKQUOTE>");

                encodedString = BbCodeRegexes.EmptyQuoteStartBbCode.Replace(encodedString, "<BLOCKQUOTE class=\"Quote\"><div>");
                encodedString = BbCodeRegexes.EmptyQuoteEndBbCode.Replace(encodedString, "</div></BLOCKQUOTE>");
            };

            // Code
            encodedString = BbCodeRegexes.Code.Replace(encodedString, "<pre>$1</pre>");

            // Anchors
            encodedString = BbCodeRegexes.Url1.Replace(encodedString, "<a rel=\"nofollow\" href=\"http://www.$1\" target=\"_blank\" title=\"$1\">$1</a>");
            encodedString = BbCodeRegexes.Url2.Replace(encodedString, "<a rel=\"nofollow\" href=\"$1\" target=\"_blank\" title=\"$1\">$1</a>");
            encodedString = BbCodeRegexes.Url3.Replace(encodedString, "<a rel=\"nofollow\" href=\"$1\" target=\"_blank\" title=\"$1\">$3</a>");
            encodedString = BbCodeRegexes.Url4.Replace(encodedString, "<a rel=\"nofollow\" href=\"$1\" target=\"_blank\" title=\"$1\">$3</a>");
            encodedString = BbCodeRegexes.Link1.Replace(encodedString, "<a rel=\"nofollow\" href=\"$1\" target=\"_blank\" title=\"$1\">$1</a>");
            encodedString = BbCodeRegexes.Link2.Replace(encodedString, "<a rel=\"nofollow\" href=\"$1\" target=\"_blank\" title=\"$1\">$3</a>");

            // Image
            encodedString = BbCodeRegexes.Img1.Replace(encodedString, "<img src=\"$1\" border=\"0\" />");
            encodedString = BbCodeRegexes.Img2.Replace(encodedString, "<img width=\"$1\" height=\"$3\" src=\"$5\" border=\"0\" />");

            // Color
            encodedString = BbCodeRegexes.Color.Replace(encodedString, "<span style=\"color:$1;\">$3</span>");

            encodedString = encodedString.Replace("\n", "<br />");

            return encodedString;
        }

        private static class BbCodeRegexes
        {
            private static readonly RegexOptions Options = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

            public static readonly Regex Bold = new Regex(@"\[b(?:\s*)\]((.|\n)*?)\[/b(?:\s*)\]", BbCodeRegexes.Options);
            public static readonly Regex Italic = new Regex(@"\[i(?:\s*)\]((.|\n)*?)\[/i(?:\s*)\]", BbCodeRegexes.Options);
            public static readonly Regex Underline = new Regex(@"\[u(?:\s*)\]((.|\n)*?)\[/u(?:\s*)\]", BbCodeRegexes.Options);
            public static readonly Regex Code = new Regex(@"\[code(?:\s*)\]((.|\n)*?)\[/code(?:\s*)\]", BbCodeRegexes.Options);
            public static readonly Regex Url1 = new Regex(@"\[url(?:\s*)\]www\.(.*?)\[/url(?:\s*)\]", BbCodeRegexes.Options);
            public static readonly Regex Url2 = new Regex(@"\[url(?:\s*)\]((.|\n)*?)\[/url(?:\s*)\]", BbCodeRegexes.Options);
            public static readonly Regex Url3 = new Regex(@"\[url=(?:""|&quot;|&#34;)((.|\n)*?)(?:\s*)(?:""|&quot;|&#34;)\]((.|\n)*?)\[/url(?:\s*)\]", BbCodeRegexes.Options);
            public static readonly Regex Url4 = new Regex(@"\[url=((.|\n)*?)(?:\s*)\]((.|\n)*?)\[/url(?:\s*)\]", BbCodeRegexes.Options);
            public static readonly Regex Link1 = new Regex(@"\[link(?:\s*)\]((.|\n)*?)\[/link(?:\s*)\]", BbCodeRegexes.Options);
            public static readonly Regex Link2 = new Regex(@"\[link=((.|\n)*?)(?:\s*)\]((.|\n)*?)\[/link(?:\s*)\]", BbCodeRegexes.Options);
            public static readonly Regex Img1 = new Regex(@"\[img(?:\s*)\]((.|\n)*?)\[/img(?:\s*)\]", BbCodeRegexes.Options);
            public static readonly Regex Img2 = new Regex(@"\[img=((.|\n)*?)x((.|\n)*?)(?:\s*)\]((.|\n)*?)\[/img(?:\s*)\]", BbCodeRegexes.Options);
            public static readonly Regex Color = new Regex(@"\[color=((.|\n)*?)(?:\s*)\]((.|\n)*?)\[/color(?:\s*)\]", BbCodeRegexes.Options);

            public static readonly Regex QuoteStartBbCode = new Regex("\\[quote(?:\\s*)user=(?:\"|&quot;|&#34;)(.*?)(?:\"|&quot;|&#34;)\\]", BbCodeRegexes.Options);
            public static readonly Regex QuoteEndBbCode = new Regex("\\[/quote(\\s*)\\]", BbCodeRegexes.Options);

            public static readonly Regex EmptyQuoteStartBbCode = new Regex("\\[quote(\\s*)\\]", BbCodeRegexes.Options);
            public static readonly Regex EmptyQuoteEndBbCode = new Regex("\\[/quote(\\s*)\\]", BbCodeRegexes.Options);
        }
    }
}