using System.Text.RegularExpressions;

namespace TheDailyWtf.Discourse
{
    public sealed class Category
    {
        public Category(string name)
        {
            this.Name = name;

            string lower = name.ToLowerInvariant();
            string alphaNumericWithDashes = Regex.Replace(lower, @"[^A-Z0-9]", "-", RegexOptions.IgnoreCase);
            this.UrlFormatted = Regex.Replace(alphaNumericWithDashes, @"-+", "-").Trim('-');
        }

        public string Name { get; private set; }
        public string UrlFormatted { get; private set; }
    }
}
