using System;
using System.Collections.Generic;

namespace TheDailyWtf.Discourse
{
    public sealed class Topic
    {
        private Topic() { }

        public int Id { get; private set; }
        public string Slug { get; private set; }
        public string Title { get; private set; }
        public IEnumerable<Post> Posts { get; private set; }
        public int PostsCount { get; private set; }
        public bool Pinned { get; private set; }
        public bool Visible { get; private set; }
        public DateTime CreatedDate { get; private set; }

        public string Url
        {
            get { return string.Format("/t/{0}/{1}", this.Slug, this.Id); }
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
                CreatedDate = topic.created_at
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
