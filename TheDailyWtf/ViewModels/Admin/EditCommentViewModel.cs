using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public class EditCommentViewModel : WtfViewModelBase
    {
        public ArticleModel Article { get; set; }
        public CommentModel Comment { get; set; }
        public CommentFormModel Post { get; set; }
    }
}