using System.Collections.Generic;
using System.Linq;

namespace TheDailyWtf.Forum
{
    internal sealed class MockForumApi : IForumApi
    {
        public IEnumerable<Topic> GetTopicsByCategory(Category category)
        {
            return Enumerable.Empty<Topic>();
        }
    }
}
