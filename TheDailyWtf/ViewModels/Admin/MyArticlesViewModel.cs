using System.Collections.Generic;
using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public sealed class MyArticlesViewModel : WtfViewModelBase
    {
        public IEnumerable<ArticleModel> UnpublishedArticles { get; set; }
        public IEnumerable<ArticleModel> RecentPublishedArticles { get; set; }
        public int MaxPublishedArticleCount { get { return 50; } }

        public MyArticlesViewModel(string authorSlug)
        {
            this.UnpublishedArticles = ArticleModel.GetUnpublishedArticles(authorSlug);
            this.RecentPublishedArticles = ArticleModel.GetRecentArticlesByAuthor(authorSlug, this.MaxPublishedArticleCount);
        }
    }
}