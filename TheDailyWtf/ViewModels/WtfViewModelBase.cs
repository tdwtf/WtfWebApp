using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Configuration;
using TheDailyWtf.Forum;
using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public abstract class WtfViewModelBase
    {
        private static readonly string copyright = typeof(WtfViewModelBase).Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)
                .Cast<AssemblyCopyrightAttribute>()
                .First()
                .Copyright;
        private static readonly string version = typeof(WtfViewModelBase).Assembly.GetName().Version.ToString(2);

        public WtfViewModelBase()
        {
            this.PageTitle = "The Daily WTF: Curious Perversions in Information Technology";
            this.ShowLeaderboardAd = true;
        }

        public bool ShowLeaderboardAd { get; set; }
        public string PageTitle { get; set; }
        public IEnumerable<ArticleModel> RecentArticles { get { return ArticleModel.GetRecentArticles(); } }
        public NavigationMenuViewModel NavigationMenu { get { return new NavigationMenuViewModel(); } }
        public string Copyright { get { return copyright; } }
        public string Version { get { return version; } }
        public OpenGraphData OpenGraph { get; set; }

        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }
        public string ForumMessage
        {
            get
            {
                var ex = ForumHelper.LastException;
                if (ex == null)
                    return null;

                return "There was an issue connecting to the forum API: " + ex;
            }
        }

        public Ad GetNextAd(Dimensions dimensions)
        {
            return AdRotator.GetNextAd(dimensions);
        }

        public class OpenGraphData
        {
            public string AuthorName { get; set; }
            public string Title { get; set; }
            public string Type { get; set; }
            public string Url { get; set; }
            public string Image { get; set; }
            public string Description { get; set; }
            public ArticleModel Article { get; set; }
            public AuthorModel Author { get; set; }
        }
    }
}