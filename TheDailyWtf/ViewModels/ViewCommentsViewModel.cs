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
            var comments = CommentModel.FromArticle(article);
            this.AllComments = comments;
            this.Comments = comments.Skip((page - 1) * CommentsPerPage).Take(CommentsPerPage).ToList();
            this.Articles = new Dictionary<int, ArticleModel> { { article.Id.Value, article } };
            this.PageNumber = page;

            this.Comment = new CommentFormModel();
        }

        public ViewCommentsViewModel(ArticleModel article, IList<CommentModel> comments)
        {
            this.Article = article;
            this.Comments = comments;
            this.Articles = new Dictionary<int, ArticleModel> { { article.Id.Value, article } };
            this.PageNumber = -1;
        }

        public ViewCommentsViewModel(IList<CommentModel> comments, int page)
        {
            this.Article = null;
            this.AllComments = comments;
            this.Comments = comments.Skip((page - 1) * CommentsPerPage).Take(CommentsPerPage).ToList();
            this.Articles = this.Comments.Select(c => c.ArticleId).Distinct().ToDictionary(id => id, id => ArticleModel.GetArticleById(id));
            this.PageNumber = page;
        }

        public virtual string BaseUrl { get { return Article.CommentsUrl; } }
        public virtual bool CanFeature { get { return false; } }
        public virtual bool CanEditDelete { get { return false; } }
        public virtual bool CanReply { get { return this.PageNumber != -1; } }
        public ArticleModel Article { get; private set; }
        public IReadOnlyDictionary<int, ArticleModel> Articles { get; private set; }
        public IList<CommentModel> AllComments { get; protected set; }
        public IList<CommentModel> Comments { get; protected set; }
        public int PageNumber { get; private set; }
        public int PageCount
        {
            get
            {
                return ((this.AllComments != null ? this.AllComments.Count : this.Article.CachedCommentCount) + CommentsPerPage - 1) / CommentsPerPage;
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
                if (this.Comments.Count() < this.Article.CachedCommentCount)
                    return string.Format("{0} of {1}", this.Comments.Count, this.Article.CachedCommentCount);
                else
                    return this.Article.CachedCommentCount.ToString();
            }
        }
        public CommentFormModel Comment { get; set; }
    }
}