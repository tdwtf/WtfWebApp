using System.Collections.Generic;
using System.Linq;
using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public class NavigationMenuViewModel
    {
        public IEnumerable<ArticleModel> RecentFeaturedArticles { get { return ArticleModel.GetRecentArticlesBySeries("feature-articles"); } }
        public IEnumerable<ArticleModel> RecentCodeSodArticles { get { return ArticleModel.GetRecentArticlesBySeries("code-sod"); } }
        public IEnumerable<ArticleModel> RecentErrordArticles { get { return ArticleModel.GetRecentArticlesBySeries("errord"); } }
        public IEnumerable<SeriesModel> OtherSeries
        { 
            get 
            { 
                return SeriesModel.GetAllSeries().Where(s => !new[]{"feature-articles", "code-sod", "errord", "pop-up-potpourri"}.Contains(s.Slug));
            }
        }
        public string ForumsAddress { get { return "https://" + Config.NodeBB.Host; } }
    }
}