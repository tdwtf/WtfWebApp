using System.Collections.Generic;
using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public abstract class WtfViewModelBase
    {
        public WtfViewModelBase()
        {
            this.PageTitle = "[UNITITLED] The Daily WTF: Curious Perversions in Information Technology";
        }

        public string PageTitle { get; set; }
        public IEnumerable<ArticleModel> RecentArticles { get { return ArticleModel.GetRecentArticles(); } }
        public NavigationMenuViewModel NavigationMenu { get { return new NavigationMenuViewModel(); } }
    }
}