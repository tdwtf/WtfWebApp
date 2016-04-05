using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using TheDailyWtf.Models;

namespace TheDailyWtf
{
    public sealed class RssArticlesResult : ActionResult
    {
        private readonly IEnumerable<ArticleModel> articles;

        public RssArticlesResult(IEnumerable<ArticleModel> articles)
        {
            if (articles == null)
                throw new ArgumentNullException("articles");

            this.articles = articles;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            var response = context.HttpContext.Response;
            response.ContentEncoding = Encoding.UTF8;
            response.ContentType = "application/rss+xml";

            var dc = XNamespace.Get("http://purl.org/dc/elements/1.1/");
            var slash = XNamespace.Get("http://purl.org/rss/1.0/modules/slash/");
            var wfw = XNamespace.Get("http://wellformedweb.org/CommentAPI/");

            var xdoc = new XDocument(
                new XElement("rss",
                    new XAttribute("version", "2.0"),
                    new XAttribute(XNamespace.Xmlns + "dc", dc),
                    new XAttribute(XNamespace.Xmlns + "slash", slash),
                    new XAttribute(XNamespace.Xmlns + "wfw", wfw),
                    new XElement("channel",
                        new XElement("title", "The Daily WTF"),
                        new XElement("link", "http://thedailywtf.com/"),
                        new XElement("description", "Curious Perversions in Information Technology"),
                        new XElement("lastBuildDate", DateTime.UtcNow.ToString("r")),
                        this.articles.Select(a => new XElement("item",
                            new XElement(dc + "creator", a.Author.Name),
                            new XElement("title", a.RssTitle),
                            new XElement("link", a.Url),
                            new XElement("category", a.Series.Title),
                            new XElement("pubDate", a.PublishedDate.Value.ToUniversalTime().ToString("r")),
                            new XElement("guid", a.Url),
                            new XElement("description", a.BodyAndAdHtml),
                            new XElement(slash + "comments", a.CachedCommentCount),
                            new XElement(wfw + "comment", a.CommentsUrl)
                        ))
                    )
                )
            );

            using (var writer = XmlWriter.Create(response.OutputStream, new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = false }))
            {
                xdoc.WriteTo(writer);
            }
        }
    }
}