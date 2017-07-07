using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public class ViewArticleViewModel : WtfViewModelBase
    {
        public ViewArticleViewModel(string slug)
        {
            this.Slug = slug;
            this.Article = ArticleModel.GetArticleBySlug(slug);
        }

        public ViewArticleViewModel(ArticleModel article)
        {
            this.Slug = article.Slug;
            this.Article = article;
            string description, image;
            ParseSummaryAndImage(article.SummaryHtml, out description, out image);
            this.OpenGraph = new OpenGraphData
            {
                AuthorName = article.Author.Name,
                Title = article.Title,
                Url = article.Url,
                Description = description,
                Image = image ?? (new Uri(new Uri("https://" + Config.Wtf.Host), this.Article.Author.ImageUrl).AbsoluteUri),
                Type = "article",
                Article = article
            };
        }

        private static void ParseSummaryAndImage(string summaryHtml, out string description, out string image)
        {
            var node = HtmlNode.CreateNode("<div>");
            node.InnerHtml = summaryHtml;
            description = HttpUtility.HtmlDecode(node.InnerText);
            image = node.Descendants("img").FirstOrDefault()?.GetAttributeValue("src", null);
        }

        public string Slug { get; private set; }
        public ArticleModel Article { get; private set; }
        public ViewCommentsViewModel FeaturedComments { get { return new ViewCommentsViewModel(this.Article, this.Article.GetFeaturedComments()); } }
        public IEnumerable<ArticleModel> SimilarArticles { get { return this.RecentArticles; } }
        public string ViewCommentsText
        {
            get
            {
                return string.Format("View All {0} Comments", this.Article.CachedCommentCount);
            }
        }
    }
}