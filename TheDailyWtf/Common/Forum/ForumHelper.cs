using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using Inedo.Diagnostics;

namespace TheDailyWtf.Forum
{
    public static class ForumHelper
    {
        private static DateTime nextConnectionAttemptDate;

        public static bool IsWorking { get { return LastException == null; } }
        public static Exception LastException { get; private set; }

        internal static void PauseConnections(Exception ex, int minutes)
        {
            if (LastException == null)
                Logger.Error($"Pausing forum connections for {minutes} minutes, error: {ex}");
            else
                Logger.Information($"Forum connections already paused, new error is: {ex}");

            LastException = ex;
            nextConnectionAttemptDate = DateTime.UtcNow.AddMinutes(minutes);
        }

        internal static void UnpauseConnections()
        {
            LastException = null;
            nextConnectionAttemptDate = DateTime.UtcNow;
            Logger.Information("Forum connections manually unpaused.");
        }

        public static IForumApi CreateApi()
        {
#if DEBUG
            Logger.Debug("Forcing use of real forum API because site compiled in DEBUG mode.");
            return new ForumApi(Config.NodeBB.Host);
#else
            if (IsWorking)
            {
                Logger.Debug("Using the real forum API because there has not been a recent connection error.");
                return new ForumApi(Config.NodeBB.Host);
            }

            if (nextConnectionAttemptDate < DateTime.UtcNow)
            {
                Logger.Information("Attempting to connect to the forum again.");
                LastException = null;
                return new ForumApi(Config.NodeBB.Host);
            }

            Logger.Debug("Using the mock forum API because there was an error previously.");
            return new MockForumApi();
#endif
        }

        public static string SideBarUrl
        {
            get
            {
                return string.Format("https://{0}/category/{1}", Config.NodeBB.Host, Config.NodeBB.SideBarWtfCategory);
            }
        }

        public static IEnumerable<Topic> SideBarWtfs
        {
            get
            {
                try
                {
                    return ForumCache.GetOrAdd(
                        "SideBarWtfs",
                        () =>
                        {
                            Logger.Debug("Getting Side Bar WTFs.");

                            var api = ForumHelper.CreateApi();
                            return api.GetTopicsByCategory(new Category(Config.NodeBB.SideBarWtfCategory))
                                .Where(topic => !topic.Pinned)
                                .Take(5);
                        });
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error getting Side Bar WTFs, error: {ex}");
                    return Enumerable.Empty<Topic>();
                }
            }
        }

        private static class ForumCache
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
