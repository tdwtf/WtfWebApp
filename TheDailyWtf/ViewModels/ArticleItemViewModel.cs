using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public class ArticleItemViewModel : WtfViewModelBase
    {
        public ArticleItemViewModel(ArticleModel article)
        {
            this.Article = article;
            this.DisplayAuthorLink = true;
        }

        public bool DisplayAuthorLink { get; set; }
        public ArticleModel Article { get; private set; }
    }
}