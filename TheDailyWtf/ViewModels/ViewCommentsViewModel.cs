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
            if (this.MaxDiscoursePostId > 0)
                this.DiscourseTopicUrl = this.Article.DiscourseThreadUrl + "/" + this.MaxDiscoursePostId;
            else
                this.DiscourseTopicUrl = this.Article.DiscourseThreadUrl;
        }

        public ArticleModel Article { get; private set; }
        public IEnumerable<CommentModel> Comments { get; private set; }
        public int MaxDiscoursePostId { get; private set; }
        public string DiscourseTopicUrl { get; private set; }
    }
}