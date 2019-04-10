using System;
using System.Collections.Generic;
using System.Linq;
using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public class ArticlesIndexViewModel : WtfViewModelBase
    {
        private readonly Lazy<IEnumerable<ArticleItemViewModel>> getArticles;

        public ArticlesIndexViewModel()
        {
            this.ReferenceDate = new DateInfo(DateTime.Now);

            this.getArticles = new Lazy<IEnumerable<ArticleItemViewModel>>(() => ArticleModel.GetSeriesArticlesByMonth(this.Series?.Slug, this.ReferenceDate.Reference).Select(a => new ArticleItemViewModel(a)).ToList());
        }

        public ArticlesIndexViewModel(SeriesModel series, DateTime? reference = null)
            : this()
        {
            this.Series = series;
            this.ReferenceDate = new DateInfo(reference ?? ArticleModel.GetRecentArticlesBySeries(series.Slug, 1).FirstOrDefault()?.PublishedDate ?? DateTime.Now);
        }

        public DateInfo ReferenceDate { get; set; }
        public SeriesModel Series { get; set; }
        public string SeriesDescription => this.Series != null ? this.Series.Description : "";
        public string ListHeading 
        { 
            get 
            {
                if (this.Series == null)
                    return "Recent Articles";
                return string.Format("{0} {1}", "Recent", this.Series.Title);
            } 
        }
        public string PreviousMonthUrl => this.FormatUrl(this.ReferenceDate.PrevMonth);
        public string NextMonthUrl => this.FormatUrl(this.ReferenceDate.NextMonth);

        public IEnumerable<ArticleItemViewModel> Articles => this.getArticles.Value;

        private string FormatUrl(DateTime date)
        {
            if (this.Series == null)
                return $"/articles/{date.Year}/{date.Month}";
            else
                return $"/series/{date.Year}/{date.Month}/{this.Series.Slug}";
        }

        public sealed class DateInfo
        {
            public DateInfo(DateTime reference)
            {
                this.Reference = reference;
                this.NextMonth = reference.AddMonths(1);
                this.PrevMonth = reference.AddMonths(-1);
            }

            public DateTime Reference { get; }
            public DateTime NextMonth { get; }
            public DateTime PrevMonth { get; }

            public string CurrentMonthAndYear => this.Reference.ToString("MMM yyyy");
            public string PreviousMonthAndYear => this.PrevMonth.ToString("MMM yy");
            public string NextMonthAndYear => this.NextMonth.ToString("MMM yy");
            public string NextMonthCssClass => this.NextMonth > DateTime.Now ? "disable" : "";
        }
    }
}