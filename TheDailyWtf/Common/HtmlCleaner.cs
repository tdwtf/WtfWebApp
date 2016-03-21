using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace TheDailyWtf
{
    public static class HtmlCleaner
    {
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
            "a", "img", "video", "audio",
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

        private static HtmlNode CleanNode(HtmlNode node, int maxDepth)
        {
            if (maxDepth == 0)
            {
                return node.OwnerDocument.CreateTextNode("[omitted]");
            }

            if (node.NodeType == HtmlNodeType.Comment)
            {
                return node.OwnerDocument.CreateTextNode(node.OuterHtml);
            }

            if (node.NodeType == HtmlNodeType.Text)
            {
                return node;
            }

            if (!allowedElements.Contains(node.Name.ToLowerInvariant()))
            {
                return node.OwnerDocument.CreateTextNode(node.OuterHtml);
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

                if (a.Name.Equals("src", StringComparison.InvariantCultureIgnoreCase) && (
                    node.Name.Equals("img", StringComparison.InvariantCultureIgnoreCase) ||
                    node.Name.Equals("video", StringComparison.InvariantCultureIgnoreCase) ||
                    node.Name.Equals("audio", StringComparison.InvariantCultureIgnoreCase)))
                {
                    if (AllowedUri(a.Value))
                    {
                        continue;
                    }
                }

                if (a.Name.Equals("controls", StringComparison.InvariantCultureIgnoreCase) && (
                    node.Name.Equals("video", StringComparison.InvariantCultureIgnoreCase) ||
                    node.Name.Equals("audio", StringComparison.InvariantCultureIgnoreCase)))
                {
                    continue;
                }

                if (a.Name.Equals("poster", StringComparison.InvariantCultureIgnoreCase) &&
                    node.Name.Equals("video", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                if (a.Name.Equals("alt", StringComparison.InvariantCultureIgnoreCase) &&
                    node.Name.Equals("img", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                toRemove.Add(a);
            }

            foreach (var a in toRemove)
            {
                node.Attributes.Remove(a);
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