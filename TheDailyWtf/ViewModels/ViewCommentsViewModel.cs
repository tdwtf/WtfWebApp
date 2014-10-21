using System;
using System.Collections.Generic;
using System.Linq;
using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public class ViewCommentsViewModel : WtfViewModelBase
    {
        public ViewCommentsViewModel(ArticleModel article)
        {
            this.Article = article;
            this.Comments = CommentModel.FromArticle(article);
            this.MaxDiscoursePostId = this.Comments.Any() ? this.Comments.Max(c => c.DiscoursePostId ?? 0) : 0;
            this.DiscourseTopicUrl = this.Article.DiscourseThreadUrl + "/last";
        }

        public ArticleModel Article { get; private set; }
        public IEnumerable<CommentModel> Comments { get; private set; }
        public int MaxDiscoursePostId { get; private set; }
        public string DiscourseTopicUrl { get; private set; }
        public string ViewCommentsHeading
        {
            get
            {
                return string.Format(
                    "Article Comments ({0} {1} comments)",
                    this.Article.CachedCommentCount < this.Article.DiscourseCommentCount ? "Previewing" : "Viewing",
                    this.CommentsFraction
                );
            }
        }
        public string CommentsFraction
        {
            get
            {
                if (this.Article.CachedCommentCount < this.Article.DiscourseCommentCount)
                    return string.Format("{0}/{1}", this.Article.CachedCommentCount, Math.Max(this.Article.DiscourseCommentCount, this.Article.CachedCommentCount));
                else
                    return this.Article.CachedCommentCount.ToString();
            }
        }
    }
}