using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using Inedo.Data;
using TheDailyWtf.Data;
using TheDailyWtf.Models;

namespace TheDailyWtf.Discourse
{
    public static class DiscourseHelper
    {
        private static DateTime nextDiscourseConnectionAttemptDate;

        public static bool DiscourseWorking { get { return DiscourseException == null; } }
        public static Exception DiscourseException { get; private set; }

        internal static void PauseDiscourseConnections(Exception ex, int minutes)
        {
            DiscourseException = ex;
            nextDiscourseConnectionAttemptDate = DateTime.UtcNow.AddMinutes(minutes);
        }

        public static IDiscourseApi CreateApi()
        {
#if DEBUG
            return new DiscourseApi(Config.Discourse.Host, Config.Discourse.Username, Config.Discourse.ApiKey);
#endif

            if (DiscourseWorking)
                return new DiscourseApi(Config.Discourse.Host, Config.Discourse.Username, Config.Discourse.ApiKey);

            if (nextDiscourseConnectionAttemptDate < DateTime.UtcNow)
            {
                DiscourseException = null;
                return new DiscourseApi(Config.Discourse.Host, Config.Discourse.Username, Config.Discourse.ApiKey);
            }

            return new MockDiscourseApi();
        }

        public static void CreateCommentDiscussion(ArticleModel article)
        {
            var api = DiscourseHelper.CreateApi();

            // create topic and embed <!--ARTICLEID:...--> in the body so the hacky JavaScript 
            // for the "Feature" button can append the article ID to each of its query strings
            var topic = api.CreateTopic(
                new Category(Config.Discourse.CommentCategory),
                article.Title,
                string.Format(
                    "Discussion for the article: {0}\r\n\r\n<!--ARTICLEID:{1}-->", 
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
                var api = DiscourseHelper.CreateApi();

                api.SetVisibility(topicId, true);

                // delete the worthless auto-generated "this topic is now invisible/visible" rubbish
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
                return DiscourseCache.GetOrAdd(
                    "Topic_" + topicId,
                    () =>
                    {
                        var api = DiscourseHelper.CreateApi();
                        return api.GetTopic(topicId);
                    }
                );
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
                return DiscourseCache.GetOrAdd(
                    "FeaturedCommentsForArticle_" + articleId,
                    () =>
                    {
                        var api = DiscourseHelper.CreateApi();

                        return StoredProcs.Articles_GetFeaturedComments(articleId)
                            .Execute()
                            .Select(c => api.GetReplyPost(c.Discourse_Post_Id))
                            .Where(p => !p.Hidden)
                            .ToList()
                            .AsReadOnly();
                    }
                );
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
            try
            {
                return DiscourseCache.GetOrAdd(
                   "SideBarWtfs",
                   () =>
                   {
                       var api = DiscourseHelper.CreateApi();
                       return api.GetTopicsByCategory(new Category(Config.Discourse.SideBarWtfCategory))
                           .Where(topic => !topic.Pinned && topic.Visible)
                           .Take(5)
                           .ToList()
                           .AsReadOnly();
                   }
               );
            }
            catch
            {
                return new Topic[0];
            }
        }

        public static bool PullCommentsFromDiscourse(ArticleModel article)
        {
            const int commentsToPull = 40;

            if (article.DiscourseTopicId == null)
                return false;
            if (article.CachedCommentCount >= commentsToPull)
                return false;

            var cachedComments = StoredProcs.Comments_GetComments(article.Id)
                .Execute()
                .Where(c => c.Discourse_Post_Id != null)
                .ToDictionary(c => (int)c.Discourse_Post_Id);

            var topic = GetDiscussionTopic((int)article.DiscourseTopicId);
            bool commentsPulled = false;

            foreach (var post in topic.Posts.Where(p => !p.Username.Equals("PaulaBean", StringComparison.OrdinalIgnoreCase)).Take(commentsToPull))
            {
                if (cachedComments.ContainsKey(post.Id))
                    continue;
                
                commentsPulled = true;

                StoredProcs.Comments_CreateOrUpdateComment(
                    article.Id, 
                    CommentModel.TrySanitizeDiscourseBody(post.BodyHtml), 
                    post.Username, 
                    post.PostDate, 
                    post.Id
                  ).Execute();
            }

            return commentsPulled;
        }

        private static class DiscourseCache
        {
            private static readonly object locker = new object();

            /// <summary>
            /// Gets an item from the cache if it exists, otherwise creates and adds the item to the cache.
            /// </summary>
            /// <typeparam name="TItem">The type of the item.</typeparam>
            /// <param name="key">The key.</param>
            /// <param name="getItem">The get item.</param>
            public static TItem GetOrAdd<TItem>(string key, Func<TItem> getItem)
                where TItem : class
            {
                lock (locker)
                {
                    var cached = HttpContext.Current.Cache[key] as TItem;
                    if (cached != null)
                        return cached;

                    var item = getItem();
                    if (item == null)
                        return null;

                    HttpContext.Current.Cache.Add(key, item, null, DateTime.Now.AddMinutes(5), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                    return item;
                }
            }
        }
    }
}
