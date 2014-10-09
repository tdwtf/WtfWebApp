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
        public IEnumerable<CommentModel> FeaturedComments { get { return this.Article.GetFeaturedComments(); } }
        public IEnumerable<ArticleModel> SimilarArticles { get { return this.RecentArticles; } }
        public string ViewCommentsText
        {
            get
            {
                if (this.Article.DiscourseTopicId == null)
                    return string.Format("View All {0} Comments", this.Article.CachedCommentCount);
                else
                    return string.Format("Preview Top {0} Comments", this.Article.CachedCommentCount);
            }
        }
    }
}