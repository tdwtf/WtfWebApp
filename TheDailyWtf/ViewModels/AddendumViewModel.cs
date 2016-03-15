using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public class AddendumViewModel : WtfViewModelBase
    {
        public AddendumViewModel(ArticleModel article, CommentModel comment)
        {
            this.Article = article;
            this.Comment = comment;
            this.ShowLeaderboardAd = false;
        }

        public ArticleModel Article { get; private set; }
        public CommentModel Comment { get; private set; }
    }
}