using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace TheDailyWtf
{
    public static class HtmlCleaner
    {
        public static string UnmixContent(string html)
        {
            if (html == null)
            {
                return null;
            }

            var doc = new HtmlDocument();
            doc.DocumentNode.InnerHtml = html;

            foreach (var node in doc.DocumentNode.Descendants("img"))
            {
                var src = node.GetAttributeValue("src", "").TrimStart();
                if (src.StartsWith("http://thedailywtf.com/") || src.StartsWith("http://img.thedailywtf.com/"))
                {
                    node.SetAttributeValue("src", src.Substring("http:".Length));
                }
            }
            foreach (var node in doc.DocumentNode.Descendants("a"))
            {
                var href = node.GetAttributeValue("href", "").TrimStart();
                if (href.StartsWith("http://thedailywtf.com/"))
                {
                    node.SetAttributeValue("href", href.Substring("http:".Length));
                }
            }
            foreach (var node in doc.DocumentNode.Descendants("script"))
            {
                var src = node.GetAttributeValue("src", "").TrimStart();
                if (src.StartsWith("http://www.cornify.com/"))
                {
                    node.SetAttributeValue("src", src.Substring("http:".Length));
                }
            }

            return doc.DocumentNode.InnerHtml;
        }

        public static string CloseTags(string html)
        {
            var doc = new HtmlDocument();
            doc.DocumentNode.InnerHtml = html;

            return doc.DocumentNode.InnerHtml;
        }

        public static string Clean(string html)
        {
            var doc = new HtmlDocument();
            doc.DocumentNode.InnerHtml = html;

            for (int i = 0; i < doc.DocumentNode.ChildNodes.Count; i++)
            {
                doc.DocumentNode.ChildNodes.Replace(i, CleanNode(doc.DocumentNode.ChildNodes[i], 100));
            }

            return doc.DocumentNode.InnerHtml;
        }

        private static HashSet<string> allowedElements = new HashSet<string>()
        {
            "a",
            "b", "i", "u", "s",
            "em", "strong", "strike",
            "big", "small", "sup", "sub",
            "ins", "del",
            "abbr", "address", "cite", "q",
            "p", "blockquote",
            "pre", "code", "kbd", "tt",
            "details", "summary",
            "h1", "h2", "h3", "h4", "h5", "h6",
            "ul", "ol", "li",
            "hr", "br",
            "div", "table",
            "thead", "tbody", "tfoot",
            "tr", "th", "td",
            "caption"
        };

        private static HtmlTextNode HtmlRealEscapeString(this HtmlDocument doc, string text)
        {
            return doc.CreateTextNode(text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;"));
        }

        private static HtmlNode CleanNode(HtmlNode node, int maxDepth)
        {
            if (maxDepth == 0)
            {
                return node.OwnerDocument.HtmlRealEscapeString("[omitted]");
            }

            if (node.NodeType == HtmlNodeType.Comment)
            {
                return node.OwnerDocument.HtmlRealEscapeString(node.OuterHtml);
            }

            if (node.NodeType == HtmlNodeType.Text)
            {
                return node;
            }

            if (node.Name.Equals("img", StringComparison.InvariantCultureIgnoreCase))
            {
                var src = node.Attributes["src"]?.Value;
                var alt = node.Attributes["alt"]?.Value ?? "[image]";
                node.Name = "a";
                node.Attributes.Remove("src");
                node.Attributes.Remove("alt");
                node.Attributes.Add("href", src);
                node.RemoveAllChildren();
                node.AppendChild(node.OwnerDocument.HtmlRealEscapeString(alt));
            }

            if (!allowedElements.Contains(node.Name.ToLowerInvariant()))
            {
                string html;
                try
                {
                    html = node.OuterHtml;
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    // "Use HtmlAgilityPack", they said. "It's the best HTML parser around", they said.
                    if (ex.ParamName != "length" || ex.TargetSite.Name != "Substring")
                    {
                        throw;
                    }
                    html = "<" + node.OriginalName + ">";
                }
                return node.OwnerDocument.HtmlRealEscapeString(html);
            }

            var toRemove = new List<HtmlAttribute>();
            foreach (var a in node.Attributes)
            {
                if (a.Name.Equals("title", StringComparison.InvariantCultureIgnoreCase))
                {
                    // title is always allowed
                    continue;
                }

                if (a.Name.Equals("value", StringComparison.InvariantCultureIgnoreCase) &&
                    node.Name.Equals("li", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                if (a.Name.Equals("start", StringComparison.InvariantCultureIgnoreCase) && (
                    node.Name.Equals("ul", StringComparison.InvariantCultureIgnoreCase) ||
                    node.Name.Equals("ol", StringComparison.InvariantCultureIgnoreCase)))
                {
                    continue;
                }

                if (a.Name.Equals("href", StringComparison.InvariantCultureIgnoreCase) &&
                    node.Name.Equals("a", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (AllowedUri(a.Value))
                    {
                        continue;
                    }
                }

                toRemove.Add(a);
            }

            foreach (var a in toRemove)
            {
                node.Attributes.Remove(a);
            }

            if (node.Name.Equals("a", StringComparison.InvariantCultureIgnoreCase))
            {
                node.Attributes.Add("rel", "nofollow");
            }

            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                node.ChildNodes.Replace(i, CleanNode(node.ChildNodes[i], maxDepth - 1));
            }

            return node;
        }

        private static bool AllowedUri(string href)
        {
            try
            {
                var uri = new Uri(href);
                return !uri.IsAbsoluteUri || uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeMailto;
            }
            catch (UriFormatException)
            {
                return false;
            }
        }
    }
}