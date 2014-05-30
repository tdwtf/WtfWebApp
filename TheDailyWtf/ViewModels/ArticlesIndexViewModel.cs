using System;
using System.Collections.Generic;
using System.Linq;
using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public class ArticlesIndexViewModel : WtfViewModelBase
    {
        public ArticlesIndexViewModel()
        {
            this.ReferenceDate = new DateInfo(DateTime.Now);
        }

        public DateInfo ReferenceDate { get; set; }
        public string Series { get; set; }
        public string PreviousMonthUrl { get { return this.FormatUrl(this.ReferenceDate.PrevMonth); } }
        public string NextMonthUrl { get { return this.FormatUrl(this.ReferenceDate.NextMonth); } }

        public IEnumerable<ArticleItemViewModel> Articles
        {
            get
            {
                return ArticleModel.GetSeriesArticlesByMonth(this.Series, this.ReferenceDate.Reference)
                    .Select(a => new ArticleItemViewModel(a));
            }
        }

        private string FormatUrl(DateTime date)
        {
            if (string.IsNullOrEmpty(this.Series))
                return string.Format("/articles/{0}/{1}", date.Year, date.Month);
            else
                return string.Format("/series/{0}/{1}/{2}", date.Year, date.Month, this.Series);
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

            public string CurrentMonthAndYear { get { return this.Reference.ToString("MMMM yyyy"); } }
            public string PreviousMonthAndYear { get { return this.PrevMonth.ToString("MMM yy"); } }
            public string NextMonthAndYear { get { return this.NextMonth.ToString("MMM yy"); } }
            public string NextMonthCssClass { get { return this.NextMonth > DateTime.Now ? "disable" : ""; } }
        }
    }
}