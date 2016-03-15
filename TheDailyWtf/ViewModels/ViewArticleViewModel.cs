using System.Collections.Generic;
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