using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TheDailyWtf.Data;
using TheDailyWtf.Models;
using TheDailyWtf.Security;

namespace TheDailyWtf.ViewModels
{
    public class EditArticleViewModel : WtfViewModelBase
    {
        public EditArticleViewModel()
        {
            this.Article = new ArticleModel();
        }

        public EditArticleViewModel(int? articleId)
        {
            if (articleId != null)
            {
                this.Article = ArticleModel.GetArticleById((int)articleId);
            }
            else
            {
                this.Article = new ArticleModel();
            }

            if (this.Article.PublishedDate != null)
            {
                this.Date = this.Article.PublishedDate.Value.Date.ToShortDateString();
                this.Time = this.Article.PublishedDate.Value.TimeOfDay.ToString();
            }

            this.UseCustomSlug = !string.Equals(Regex.Replace(this.Article.Title ?? "", @"[^a-z0-9_]+", "-", RegexOptions.IgnoreCase).Trim('-'), this.Article.Slug ?? "", StringComparison.OrdinalIgnoreCase);
        }

        public AuthorPrincipal User { get; set; }
        public bool ShowStatusDropdown { get { return this.User == null || this.User.IsAdmin || this.Article.Status != Domains.PublishedStatus.Published; } }
        public bool UserCanEdit { get { return this.ArticleId == null || this.User != null && (this.User.IsAdmin || this.Article.Author.Slug == this.User.Identity.Name); } }
        public int? ArticleId { get { return Inedo.InedoLib.Util.NullIf(this.Article.Id, 0); } }
        public string Heading { get { return this.ArticleId != null ? string.Format("Edit Article - {0}", this.Article.Title) : "Create New Article"; } }
        public ArticleModel Article { get; private set; }
        public bool UseCustomSlug { get; set; }
        public DateTime? PublishedDate
        {
            get
            {
                var date = Inedo.InedoLib.Util.DateTime.ParseN(this.Date);
                TimeSpan time;
                if (date != null && TimeSpan.TryParse(this.Time, out time))
                    return date.Value.Date.Add(time);

                return this.Article.PublishedDate;
            }
        }
        public string Date { get; set; }
        public string Time { get; set; }

        public bool CreateCommentDiscussionChecked { get; set; }
        public bool CreateCommentDiscussionVisible { get; set; }
        public bool OpenCommentDiscussionChecked { get; set; }

        public IEnumerable<SeriesModel> AllSeries { get { return SeriesModel.GetAllSeries(); } }
        public IEnumerable<AuthorModel> ActiveAuthors { get { return AuthorModel.GetActiveAuthors(); } }
        public IEnumerable<string> ArticleStatuses 
        { 
            get 
            {
                if (this.User != null && this.User.IsAdmin)
                    return Domains.PublishedStatus.ToArray();
                else
                    return new[] { Domains.PublishedStatus.Draft, Domains.PublishedStatus.Pending };
            } 
        }
    }
}
