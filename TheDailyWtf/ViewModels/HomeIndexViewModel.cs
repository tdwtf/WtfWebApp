using System;
using System.Collections.Generic;
using System.Linq;
using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public class HomeIndexViewModel : WtfViewModelBase
    {
        public IEnumerable<ArticleModel> RecentWtfsSideBar { get { return this.RecentArticles.Take(5); } }

        public IEnumerable<ArticleItemViewModel> Articles 
        { 
            get 
            { 
                return ArticleModel.GetRecentArticles()
                    .Select(a => new ArticleItemViewModel(a)); 
            } 
        }

        public IEnumerable<DimensionRoot> AllAdDimensions
        {
            get { return AdRotator.DimensionRoots; }
        }

        public string ArchivesUrl
        {
            get
            {
                var date = DateTime.Today;
                return string.Format("/articles/{0}/{1}", date.Year, date.Month);
            }
        }
    }
}