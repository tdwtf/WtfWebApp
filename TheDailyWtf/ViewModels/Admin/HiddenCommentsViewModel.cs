using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public sealed class HiddenCommentsViewModel : ViewCommentsViewModel
    {
        public HiddenCommentsViewModel(int page, string authorSlug) : base(CommentModel.GetHiddenComments(authorSlug), page)
        {
            this.authorSlug = authorSlug;
        }

        public override string BaseUrl { get { return "/admin/comment-moderation"; } }
        public override bool CanFeature { get { return true; } }
        private readonly string authorSlug;
        public override bool CanEditDelete { get { return this.authorSlug == null; } }
        public override bool CanReply { get { return false; } }
    }
}