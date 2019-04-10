using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheDailyWtf.Common.HtmlCleaner;
using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public class ViewArticleViewModel : WtfViewModelBase
    {
        private readonly Lazy<ViewCommentsViewModel> getFeaturedComments;

        public ViewArticleViewModel(string slug)
        {
            this.Slug = slug;
            this.Article = ArticleModel.GetArticleBySlug(slug);

            this.getFeaturedComments = new Lazy<ViewCommentsViewModel>(() => new ViewCommentsViewModel(this.Article, this.Article.GetFeaturedComments()));
        }

        public ViewArticleViewModel(ArticleModel article)
        {
            this.Slug = article.Slug;
            this.Article = article;
            ParseSummaryAndImage(article.SummaryHtml, out var description, out var image);
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

            this.getFeaturedComments = new Lazy<ViewCommentsViewModel>(() => new ViewCommentsViewModel(this.Article, this.Article.GetFeaturedComments()));
        }

        private static void ParseSummaryAndImage(string summaryHtml, out string description, out string image)
        {
            var node = Cleaner.Parse(summaryHtml);
            description = HttpUtility.HtmlDecode(node.GetInnerText());
            image = node.Descendants("img").FirstOrDefault()?.GetAttributeValue("src", null);
        }

        public string Slug { get; }
        public ArticleModel Article { get; }
        public ViewCommentsViewModel FeaturedComments => this.getFeaturedComments.Value;
        public IEnumerable<ArticleModel> SimilarArticles => this.RecentArticles;
        public string ViewCommentsText => $"View All {this.Article.CachedCommentCount} Comments";
    }
}