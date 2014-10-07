using System;
using System.Collections.Generic;

namespace TheDailyWtf.Discourse
{
    public sealed class Post
    {
        public enum PostType { Regular = 1, ModeratorAction = 2 }

        private Post() { }

        public static readonly IEqualityComparer<Post> IdComparer = new PostIdEqualityComparer();

        public int Id { get; private set; }
        public string Username { get; private set; }
        public string BodyHtml { get; private set; }
        public bool Hidden { get; private set; }
        public DateTime PostDate { get; private set; }
        public PostType Type { get; private set; }
        public int Sequence { get; private set; }
        public int TopicId { get; private set; }
        public string TopicSlug { get; private set; }
        public string ImageUrl { get; private set; }

        public string Url { get { return string.Format("/t/{0}/{1}/{2}", this.TopicSlug, this.TopicId, this.Id); } }

        public static Post CreateFromJson(dynamic post)
        {
            return new Post()
            {
                Id = post.id,
                Username = post.username,
                BodyHtml = post.cooked,
                Hidden = post.hidden,
                PostDate = post.created_at,
                Type = (PostType)post.post_type,
                Sequence = post.post_number,
                TopicId = post.topic_id,
                TopicSlug = post.topic_slug,
                ImageUrl = post.avatar_template.ToString().Replace("{size}", "40")
            };
        }

        public override string ToString()
        {
            return string.Format("Post {0} - by {1}: {2}", this.Id, this.Username, this.BodyHtml);
        }

        private sealed class PostIdEqualityComparer : IEqualityComparer<Post>
        {
            public bool Equals(Post x, Post y)
            {
                if (x == null || y == null)
                    return false;
                return x.Id == y.Id;
            }

            public int GetHashCode(Post obj)
            {
                return obj.Id.GetHashCode();
            }
        }

    }
}
