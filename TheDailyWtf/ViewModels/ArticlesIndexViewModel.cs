using System;
using System.Collections.Generic;
using System.Linq;
using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public class ArticlesIndexViewModel : WtfViewModelBase
    {
        private static readonly HashSet<string> paginatedSeries 
            = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "feature-articles", "code-sod", "errord" };

        public ArticlesIndexViewModel()
        {
            this.ReferenceDate = new DateInfo(DateTime.Now);
        }

        public DateInfo ReferenceDate { get; set; }
        public SeriesModel Series { get; set; }
        public bool PaginateArticleListByMonth { get { return this.Series == null || paginatedSeries.Contains(this.Series.Slug); } }
        public string ListHeading 
        { 
            get 
            {
                if (this.Series == null)
                    return "Recent Articles";
                return string.Format("{0} {1}", this.PaginateArticleListByMonth ? "Recent" : "All", this.Series.Title);
            } 
        }
        public string PreviousMonthUrl { get { return this.FormatUrl(this.ReferenceDate.PrevMonth); } }
        public string NextMonthUrl { get { return this.FormatUrl(this.ReferenceDate.NextMonth); } }

        public IEnumerable<ArticleItemViewModel> Articles
        {
            get
            {
                if (this.PaginateArticleListByMonth)
                {
                    return ArticleModel.GetSeriesArticlesByMonth(this.Series.Slug, this.ReferenceDate.Reference)
                        .Select(a => new ArticleItemViewModel(a));
                }
                else
                {
                    return ArticleModel.GetAllArticlesBySeries(this.Series.Slug)
                        .Select(a => new ArticleItemViewModel(a));
                }
            }
        }

        private string FormatUrl(DateTime date)
        {
            if (this.Series == null)
                return string.Format("/articles/{0}/{1}", date.Year, date.Month);
            else
                return string.Format("/series/{0}/{1}/{2}", date.Year, date.Month, this.Series.Slug);
        }

        public sealed class DateInfo
        {
            public DateInfo(DateTime reference)
            {
                this.Reference = reference;
                this.NextMonth = reference.AddMonths(1);
                this.PrevMonth = reference.AddMonths(-1);
            }

            public DateTime Reference { get; private set; }
            public DateTime NextMonth { get; private set; }
            public DateTime PrevMonth { get; private set; }

            public string CurrentMonthAndYear { get { return this.Reference.ToString("MMM yyyy"); } }
            public string PreviousMonthAndYear { get { return this.PrevMonth.ToString("MMM yy"); } }
            public string NextMonthAndYear { get { return this.NextMonth.ToString("MMM yy"); } }
            public string NextMonthCssClass { get { return this.NextMonth > DateTime.Now ? "disable" : ""; } }
        }
    }
}