using System;
using System.Collections.Generic;
using System.Linq;
using Inedo;
using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public class NavigationMenuViewModel
    {
        private static readonly LazyCached<IEnumerable<ArticleModel>> recentFeatured = new LazyCached<IEnumerable<ArticleModel>>(() => ArticleModel.GetRecentArticlesBySeries("feature-articles"), TimeSpan.FromHours(1));
        private static readonly LazyCached<IEnumerable<ArticleModel>> recentCodeSod = new LazyCached<IEnumerable<ArticleModel>>(() => ArticleModel.GetRecentArticlesBySeries("code-sod"), TimeSpan.FromHours(1));
        private static readonly LazyCached<IEnumerable<ArticleModel>> recentErrord = new LazyCached<IEnumerable<ArticleModel>>(() => ArticleModel.GetRecentArticlesBySeries("errord"), TimeSpan.FromHours(1));

        public IEnumerable<ArticleModel> RecentFeaturedArticles => recentFeatured.Value;
        public IEnumerable<ArticleModel> RecentCodeSodArticles => recentCodeSod.Value;
        public IEnumerable<ArticleModel> RecentErrordArticles => recentErrord.Value;
        public IEnumerable<SeriesModel> OtherSeries
        { 
            get 
            {
                return SeriesModel.GetAllSeries().Where(s => !new[] { "feature-articles", "code-sod", "errord", "pop-up-potpourri" }.Contains(s.Slug));
            }
        }
        public string ForumsAddress => "https://" + Config.NodeBB.Host;
    }
}