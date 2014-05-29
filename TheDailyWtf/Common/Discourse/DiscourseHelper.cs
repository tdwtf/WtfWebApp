using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using Inedo.Data;
using TheDailyWtf.Data;
using TheDailyWtf.Models;

namespace TheDailyWtf.Discourse
{
    public static class DiscourseHelper
    {
        public static void CreateCommentDiscussion(ArticleModel article)
        {
            var api = new DiscourseApi();

            // create topic and embed <!--ARTICLEID:...--> in the body so the hacky JavaScript 
            // for the "Feature" button can append the article ID to each of its query strings
            var topic = api.CreateTopic(
                new Category(WebConfigurationManager.AppSettings["Discourse.CommentCategory"]),
                article.Title,
                string.Format(
                    "Discussion for the article: http://{0}\r\n\r\n<!--ARTICLEID:{1}-->", 
                    article.Url,
                    article.Id
                )
            );

            api.SetVisibility(topic.Id, false);

            StoredProcs
                .Articles_CreateOrUpdateArticle(article.Id, Discourse_Topic_Id: topic.Id)
                .Execute();
        }

        public static void OpenCommentDiscussion(int articleId, int topicId)
        {
            try
            {
                var api = new DiscourseApi();

                api.SetVisibility(topicId, true);

                // delete the worthless auto-generated "this topic is now invisble/visible" rubbish
                var topic = api.GetTopic(topicId);
                foreach (var post in topic.Posts.Where(p => p.Type == Post.PostType.ModeratorAction))
                    api.DeletePost(post.Id);

                StoredProcs
                    .Articles_CreateOrUpdateArticle(articleId, Discourse_Topic_Opened: YNIndicator.Yes)
                    .Execute();
            }
            catch (Exception ex) 
            {
                throw new InvalidOperationException(
                    string.Format("An unknown error occurred when attempting to open the Discourse discussion. " 
                    + "Verify that the article #{0} is assigned to a valid correct topic ID (currently #{1})", articleId, topicId), ex);
            }
        }

        public static void ReassignCommentDiscussion(int articleId, int topicId)
        {
            StoredProcs
                .Articles_CreateOrUpdateArticle(
                    articleId, 
                    Discourse_Topic_Id: topicId, 
                    Discourse_Topic_Opened: YNIndicator.No
                ).Execute();
        }

        public static Topic GetDiscussionTopic(int topicId)
        {
            try
            {
                var api = new DiscourseApi();
                return api.GetTopic(topicId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    string.Format("An unknown error occurred when attempting to get the Discourse topic #{0}. "
                   + "Verify that this Discourse topic ID actually exists (e.g. /t/{0} relative to forum)", topicId), ex);
            }
        }

        public static IList<Post> GetFeaturedCommentsForArticle(int articleId)
        {
            try
            {
                var api = new DiscourseApi();

                return StoredProcs.Articles_GetFeaturedComments(articleId)
                    .Execute()
                    .Select(c => api.GetReplyPost(c.Discourse_Post_Id))
                    .Where(p => !p.Hidden)
                    .ToList()
                    .AsReadOnly();
            }
            catch
            {
                return new Post[0];
            }
        }

        public static void UnfeatureComment(int articleId, int discourseTopicId)
        {
            StoredProcs
                .Articles_UnfeatureComment(articleId, discourseTopicId)
                .Execute();
        }

        public static IList<Topic> GetSideBarWtfs()
        {
            var api = new DiscourseApi();
            return api.GetTopicsByCategory(new Category(WebConfigurationManager.AppSettings["Discourse.SideBarWtfCategory"]))
                .Where(topic => !topic.Pinned && topic.Visible)
                .Take(5)
                .ToList()
                .AsReadOnly();
        }
    }
}
