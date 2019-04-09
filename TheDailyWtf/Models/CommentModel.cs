using CommonMark;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TheDailyWtf.Data;

namespace TheDailyWtf.Models
{
    public class CommentModel
    {
        private static readonly Regex ImgSrcRegex = new Regex(@"src=""(?<comment>[^""]+)""", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

        private readonly Lazy<IList<string>> getLinks;

        public CommentModel()
        {
            this.getLinks = new Lazy<IList<string>>(() => HtmlCleaner.GetLinkUrls(this.BodyHtml).ToList());
        }

        public int Id { get; set; }
        public int Index { get; set; }
        public int ArticleId { get; set; }
        public string ArticleTitle { get; set; }
        public string BodyRaw { get; set; }
        public string BodyHtml { get; set; }
        public string Username { get; set;}
        public DateTime PublishedDate { get; set; }
        public int? DiscoursePostId { get; set; }
        public bool Featured { get; set; }
        public bool Anonymous { get { return UserToken == null; } }
        public bool Hidden { get; set; }
        public int? ParentCommentId { get; set; }
        public int? ParentCommentIndex { get; set; }
        public string ParentCommentUsername { get; set; }
        [NonSerialized]
        public string UserIP;
        [NonSerialized]
        public string UserToken;
        public string TokenType => UserToken?.Split(':')[0];
        public string ProfileUrl
        {
            get
            {
                if (UserToken == null)
                {
                    return null;
                }

                var split = UserToken.Split(':');
                return split[0] == "author" ? $"https://{Config.Wtf.Host}/authors/{split[1]}" :
                    split[0] == "nodebb" ? $"https://{Config.NodeBB.Host}/user/{split[1]}" :
                    split[0] == "disco" ? $"https://{Config.NodeBB.Host}/api/tdwtf-disco-user-redirect/{split[1]}" :
                    split[0] == "cs" ? $"https://{Config.NodeBB.Host}/user/Profile.aspx?UserID={split[1]}" :
                    null;
            }
        }
        public IList<string> Links => this.getLinks.Value;

        public static IList<CommentModel> GetFeaturedCommentsForArticle(ArticleModel article)
        {
            var comments = DB.Articles_GetFeaturedComments(article.Id);
            return comments.Select(c => FromTable(c)).ToList();
        }

        public static CommentModel GetCommentById(int id)
        {
            var comments = DB.Comments_GetCommentById(Comment_Id: id);
            return comments.Select(c => FromTable(c)).FirstOrDefault();
        }

        public static IList<CommentModel> FromArticle(ArticleModel article, int? offset = null, int? limit = null)
        {
            var comments = DB.Comments_GetComments(Article_Id: article.Id, Skip_Count: offset, Limit_Count: limit);
            return comments.Select(c => FromTable(c)).ToList();
        }

        public static IList<CommentModel> GetCommentsByIP(string ip, int? offset = null, int? limit = null)
        {
            var comments = DB.Comments_GetCommentsByIP(User_IP: ip, Skip_Count: offset, Limit_Count: limit);
            return comments.Select(c => FromTable(c)).ToList();
        }

        public static IList<CommentModel> GetCommentsByToken(string token, int? offset = null, int? limit = null)
        {
            var comments = DB.Comments_GetCommentsByToken(User_Token: token, Skip_Count: offset, Limit_Count: limit);
            return comments.Select(c => FromTable(c)).ToList();
        }

        public static IList<CommentModel> GetHiddenComments(string authorSlug = null, int? offset = null, int? limit = null)
        {
            var comments = DB.Comments_GetHiddenComments(Author_Slug: authorSlug, Skip_Count: offset, Limit_Count: limit);
            return comments.Select(c => FromTable(c)).ToList();
        }

        public static int CountCommentsByIP(string ip)
        {
            return DB.Comments_CountCommentsByIP(User_IP: ip).Value;
        }

        public static int CountCommentsByToken(string token)
        {
            return DB.Comments_CountCommentsByToken(User_Token: token).Value;
        }

        public static int CountHiddenComments(string authorSlug = null)
        {
            return DB.Comments_CountHiddenComments(Author_Slug: authorSlug).Value;
        }

        private static CommentModel FromTable(Tables.Comments_Extended comment)
        {
            return new CommentModel()
            {
                Id = comment.Comment_Id,
                ArticleId = comment.Article_Id,
                ArticleTitle = comment.Article_Title,
                BodyRaw = comment.Body_Html,
                BodyHtml = MarkdownFormatContent(comment.Body_Html),
                Username = comment.User_Name,
                DiscoursePostId = comment.Discourse_Post_Id,
                PublishedDate = comment.Posted_Date,
                Featured = comment.Featured_Indicator,
                Hidden = comment.Hidden_Indicator,
                ParentCommentId = comment.Parent_Comment_Id,
                ParentCommentUsername = comment.Parent_Comment_User_Name,
                UserIP = comment.User_IP,
                UserToken = comment.User_Token,
                Index = comment.Comment_Index,
                ParentCommentIndex = comment.Parent_Comment_Index
            };
        }

        private static string MarkdownFormatContent(string text)
        {
            return HtmlCleaner.Clean(CommonMarkConverter.Convert(BbCodeFormatComment(text)));
        }

        private static string BbCodeFormatComment(string text)
        {
            string encodedString = text;

            // Bold, Italic, Underline
            encodedString = Regexes.Bold.Replace(encodedString, "<b>$1</b>");
            encodedString = Regexes.Italic.Replace(encodedString, "<i>$1</i>");
            encodedString = Regexes.Underline.Replace(encodedString, "<u>$1</u>");

            // Quote
            if (Regexes.QuoteEndBbCode.Matches(encodedString).Count ==
                Regexes.QuoteStartBbCode.Matches(encodedString).Count + Regexes.EmptyQuoteStartBbCode.Matches(encodedString).Count)
            {
                encodedString = Regexes.QuoteStartBbCode.Replace(encodedString, "<BLOCKQUOTE class=\"Quote\"><div><i class=\"icon-quote\"></i> <strong>$1:</strong></div><div>");
                encodedString = Regexes.QuoteEndBbCode.Replace(encodedString, "</div></BLOCKQUOTE>");

                encodedString = Regexes.EmptyQuoteStartBbCode.Replace(encodedString, "<BLOCKQUOTE class=\"Quote\"><div>");
                encodedString = Regexes.EmptyQuoteEndBbCode.Replace(encodedString, "</div></BLOCKQUOTE>");
            };

            // Code
            encodedString = Regexes.Code.Replace(encodedString, "<pre>$1</pre>");

            // Anchors
            encodedString = Regexes.Url1.Replace(encodedString, "<a rel=\"nofollow\" href=\"http://www.$1\" target=\"_blank\" title=\"$1\">$1</a>");
            encodedString = Regexes.Url2.Replace(encodedString, "<a rel=\"nofollow\" href=\"$1\" target=\"_blank\" title=\"$1\">$1</a>");
            encodedString = Regexes.Url3.Replace(encodedString, "<a rel=\"nofollow\" href=\"$1\" target=\"_blank\" title=\"$1\">$3</a>");
            encodedString = Regexes.Url4.Replace(encodedString, "<a rel=\"nofollow\" href=\"$1\" target=\"_blank\" title=\"$1\">$3</a>");
            encodedString = Regexes.Link1.Replace(encodedString, "<a rel=\"nofollow\" href=\"$1\" target=\"_blank\" title=\"$1\">$1</a>");
            encodedString = Regexes.Link2.Replace(encodedString, "<a rel=\"nofollow\" href=\"$1\" target=\"_blank\" title=\"$1\">$3</a>");

            // Image
            encodedString = Regexes.Img1.Replace(encodedString, "<img src=\"$1\" border=\"0\" />");
            encodedString = Regexes.Img2.Replace(encodedString, "<img width=\"$1\" height=\"$3\" src=\"$5\" border=\"0\" />");

            // Color
            encodedString = Regexes.Color.Replace(encodedString, "<span style=\"color:$1;\">$3</span>");

            return encodedString;
        }

        private static class Regexes
        {
            private static readonly RegexOptions Options = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

            public static readonly Regex Bold = new Regex(@"\[b(?:\s*)\]((.|\n)*?)\[/b(?:\s*)\]", Regexes.Options);
            public static readonly Regex Italic = new Regex(@"\[i(?:\s*)\]((.|\n)*?)\[/i(?:\s*)\]", Regexes.Options);
            public static readonly Regex Underline = new Regex(@"\[u(?:\s*)\]((.|\n)*?)\[/u(?:\s*)\]", Regexes.Options);
            public static readonly Regex Code = new Regex(@"\[code(?:\s*)\]((.|\n)*?)\[/code(?:\s*)\]", Regexes.Options);
            public static readonly Regex Url1 = new Regex(@"\[url(?:\s*)\]www\.(.*?)\[/url(?:\s*)\]", Regexes.Options);
            public static readonly Regex Url2 = new Regex(@"\[url(?:\s*)\]((.|\n)*?)\[/url(?:\s*)\]", Regexes.Options);
            public static readonly Regex Url3 = new Regex(@"\[url=(?:""|&quot;|&#34;)((.|\n)*?)(?:\s*)(?:""|&quot;|&#34;)\]((.|\n)*?)\[/url(?:\s*)\]", Regexes.Options);
            public static readonly Regex Url4 = new Regex(@"\[url=((.|\n)*?)(?:\s*)\]((.|\n)*?)\[/url(?:\s*)\]", Regexes.Options);
            public static readonly Regex Link1 = new Regex(@"\[link(?:\s*)\]((.|\n)*?)\[/link(?:\s*)\]", Regexes.Options);
            public static readonly Regex Link2 = new Regex(@"\[link=((.|\n)*?)(?:\s*)\]((.|\n)*?)\[/link(?:\s*)\]", Regexes.Options);
            public static readonly Regex Img1 = new Regex(@"\[img(?:\s*)\]((.|\n)*?)\[/img(?:\s*)\]", Regexes.Options);
            public static readonly Regex Img2 = new Regex(@"\[img=((.|\n)*?)x((.|\n)*?)(?:\s*)\]((.|\n)*?)\[/img(?:\s*)\]", Regexes.Options);
            public static readonly Regex Color = new Regex(@"\[color=((.|\n)*?)(?:\s*)\]((.|\n)*?)\[/color(?:\s*)\]", Regexes.Options);

            public static readonly Regex QuoteStartBbCode = new Regex("\\[quote(?:\\s*)(?:user)?=(?:\"|&quot;|&#34;)(.*?)(?:,.*?)?(?:\"|&quot;|&#34;)\\]", Regexes.Options);
            public static readonly Regex QuoteEndBbCode = new Regex("\\[/quote(\\s*)\\]", Regexes.Options);

            public static readonly Regex EmptyQuoteStartBbCode = new Regex("\\[quote(\\s*)\\]", Regexes.Options);
            public static readonly Regex EmptyQuoteEndBbCode = new Regex("\\[/quote(\\s*)\\]", Regexes.Options);
        }
    }
}