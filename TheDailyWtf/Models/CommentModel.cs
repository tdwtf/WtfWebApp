using System;
using System.Collections.Generic;
using System.Linq;
using TheDailyWtf.Discourse;

namespace TheDailyWtf.Models
{
    public class CommentModel
    {
        public string BodyHtml { get; set; }
        public string Username { get; set;}
        public DateTime PublishedDate { get; set; }
        public int DiscoursePostId { get; set; }
        public string ImageUrl { get; set; }

        public static IEnumerable<CommentModel> GetFeaturedCommentsForArticle(ArticleModel article)
        {
            if (article.DiscourseTopicId != null)
            {
                var comments = DiscourseHelper.GetFeaturedCommentsForArticle(article.Id);
                return comments.Select(c => CommentModel.FromDiscourse(c));
            }

            return new CommentModel[0];
        }

        private static CommentModel FromDiscourse(Post post)
        {
            return new CommentModel()
            {
                BodyHtml = post.BodyHtml,
                Username = post.Username,
                PublishedDate = post.PostDate,
                DiscoursePostId = post.Id,
                ImageUrl = post.ImageUrl
            };
        }
    }
}