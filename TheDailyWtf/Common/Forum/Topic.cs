using System;
using System.Web;

namespace TheDailyWtf.Forum
{
    public sealed class Topic
    {
        private Topic() { }

        public int Id { get; private set; }
        public string Slug { get; private set; }
        public HtmlString Title { get; private set; }
        public int PostsCount { get; private set; }
        public bool Pinned { get; private set; }
        public DateTime CreatedDate { get; private set; }
        public DateTime? LastPostedAt { get; private set; }

        public string Url
        {
            get { return string.Format("https://{0}/topic/{1}", Config.NodeBB.Host, this.Slug); }
        }

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static Topic CreateFromJson(dynamic topic)
        {
            return new Topic()
            {
                Id = topic.tid,
                Slug = topic.slug,
                Title = new HtmlString(((string) topic.title).Replace("<", "&lt;").Replace(">", "&gt;")),
                PostsCount = topic.postcount,
                Pinned = topic.pinned,
                CreatedDate = Epoch.AddMilliseconds((long) topic.timestamp),
                LastPostedAt = Epoch.AddMilliseconds((long) topic.lastposttime)
            };
        }

        public override string ToString()
        {
            return string.Format("Topic {0}: \"{1}\" ({2} posts)", this.Id, this.Title, this.PostsCount);
        }
    }
}
