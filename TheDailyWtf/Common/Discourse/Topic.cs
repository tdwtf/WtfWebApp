using System;
using System.Collections.Generic;
using System.Linq;

namespace TheDailyWtf.Discourse
{
    public sealed class Topic
    {
        public static readonly Topic Null = new Topic() { Slug = "", Title = "Empty Topic", Posts = Enumerable.Empty<Post>() };

        private Topic() { }

        public int Id { get; private set; }
        public string Slug { get; private set; }
        public string Title { get; private set; }
        public IEnumerable<Post> Posts { get; private set; }
        public int PostsCount { get; private set; }
        public bool Pinned { get; private set; }
        public bool Visible { get; private set; }
        public DateTime CreatedDate { get; private set; }
        public DateTime? LastPostedAt { get; private set; }

        public string Url
        {
            get { return string.Format("//{0}/t/{1}/{2}", Config.Discourse.Host, this.Slug, this.Id); }
        }

        public static Topic CreateFromJson(dynamic topic)
        {
            var posts = new List<Post>();
            if (topic.post_stream != null)
            {
                foreach (dynamic post in topic.post_stream.posts)
                    posts.Add(Post.CreateFromJson(post));
            }

            return new Topic()
            {
                Id = topic.id,
                Slug = topic.slug,
                Title = topic.title,
                PostsCount = topic.posts_count,
                Posts = posts.AsReadOnly(),
                Pinned = topic.pinned,
                Visible = topic.visible,
                CreatedDate = topic.created_at,
                LastPostedAt = topic.last_posted_at
            };
        }

        public static Topic CreateFromPostJson(dynamic topic, string title)
        {
            return new Topic()
            {
                Id = topic.topic_id,
                Slug = topic.topic_slug,
                Title = title,
                Posts = new Post[0]
            };
        }

        public override string ToString()
        {
            return string.Format("Topic {0}: \"{1}\" ({2} posts)", this.Id, this.Title, this.PostsCount);
        }
    }
}
