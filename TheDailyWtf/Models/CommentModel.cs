using System;
using System.Collections.Generic;

namespace TheDailyWtf.Models
{
    public class CommentModel
    {
        public string BodyHtml { get; set; }
        public AuthorModel Author { get; set;}
        public string PublishedDate { get; set; }
        public int DiscoursePostId { get; set; }

        public static IEnumerable<CommentModel> GetFeaturedCommentsForArticle(int articleId)
        {
            yield return GetCommentById(1);
            yield return GetCommentById(2);
            yield return GetCommentById(3);
        }

        private static CommentModel GetCommentById(int id)
        {
            return new CommentModel()
            {
                BodyHtml = "hello dears",
                Author = AuthorModel.GetAuthorBySlug("hdars"),
                PublishedDate = DateTime.Now.ToString("MMMM dd yyyy"),
                DiscoursePostId = id
            };
        }
    }
}