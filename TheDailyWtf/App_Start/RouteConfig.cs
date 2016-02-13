using System.Web.Mvc;
using System.Web.Routing;

namespace TheDailyWtf
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            RegisterLegacyForumRoutes(routes);

            routes.MapRoute(
                name: "DefaultLegacy",
                url: "Default.aspx",
                defaults: new { controller = "Home", action = "Index" }
            );

            routes.MapRoute(
                name: "GoogleNow",
                url: "{id}",
                constraints: new { id = @"\d+" },
                defaults: new { controller = "Articles", action = "ViewArticleById" }
            );

            routes.MapRoute(
                name: "MyArticlesAdmin",
                url: "admin/my-articles",
                defaults: new { controller = "Admin", action = "MyArticles" }
            );

            routes.MapRoute(
                name: "ArticleAdmin",
                url: "admin/article/edit/{id}",
                defaults: new { controller = "Admin", action = "EditArticle", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ArticleCommentsAdmin",
                url: "admin/article/comments/{id}/{page}",
                defaults: new { controller = "Admin", action = "ArticleComments", page = 1 }
            );

            routes.MapRoute(
                name: "UserIPCommentsAdmin",
                url: "admin/user-comments/by-ip/{ip}/{page}",
                defaults: new { controller = "Admin", action = "CommentsByIP", page = 1 }
            );

            routes.MapRoute(
                name: "UserTokenCommentsAdmin",
                url: "admin/user-comments/by-token/{token}/{page}",
                defaults: new { controller = "Admin", action = "CommentsByToken", page = 1 }
            );

            routes.MapRoute(
                name: "DeleteCommentsAdmin",
                url: "admin/delete-comments",
                defaults: new { controller = "Admin", action = "DeleteComments" }
            );

            routes.MapRoute(
                name: "EditCommentAdmin",
                url: "admin/edit-comment",
                defaults: new { controller = "Admin", action = "EditComment" }
            );

            routes.MapRoute(
                name: "FeatureComment",
                url: "admin/feature-comment",
                defaults: new { controller = "Admin", action = "FeatureComment" }
            );

            routes.MapRoute(
                name: "UnfeatureComment",
                url: "admin/unfeature-comment",
                defaults: new { controller = "Admin", action = "UnfeatureComment" }
            );

            routes.MapRoute(
                name: "AuthorAdmin",
                url: "admin/author/edit/{slug}",
                defaults: new { controller = "Admin", action = "EditAuthor", slug = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "SeriesAdmin",
                url: "admin/series/edit/{slug}",
                defaults: new { controller = "Admin", action = "EditSeries", slug = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AdAdmin",
                url: "admin/ad/edit/{id}",
                defaults: new { controller = "Admin", action = "EditAd", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DeleteAd",
                url: "admin/ad/delete/{id}",
                defaults: new { controller = "Admin", action = "DeleteAd" }
            );

            routes.MapRoute(
                name: "ReenableDiscourse",
                url: "admin/reenable-discourse",
                defaults: new { controller = "Admin", action = "ReenableDiscourse" }
            );

            routes.MapRoute(
                name: "ViewAds",
                url: "admin/ads",
                defaults: new { controller = "Admin", action = "ViewAds" }
            );

            routes.MapRoute(
                name: "Rss",
                url: "rss",
                defaults: new { controller = "Home", action = "Rss" }
            );

            routes.MapRoute(
                name: "RssLegacy",
                url: "Rss.aspx",
                defaults: new { controller = "Home", action = "Rss" }
            );

            routes.MapRoute(
                name: "Contact",
                url: "contact",
                defaults: new { controller = "Home", action = "Contact" }
            );

            routes.MapRoute(
                name: "ContactLegacy",
                url: "Contact.aspx",
                defaults: new { controller = "Home", action = "Contact" }
            );

            routes.MapRoute(
                name: "Sponsors",
                url: "sponsors",
                defaults: new { controller = "Home", action = "Sponsors" }
            );

            routes.MapRoute(
                name: "Search",
                url: "search",
                defaults: new { controller = "Home", action = "Search" }
            );

            routes.MapRoute(
                name: "StorySubmissionGuidelines",
                url: "submit-wtf/story-guidelines",
                defaults: new { controller = "Home", action = "StorySubmissionGuidelines" }
            );

            routes.MapRoute(
                name: "ViewLegacyArticle",
                url: "articles/{articleSlug}.aspx",
                defaults: new { controller = "Articles", action = "ViewLegacyArticle" }
            );

            routes.MapRoute(
                name: "ViewRandomArticle",
                url: "articles/random",
                defaults: new { controller = "Articles", action = "RandomArticle" }
            );

            routes.MapRoute(
                name: "SubmitWtf",
                url: "submit-wtf",
                defaults: new { controller = "Home", action = "Submit" }
            );

            routes.MapRoute(
                name: "ViewLegacyArticleComments",
                url: "comments/{articleSlug}.aspx",
                defaults: new { controller = "Articles", action = "ViewLegacyArticleComments" }
            );

            routes.MapRoute(
                name: "ViewArticleComments",
                url: "articles/comments/{articleSlug}/{page}",
                defaults: new { controller = "Articles", action = "ViewArticleComments", page = 1 }
            );

            routes.MapRoute(
                name: "CommentsLogin",
                url: "login",
                defaults: new { controller = "Articles", action = "Login" }
            );

            routes.MapRoute(
                name: "CommentsLoginGoogle",
                url: "login/google",
                defaults: new { controller = "Articles", action = "LoginGoogle" }
            );

            routes.MapRoute(
                name: "CommentsLoginGitHub",
                url: "login/github",
                defaults: new { controller = "Articles", action = "LoginGitHub" }
            );

            routes.MapRoute(
                name: "CommentsAddendum",
                url: "articles/comments/{articleSlug}/addendum/{id}",
                defaults: new { controller = "Articles", action = "Addendum" }
            );

            routes.MapRoute(
                name: "ViewArticle",
                url: "articles/{articleSlug}",
                defaults: new { controller = "Articles", action = "ViewArticle" }
            );

            routes.MapRoute(
                name: "ViewArticlesByMonth",
                url: "articles/{year}/{month}",
                defaults: new { controller = "Articles", action = "ViewArticlesByMonth" }
            );

            routes.MapRoute(
                name: "ViewLegacySeries",
                url: "series/{legacySeries}.aspx",
                defaults: new { controller = "Articles", action = "ViewLegacySeries" }
            );

            routes.MapRoute(
                name: "ViewArticlesBySeries",
                url: "series/{seriesSlug}",
                defaults: new { controller = "Articles", action = "ViewArticlesBySeries" }
            );

            routes.MapRoute(
                name: "ViewArticlesBySeriesAndMonth",
                url: "series/{year}/{month}/{seriesSlug}",
                defaults: new { controller = "Articles", action = "ViewArticlesBySeriesAndMonth" }
            );

            routes.MapRoute(
                name: "ViewAuthor",
                url: "authors/{authorSlug}",
                defaults: new { controller = "Authors", action = "ViewAuthor" }
            );

            // API Routes

            routes.MapRoute(
                name: "ApiDocumentation",
                url: "api",
                defaults: new { controller = "Api", action = "ViewApiDocumentation" }
             );

            routes.MapRoute(
                name: "ApiViewArticleById",
                url: "api/articles/id/{id}",
                defaults: new { controller = "Api", action = "ViewArticleById" }
            );

            routes.MapRoute(
                name: "ApiViewArticleByIdFilterBody",
                url: "api/articles/id/{id}/{onlyBodyAndAdHtml}",
                defaults: new { controller = "Api", action = "ViewArticleById" }
            );

            routes.MapRoute(
                name: "ApiViewArticleByArticleSlug",
                url: "api/articles/slug/{articleSlug}",
                defaults: new { controller = "Api", action = "ViewArticleBySlug" }
            );

            routes.MapRoute(
                name: "ApiViewArticleByArticleSlugFilterBody",
                url: "api/articles/slug/{articleSlug}/{onlyBodyAndAdHtml}",
                defaults: new { controller = "Api", action = "ViewArticleBySlug" }
            );

            routes.MapRoute(
                name: "ApiViewRandomArticle",
                url: "api/articles/random/",
                defaults: new { controller = "Api", action = "ViewRandomArticle" }
            );

            routes.MapRoute(
                name: "ApiViewRecentArticles",
                url: "api/articles/recent/",
                defaults: new { controller = "Api", action = "ViewRecentArticlesByCount" }
            );

            routes.MapRoute(
                name: "ApiViewRecentArticlesByCount",
                url: "api/articles/recent/{count}/",
                defaults: new { controller = "Api", action = "ViewRecentArticlesByCount" }
            );

            routes.MapRoute(
                name: "ApiViewArticlesByDate",
                url: "api/articles/{year}/{month}/",
                defaults: new { controller = "Api", action = "ViewArticlesByDate" }
            );

            routes.MapRoute(
                name: "ApiViewArticlesBySeries",
                url: "api/series/{slug}/",
                defaults: new { controller = "Api", action = "ViewRecentArticlesBySeriesAndCount" }
            );

            routes.MapRoute(
                name: "ApiViewArticlesBySeriesAndCount",
                url: "api/series/{slug}/{count}",
                defaults: new { controller = "Api", action = "ViewRecentArticlesBySeriesAndCount" }
            );

            routes.MapRoute(
                name: "ApiViewArticlesBySeriesAndDate",
                url: "api/series/{slug}/{year}/{month}/",
                defaults: new { controller = "Api", action = "ViewArticlesBySeriesAndDate" }
            );

            routes.MapRoute(
                name: "ApiViewArticlesByAuthor",
                url: "api/author/{slug}/",
                defaults: new { controller = "Api", action = "ViewRecentArticlesByAuthorAndCount" }
            );

            routes.MapRoute(
                name: "ApiViewArticlesByAuthorAndCount",
                url: "api/author/{slug}/{count}/",
                defaults: new { controller = "Api", action = "ViewRecentArticlesByAuthorAndCount" }
            );

            routes.MapRoute(
                name: "ViewAd",
                url: "fblast/{id}",
                defaults: new { controller = "Ads", action = "ViewAd" }
            );

            routes.MapRoute(
                name: "ClickAd",
                url: "fbuster/{redirectGuid}",
                defaults: new { controller = "Ads", action = "ClickAd" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }

        private static void RegisterLegacyForumRoutes(RouteCollection routes)
        {
            routes.MapRoute(
                name: "LegacyForumPost1",
                url: "forums/{ignore}/{postId}/ShowThread.aspx",
                constraints: new { ignore = @"\d+", postId = @"\d+" },
                defaults: new { controller = "Articles", action = "ViewLegacyPost" }
            );

            routes.MapRoute(
                name: "LegacyForumPost2",
                url: "forums/{postId}/ShowPost.aspx",
                constraints: new { postId = @"\d+" },
                defaults: new { controller = "Articles", action = "ViewLegacyPost" }
            );

            routes.MapRoute(
                name: "LegacyForumPost3",
                url: "forums/thread/{postId}.aspx",
                constraints: new { postId = @"\d+" },
                defaults: new { controller = "Articles", action = "ViewLegacyPost" }
            );

            routes.MapRoute(
                name: "LegacyForumPost4",
                url: "forums/thread/{ignore}/{postId}.aspx",
                constraints: new { ignore = @"\d+", postId = @"\d+" },
                defaults: new { controller = "Articles", action = "ViewLegacyPost" }
            );

            routes.MapRoute(
                name: "LegacyForumPost5",
                url: "forums/post/{postId}.aspx",
                constraints: new { postId = @"\d+" },
                defaults: new { controller = "Articles", action = "ViewLegacyPost" }
            );

            routes.MapRoute(
                name: "LegacyForumPost6",
                url: "forums/{postId}/PrintPost.aspx",
                constraints: new { postId = @"\d+" },
                defaults: new { controller = "Articles", action = "ViewLegacyPost" }
            );

            routes.MapRoute(
                name: "LegacyForumPost7",
                url: "forums/{postId}/ShowThread.aspx",
                constraints: new { postId = @"\d+" },
                defaults: new { controller = "Articles", action = "ViewLegacyPost" }
            );

            routes.MapRoute(
                name: "LegacyForumPost8",
                url: "forums/permalink/{ignore}/{postId}/ShowThread.aspx",
                constraints: new { ignore = @"\d+", postId = @"\d+" },
                defaults: new { controller = "Articles", action = "ViewLegacyPost" }
            );

            routes.MapRoute(
                name: "LegacyForumPost9",
                url: "forums/AddPost.aspx",
                defaults: new { controller = "Articles", action = "ViewLegacyPost" }
            );

            routes.MapRoute(
                name: "LegacyForumPost10",
                url: "forums/ShowPost.aspx",
                defaults: new { controller = "Articles", action = "ViewLegacyPost" }
            );

            routes.MapRoute(
                name: "LegacyForumPost11",
                url: "ShowPost.aspx",
                defaults: new { controller = "Articles", action = "ViewLegacyPost" }
            );

            routes.MapRoute(
                name: "LegacyForumPost12",
                url: "forums/{postId}/PostAttachment.aspx",
                constraints: new { postId = @"\d+" },
                defaults: new { controller = "Articles", action = "ViewLegacyPost" }
            );
        }
    }
}