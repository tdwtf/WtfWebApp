using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public class ViewAuthorViewModel : WtfViewModelBase
    {
        public ViewAuthorViewModel(string slug, DateTime? month = null)
        {
            this.Author = AuthorModel.GetAuthorBySlug(slug);
            if (this.Author == null)
            {
                throw new HttpException(404, "author not found");
            }
            this.ReferenceDate = month.HasValue ? new ArticlesIndexViewModel.DateInfo(month.Value) : null;

            this.OpenGraph = new OpenGraphData
            {
                Title = this.Author.Name,
                Type = "profile",
                Image = new Uri(new Uri("http://" + Config.Wtf.Host), this.Author.ImageUrl).AbsoluteUri,
                Description = this.Author.ShortDescription,
                Url = "http://" + Config.Wtf.Host + "/authors/" + this.Author.Slug,
                Author = this.Author
            };
        }

        public AuthorModel Author { get; set; }
        public ArticlesIndexViewModel.DateInfo ReferenceDate { get; set; }
        public IEnumerable<ArticleItemViewModel> Articles
        {
            get
            {
                if (this.ReferenceDate != null)
                {
                    return ArticleModel.GetAuthorArticlesByMonth(this.Author.Slug, this.ReferenceDate.Reference)
                        .Select(a => new ArticleItemViewModel(a));
                }
                return ArticleModel.GetRecentArticlesByAuthor(this.Author.Slug)
                    .Select(a => new ArticleItemViewModel(a));
            }
        }

        private string FormatUrl(DateTime date)
        {
            return string.Format("/authors/{0}/{1}/{2}", date.Year, date.Month, this.Author.Slug);
        }

        public string ArchivesUrl { get { return this.FormatUrl(this.Articles.Last().Article.PublishedDate.Value); } }
        public string PreviousMonthUrl { get { return this.FormatUrl(this.ReferenceDate.PrevMonth); } }
        public string NextMonthUrl { get { return this.FormatUrl(this.ReferenceDate.NextMonth); } }
    }
}