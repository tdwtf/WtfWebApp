using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using Inedo.Data;
using Inedo.Diagnostics;
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
            if (DiscourseException == null)
                Logger.Error("Pausing Discourse connections for {0} minutes, error: {1}", minutes, ex);
            else
                Logger.Information("Discourse connections already paused, new error is: " + ex);

            DiscourseException = ex;
            nextDiscourseConnectionAttemptDate = DateTime.UtcNow.AddMinutes(minutes);
        }

        internal static void UnpauseDiscourseConnections()
        {
            DiscourseException = null;
            nextDiscourseConnectionAttemptDate = DateTime.UtcNow;
            Logger.Information("Discourse connections manually unpaused.");
        }

        public static IDiscourseApi CreateApi()
        {
#if DEBUG
            Logger.Debug("Forcing use of real Discourse API because site compiled in DEBUG mode.");
            return new DiscourseApi(Config.Discourse.Host, Config.Discourse.Username, Config.Discourse.ApiKey);
#endif

            if (DiscourseWorking)
            {
                Logger.Debug("Using the real Discourse API because there has not been a recent Discourse connection error.");
                return new DiscourseApi(Config.Discourse.Host, Config.Discourse.Username, Config.Discourse.ApiKey);
            }

            if (nextDiscourseConnectionAttemptDate < DateTime.UtcNow)
            {
                Logger.Information("Attempting to connect to Discourse again.");
                DiscourseException = null;
                return new DiscourseApi(Config.Discourse.Host, Config.Discourse.Username, Config.Discourse.ApiKey);
            }

            Logger.Debug("Using the mock Discourse API because there was an error previously.");
            return new MockDiscourseApi();
        }

        public static int CreateCommentDiscussion(ArticleModel article)
        {
            Logger.Information("Creating comment discussion for article \"{0}\" (ID={1}).", article.Title, article.Id);

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

            return topic.Id;
        }

        public static void OpenCommentDiscussion(int articleId, int topicId)
        {
            Logger.Information("Opening comment discussion for article (ID={0}) and topic (ID={1}).", articleId, topicId);
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
            Logger.Information("Reassigning comment discussion for article (ID={0}) and topic (ID={1}).", articleId, topicId);
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
                        Logger.Debug("Getting and caching comment discussion for topic (ID={0}).", topicId);
                        var api = DiscourseHelper.CreateApi();
                        return api.GetTopic(topicId);
                    }
                );
            }
            catch (Exception ex)
            {
                Logger.Error("Error getting comment discussion for topic (ID={0}), error: {1}", topicId, ex);
                throw new InvalidOperationException(
                    string.Format("An unknown error occurred when attempting to get the Discourse topic #{0}. "
                   + "Verify that this Discourse topic ID actually exists (e.g. /t/{0} relative to forum)", topicId), ex);
            }
        }

        public static IList<Post> GetFeaturedCommentsForArticle(int articleId)
        {
            // There was never a way to feature Discourse comments, so there's no sense implementing one while we're replacing the system.
            return new Post[0];
        }

        public static void UnfeatureComment(int articleId, int discourseTopicId)
        {
            Logger.Information("Unfeaturing comment for article (ID={0}) and discourse topic (ID={1}).", articleId, discourseTopicId);

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
                       Logger.Debug("Getting Side Bar WTFs.");

                       var api = DiscourseHelper.CreateApi();
                       return api.GetTopicsByCategory(new Category(Config.Discourse.SideBarWtfCategory))
                           .Where(topic => !topic.Pinned && topic.Visible)
                           .Take(5)
                           .ToList()
                           .AsReadOnly();
                   }
               );
            }
            catch (Exception ex)
            {
                Logger.Error("Error getting Side Bar WTFs, error: {0}", ex);
                return new Topic[0];
            }
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

                    HttpContext.Current.Cache.Add(key, item, null, DateTime.Now.AddMinutes(10), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                    return item;
                }
            }
        }
    }
}
