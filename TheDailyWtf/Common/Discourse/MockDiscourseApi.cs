using System.Collections.Generic;
using System.Linq;

namespace TheDailyWtf.Discourse
{
    internal sealed class MockDiscourseApi : IDiscourseApi
    {
        public Topic CreateTopic(Category category, string title, string body)
        {
            return Topic.Null;
        }

        public void SetVisibility(int topicId, bool visible)
        {
        }

        public void SetCategoryTopic(Topic topic, Category category)
        {
        }

        public void SetTopicsCategoryBulk(IEnumerable<Topic> topics, Category category)
        {
        }

        public IEnumerable<Topic> GetTopicsByCategory(Category category, string filter = "latest")
        {
            return Enumerable.Empty<Topic>();
        }

        public IEnumerable<Topic> GetTopics(string filter = "latest")
        {
            return Enumerable.Empty<Topic>();
        }

        public Topic GetTopic(int id)
        {
            return Topic.Null;
        }

        public Post GetReplyPost(int postId)
        {
            return Post.Null;
        }

        public void DeletePost(int postId)
        {
        }

        public string CreateUser(string name, string username, string email, string password)
        {
            return null;
        }
    }
}
