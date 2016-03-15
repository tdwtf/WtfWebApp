using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public sealed class ArticleCommentsViewModel : ViewCommentsViewModel
    {
        public ArticleCommentsViewModel(ArticleModel article, int page, bool isAdmin) : base(article, page)
        {
            this.isAdmin = isAdmin;
        }

        public override string BaseUrl { get { return "/admin/article/comments/" + Article.Id; } }
        public override bool CanFeature { get { return true; } }
        private bool isAdmin;
        public override bool CanEditDelete { get { return isAdmin; } }
        public override bool CanReply { get { return false; } }
    }
}