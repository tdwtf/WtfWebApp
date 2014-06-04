using System;
using System.Collections.Generic;
using System.Linq;
using TheDailyWtf.Discourse;
using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public class HomeIndexViewModel : WtfViewModelBase
    {
        public IEnumerable<ArticleModel> RecentWtfsSideBar { get { return this.RecentArticles.Take(5); } }

        public IEnumerable<Topic> GetSideBarWtfs()
        {
            return DiscourseHelper.GetSideBarWtfs();
        }

        public IEnumerable<ArticleItemViewModel> Articles 
        { 
            get 
            { 
                return ArticleModel.GetRecentArticles()
                    .Select(a => new ArticleItemViewModel(a) { DisplayAuthorLink = false }); 
            } 
        }
    }
}