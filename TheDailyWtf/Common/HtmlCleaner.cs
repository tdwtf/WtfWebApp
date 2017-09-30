using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TheDailyWtf.Common.HtmlCleaner;
using atom = TheDailyWtf.Common.Html.Atom;

namespace TheDailyWtf
{
    public static class HtmlCleaner
    {
        private static readonly Common.HtmlCleaner.Config Config = (new Common.HtmlCleaner.Config
        {
            ValidateURL = Cleaner.SafeURLScheme,
            EscapeComments = true, // work around https://github.com/psychobunny/templates.js/issues/54
            WrapText = false, // https://what.thedailywtf.com/post/1049805
        }).
            GlobalAttrAtom(atom.Title).
            ElemAttrAtom(atom.A, atom.Href).
            ElemAttrAtomMatch(atom.A, atom.Rel, new Regex(@"^nofollow$", RegexOptions.Compiled)).
            ElemAttrAtom(atom.Img, atom.Src, atom.Alt, atom.Width, atom.Height).
            //ElemAttrAtomMatch(atom.Img, atom.Class, new Regex(@"^((emoji|img-markdown|img-responsive)(\s+|\s*$))*$")).
            ElemAttrAtom(atom.Video, atom.Src, atom.Poster, atom.Controls).
            ElemAttrAtom(atom.Audio, atom.Src, atom.Controls).
            ElemAtom(atom.B, atom.I, atom.U, atom.S).
            ElemAtom(atom.Em, atom.Strong, atom.Strike).
            ElemAtom(atom.Big, atom.Small, atom.Sup, atom.Sub).
            ElemAtom(atom.Ins, atom.Del).
            ElemAtom(atom.Abbr, atom.Address, atom.Cite, atom.Q).
            ElemAtom(atom.P, atom.Blockquote).
            ElemAtom(atom.Pre, atom.Code).
            //ElemAttrAtomMatch(atom.Pre, atom.Class, new Regex(@"^((markdown-highlight)(\s+|\s*$))*")).
            //ElemAttrAtomMatch(atom.Code, atom.Class, new Regex(@"^((hljs|language-[a-z0-9]+)(\s+|\s*$))*$")).
            ElemAtom(atom.Kbd, atom.Tt).
            ElemAttrAtom(atom.Details, atom.Open).
            ElemAtom(atom.Summary).
            ElemAtom(atom.H1, atom.H2, atom.H3, atom.H4, atom.H5, atom.H6).
            ElemAttrAtom(atom.Ul, atom.Start).
            ElemAttrAtom(atom.Ol, atom.Start).
            ElemAttrAtom(atom.Li, atom.Value).
            ElemAtom(atom.Hr, atom.Br).
            ElemAtom(atom.Div, atom.Span).
            ElemAtom(atom.Table).
            //ElemAttrAtomMatch(atom.Table, atom.Class, new Regex(@"^((table|table-bordered|table-striped)(\s+|\s*$))*$")).
            ElemAtom(atom.Thead, atom.Tbody, atom.Tfoot).
            ElemAtom(atom.Tr, atom.Th, atom.Td).
            ElemAtom(atom.Caption).
            ElemAtom(atom.Dl, atom.Dt, atom.Dd);

        internal static IEnumerable<Common.Html.Node> Descendants(this Common.Html.Node[] roots, string tagName)
        {
            return roots.SelectMany(r => r.Descendants(tagName));
        }

        internal static IEnumerable<Common.Html.Node> Descendants(this Common.Html.Node root, string tagName)
        {
            if (root.Type != Common.Html.NodeType.Element)
            {
                yield break;
            }
            if (root.Data == tagName)
            {
                yield return root;
            }
            for (var c = root.FirstChild; c != null; c = c.NextSibling)
            {
                foreach (var d in c.Descendants(tagName))
                {
                    yield return d;
                }
            }
        }

        internal static string GetAttributeValue(this Common.Html.Node node, string key, string defaultValue)
        {
            foreach (var attr in node.Attr)
            {
                if (attr.Namespace == "" && attr.Key == key)
                {
                    return attr.Val;
                }
            }
            return defaultValue;
        }

        internal static void SetAttributeValue(this Common.Html.Node node, string key, string value)
        {
            foreach (var attr in node.Attr)
            {
                if (attr.Namespace == "" && attr.Key == key)
                {
                    attr.Val = value;
                    return;
                }
            }
            node.Attr.Add(new Common.Html.Attribute
            {
                Namespace = "",
                Key = key,
                Val = value,
            });
        }

        internal static string GetInnerText(this Common.Html.Node[] nodes)
        {
            return string.Join("", nodes.Select(n => n.GetInnerText()));
        }

        internal static string GetInnerText(this Common.Html.Node node)
        {
            switch (node.Type)
            {
                case Common.Html.NodeType.Text:
                    return node.Data;
                case Common.Html.NodeType.Element:
                    var buf = new StringBuilder();
                    for (var c = node.FirstChild; c != null; c = c.NextSibling)
                    {
                        buf.Append(c.GetInnerText());
                    }
                    return buf.ToString();
                default:
                    return "";
            }
        }

        public static string UnmixContent(string html)
        {
            if (html == null)
            {
                return null;
            }

            var doc = Cleaner.ParseDepth(html, 0);

            foreach (var node in doc.Descendants("img"))
            {
                var src = node.GetAttributeValue("src", "").TrimStart();
                if (src.StartsWith("http://thedailywtf.com/") || src.StartsWith("http://img.thedailywtf.com/"))
                {
                    node.SetAttributeValue("src", src.Substring("http:".Length));
                }
            }
            foreach (var node in doc.Descendants("a"))
            {
                var href = node.GetAttributeValue("href", "").TrimStart();
                if (href.StartsWith("http://thedailywtf.com/"))
                {
                    node.SetAttributeValue("href", href.Substring("http:".Length));
                }
            }
            foreach (var node in doc.Descendants("script"))
            {
                var src = node.GetAttributeValue("src", "").TrimStart();
                if (src.StartsWith("http://www.cornify.com/"))
                {
                    node.SetAttributeValue("src", src.Substring("http:".Length));
                }
            }

            return Cleaner.Render(doc);
        }

        public static string CloseTags(string html)
        {
            return Cleaner.Render(Cleaner.ParseDepth(html, 0));
        }

        public static string Clean(string html)
        {
            html = Cleaner.Preprocess(Config, html);
            var nodes = Cleaner.Parse(html);
            foreach (var img in nodes.Descendants("img"))
            {
                img.Data = "a";
                img.DataAtom = atom.A;
                var alt = new Common.Html.Node
                {
                    Type = Common.Html.NodeType.Text,
                    Data = "[image]",
                };
                img.AppendChild(alt);
                foreach (var attr in img.Attr)
                {
                    if (attr.Namespace != "")
                    {
                        continue;
                    }
                    if (attr.Key == "src")
                    {
                        attr.Key = "href";
                    }
                    if (attr.Key == "alt")
                    {
                        alt.Data = attr.Val;
                    }
                }
            }
            foreach (var a in nodes.Descendants("a"))
            {
                a.SetAttributeValue("rel", "nofollow");
            }
            nodes = Cleaner.CleanNodes(Config, nodes);
            return Cleaner.Render(nodes);
        }
    }
}