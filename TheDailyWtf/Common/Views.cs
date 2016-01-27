
namespace TheDailyWtf
{
    /// <summary>
    /// Helper class that contains all the view names as strongly-typed constants.
    /// </summary>
    public static class Views
    {
        public static class Articles
        {
            public const string Index = "Index";
            public const string ViewArticle = "ViewArticle";
            public const string ViewArticleComments = "ViewArticleComments";
            public const string Submit = "Submit";
        }

        public static class Authors
        {
            public const string ViewAuthor = "ViewAuthor";
        }

        public static class Home
        {
            public const string Index = "Index";
            public const string Contact = "Contact";
        }

        public static class Info
        {
            public const string Advertise = "Advertise";
            public const string Privacy = "Privacy";
        }

        public static class Shared
        {
            public const string Layout = "_Layout";
            public const string PartialArticleItem = "PartialArticleItem";
            public const string PartialComments = "PartialComments";
            public const string PartialCommentsPages = "PartialCommentsPages";
            public const string PartialNavigationMenu = "PartialNavigationMenu";
            public const string PartialRecentArticleList = "PartialRecentArticleList";
            public const string PartialLeaderboardAd = "PartialLeaderboardAd";
            public const string PartialSidebarAd = "PartialSidebarAd";
        }
    }
}