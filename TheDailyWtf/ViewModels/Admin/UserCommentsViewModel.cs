using System;
using System.Collections.Generic;
using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public sealed class UserCommentsViewModel : ViewCommentsViewModel
    {
        private UserCommentsViewModel(string prefix, IList<CommentModel> comments, int page, int total) : base(comments, page, total)
        {
            this.prefix = prefix;
        }

        private string prefix;
        public override string BaseUrl { get { return "/admin/user-comments/" + prefix; } }
        public override bool CanFeature { get { return false; } }
        public override bool CanEditDelete { get { return true; } }
        public override bool CanReply { get { return false; } }

        public static UserCommentsViewModel ByIP(string ip, int page)
        {
            var total = CommentModel.CountCommentsByIP(ip);
            var comments = CommentModel.GetCommentsByIP(ip, (page - 1) * CommentsPerPage, CommentsPerPage);
            return new UserCommentsViewModel("by-ip/" + Uri.EscapeDataString(ip), comments, page, total);
        }

        public static UserCommentsViewModel ByToken(string token, int page)
        {
            var total = CommentModel.CountCommentsByToken(token);
            var comments = CommentModel.GetCommentsByToken(token, (page - 1) * CommentsPerPage, CommentsPerPage);
            return new UserCommentsViewModel("by-token/" + Uri.EscapeDataString(token), comments, page, total);
        }
    }
}