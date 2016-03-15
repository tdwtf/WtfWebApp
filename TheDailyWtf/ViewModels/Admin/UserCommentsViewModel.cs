using System.Collections.Generic;
using System.Linq;
using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public sealed class UserCommentsViewModel : ViewCommentsViewModel
    {
        public UserCommentsViewModel(string prefix, IEnumerable<CommentModel> comments, int page) : base(comments, page)
        {
            this.prefix = prefix;
        }

        private string prefix;
        public override string BaseUrl { get { return "/admin/user-comments/" + prefix; } }
        public override bool CanFeature { get { return false; } }
        public override bool CanEditDelete { get { return true; } }
        public override bool CanReply { get { return false; } }
    }
}