using System.Collections.Generic;
using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public class ViewCommentsViewModel : WtfViewModelBase
    {
        public ViewCommentsViewModel(ArticleModel article)
        {
            this.Article = article;
            this.Comments = CommentModel.FromArticle(article);
        }

        public ArticleModel Article { get; private set; }
        public IEnumerable<CommentModel> Comments { get; private set; }
    }
}