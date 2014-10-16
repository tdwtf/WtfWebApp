using System.Collections.Generic;

namespace TheDailyWtf.Discourse
{
    public interface IDiscourseApi
    {
        Topic CreateTopic(Category category, string title, string body);
        void SetVisibility(int topicId, bool visible);
        void SetCategoryTopic(Topic topic, Category category);
        void SetTopicsCategoryBulk(IEnumerable<Topic> topics, Category category);
        IEnumerable<Topic> GetTopicsByCategory(Category category, string filter = "latest");
        IEnumerable<Topic> GetTopics(string filter = "latest");
        Topic GetTopic(int id);
        Post GetReplyPost(int postId);
        void DeletePost(int postId);
        string CreateUser(string name, string username, string email, string password);
    }
}