using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public class ArticleItemViewModel : WtfViewModelBase
    {
        public ArticleItemViewModel(ArticleModel article)
        {
            this.Article = article;
        }

        public ArticleModel Article { get; private set; }
    }
}