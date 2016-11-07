using System;
using System.Web.Mvc;
using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public class AddendumViewModel : WtfViewModelBase
    {
        public AddendumViewModel(ArticleModel article, CommentModel comment)
        {
            this.Article = article;
            this.Comment = comment;
            this.ShowLeaderboardAd = false;
        }

        public int MaxBodyLength
        {
            get
            {
                int maxLength = CommentFormModel.MaxBodyLength - this.Comment.BodyRaw.Length - "\n\n**Addendum :**\n".Length - DateTime.Now.ToString().Length;
                return maxLength > 0 ? maxLength : 0;
            }
        }

        public ArticleModel Article { get; private set; }
        public CommentModel Comment { get; private set; }
        [AllowHtml]
        public string Body { get; set; }
    }
}