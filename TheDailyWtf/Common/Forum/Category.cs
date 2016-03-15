using System.Text.RegularExpressions;

namespace TheDailyWtf.Forum
{
    public sealed class Category
    {
        public Category(string url)
        {
            this.UrlFormatted = url;
        }

        public string UrlFormatted { get; private set; }
    }
}
