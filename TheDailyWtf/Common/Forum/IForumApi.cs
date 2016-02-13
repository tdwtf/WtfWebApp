using System.Collections.Generic;

namespace TheDailyWtf.Forum
{
    public interface IForumApi
    {
        IEnumerable<Topic> GetTopicsByCategory(Category category);
    }
}