using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public sealed class ArticleCommentsViewModel : ViewCommentsViewModel
    {
        public ArticleCommentsViewModel(int id, int page) : base(ArticleModel.GetArticleById(id), page)
        {
        }

        public override string BaseUrl { get { return "/admin/article/comments/" + Article.Id; } }
    }
}