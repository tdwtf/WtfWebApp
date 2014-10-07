using System.Collections.Generic;
using TheDailyWtf.Data;
using TheDailyWtf.Models;

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
                this.Article = ArticleModel.GetArticleById((int)articleId);
            else
                this.Article = new ArticleModel();
        }

        public int? ArticleId { get { return Inedo.InedoLib.Util.NullIf(this.Article.Id, 0); } }
        public string Heading { get { return this.ArticleId != null ? string.Format("Edit Article - {0}", this.Article.Title) : "Create New Article"; } }
        public ArticleModel Article { get; private set; }
        public bool UseCustomSlug { get; set; }

        public IEnumerable<SeriesModel> AllSeries { get { return SeriesModel.GetAllSeries(); } }
        public IEnumerable<AuthorModel> AllAuthors { get { return AuthorModel.GetAllAuthors(); } }
        public IEnumerable<string> ArticleStatuses { get { return Domains.PublishedStatus.ToArray(); } }
    }
}