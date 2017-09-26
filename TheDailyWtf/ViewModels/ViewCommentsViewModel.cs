using System.Collections.Generic;
using System.Linq;
using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public class ViewCommentsViewModel : WtfViewModelBase
    {
        public const int CommentsPerPage = 50;
        public const int NearbyPages = 3;

        public ViewCommentsViewModel(ArticleModel article, int page)
        {
            this.Article = article;
            this.TotalComments = article.CachedCommentCount;
            this.Comments = CommentModel.FromArticle(article, (page - 1) * CommentsPerPage, CommentsPerPage);
            this.PageNumber = page;

            this.Comment = new CommentFormModel();
        }

        public ViewCommentsViewModel(ArticleModel article, IList<CommentModel> comments)
        {
            this.Article = article;
            this.TotalComments = article.CachedCommentCount;
            this.Comments = comments;
            this.PageNumber = -1;
        }

        public ViewCommentsViewModel(IList<CommentModel> comments, int page, int totalComments)
        {
            this.Article = null;
            this.TotalComments = totalComments;
            this.Comments = comments;
            this.PageNumber = page;
        }

        public virtual string BaseUrl { get { return Article.CommentsUrl; } }
        public virtual bool CanFeature { get { return false; } }
        public virtual bool CanEditDelete { get { return false; } }
        public virtual bool CanReply { get { return this.PageNumber != -1; } }
        public ArticleModel Article { get; }
        public int TotalComments { get; }
        public IList<CommentModel> Comments { get; }
        public int PageNumber { get; }
        public int PageCount
        {
            get
            {
                return (this.TotalComments + CommentsPerPage - 1) / CommentsPerPage;
            }
        }
        public string ViewCommentsHeading
        {
            get
            {
                return string.Format("(Viewing {0} comments)", this.CommentsFraction);
            }
        }
        public string CommentsFraction
        {
            get
            {
                if (this.Comments.Count() < this.TotalComments)
                    return string.Format("{0} of {1}", this.Comments.Count, this.TotalComments);
                else
                    return this.TotalComments.ToString();
            }
        }
        public CommentFormModel Comment { get; set; }
    }
}