using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using TheDailyWtf.Data;
using TheDailyWtf.Discourse;

namespace TheDailyWtf.Models
{
    public class CommentModel
    {
        private static readonly Regex ImgSrcRegex = new Regex(@"src=""(?<comment>[^""]+)""", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

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

        public static IEnumerable<CommentModel> FromArticle(ArticleModel article)
        {
            var comments = StoredProcs.Comments_GetComments(article.Id).Execute();
            return comments.Select(c => CommentModel.FromTable(c));
        }

        public static string TrySanitizeDiscourseBody(string body)
        {
            try
            {
                // image src attributes in Discourse comment bodies are relative,
                // make them absolute to avoid image 404s on comments overview

                string replaced = ImgSrcRegex.Replace(
                    body,
                    m =>
                    {
                        string value = m.Groups["comment"].Value;
                        if (value.StartsWith("//"))
                            return string.Format("src=\"{0}\"", value);

                        return string.Format("src=\"//{0}{1}\"", WebConfigurationManager.AppSettings["Discourse.Host"], value);
                    }
                );

                return replaced;
            }
            catch
            {
                return body;
            }
        }

        private static CommentModel FromTable(Tables.Comments comment)
        {
            return new CommentModel()
            {
                BodyHtml = comment.Body_Html,
                Username = comment.User_Name,
                DiscoursePostId = comment.Discourse_Post_Id,
                PublishedDate = comment.Posted_Date
            };
        }

        private static CommentModel FromDiscourse(Post post)
        {
            return new CommentModel()
            {
                BodyHtml = TrySanitizeDiscourseBody(post.BodyHtml),
                Username = post.Username,
                PublishedDate = post.PostDate,
                DiscoursePostId = post.Id,
                ImageUrl = post.ImageUrl
            };
        }
    }
}