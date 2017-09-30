using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace TheDailyWtf.Common.HtmlCleaner
{
    public static class Cleaner
    {
        // DefaultMaxDepth is the default maximum depth of the node trees returned by
        // Parse.
        public const int DefaultMaxDepth = 100;

        // Preprocess escapes disallowed tags in a cleaner way, but does not fix
        // nesting problems. Use with Clean.
        public static string Preprocess(Config config, string fragment)
        {
            if (config == null)
            {
                config = Config.DefaultConfig;
            }

            var buf = new StringBuilder();
            var t = new Html.Tokenizer(new MemoryStream(Encoding.UTF8.GetBytes(fragment)));
            while (true)
            {
                var tok = t.Next();
                switch (tok)
                {
                    case Html.TokenType.Error:
                        try
                        {
                            t.ThrowErr();
                        }
                        catch (EndOfStreamException)
                        {
                            // The only possible errors are from the Reader or from
                            // the buffer capacity being exceeded. Neither can
                            // happen with strings.NewReader as the string must
                            // already fit into memory.
                            buf.Append(Html.EscapeString(Encoding.UTF8.GetString(t.Raw)));
                            return buf.ToString();
                        }
                        break;
                    case Html.TokenType.Text:
                        buf.Append(Encoding.UTF8.GetString(t.Raw));
                        break;
                    case Html.TokenType.StartTag:
                    case Html.TokenType.EndTag:
                    case Html.TokenType.SelfClosingTag:
                        var raw = Encoding.UTF8.GetString(t.Raw);
                        var (tagName, _) = t.TagName();
                        var allowed = false;
                        var tag = Html.Atom.Lookup(tagName);
                        if (tag != 0)
                        {
                            if (config.elem.ContainsKey(tag))
                            {
                                allowed = true;
                            }
                        }
                        if (!allowed)
                        {
                            if (config.elemCustom.ContainsKey(Encoding.UTF8.GetString(tagName)))
                            {
                                allowed = true;
                            }
                        }
                        if (!allowed)
                        {
                            raw = Html.EscapeString(raw);
                        }
                        buf.Append(raw);
                        break;
                    case Html.TokenType.Comment:
                        var raw1 = Encoding.UTF8.GetString(t.Raw);
                        if (config.EscapeComments || !raw1.StartsWith("<!--") || !raw1.EndsWith("-->"))
                        {
                            raw1 = Html.EscapeString(raw1);
                        }
                        buf.Append(raw1);
                        break;
                    default:
                        buf.Append(Html.EscapeString(Encoding.UTF8.GetString(t.Raw)));
                        break;
                }
            }
        }

        // Parse is a convenience wrapper that calls ParseDepth with DefaultMaxDepth.
        public static Html.Node[] Parse(string fragment)
        {
            return ParseDepth(fragment, DefaultMaxDepth);
        }

        // ParseDepth is a convenience function that wraps html.ParseFragment but takes
        // a string instead of an io.Reader and omits deep trees.
        public static Html.Node[] ParseDepth(string fragment, int maxDepth)
        {
            var nodes = Html.ParseFragment(new MemoryStream(Encoding.UTF8.GetBytes(fragment)), new Html.Node
            {
                Type = Html.NodeType.Element,
                Data = "div",
                DataAtom = Html.Atom.Div,
            });

            if (maxDepth > 0)
            {
                foreach (var n in nodes)
                {
                    forceMaxDepth(n, maxDepth);
                }
            }

            return nodes;
        }

        // Render is a convenience function that wraps html.Render and renders to a
        // string instead of an io.Writer.
        public static string Render(params Html.Node[] nodes)
        {
            using (var buf = new MemoryStream())
            {
                foreach (var n in nodes)
                {
                    Html.Render(buf, n);
                }

                return Encoding.UTF8.GetString(buf.ToArray());
            }
        }

        // Clean a fragment of HTML using the specified Config, or the DefaultConfig
        // if it is nil.
        public static string Clean(Config c, string fragment)
        {
            return Render(CleanNodes(c, Parse(fragment)));
        }

        private static readonly HashSet<Html.Atom.AtomType> isBlockElement = new HashSet<Html.Atom.AtomType>
        {
            0, // custom elements are not wrapped
            Html.Atom.Address,
            Html.Atom.Article,
            Html.Atom.Aside,
            Html.Atom.Blockquote,
            Html.Atom.Center,
            Html.Atom.Dd,
            Html.Atom.Details,
            Html.Atom.Dir,
            Html.Atom.Div,
            Html.Atom.Dl,
            Html.Atom.Dt,
            Html.Atom.Fieldset,
            Html.Atom.Figcaption,
            Html.Atom.Figure,
            Html.Atom.Footer,
            Html.Atom.Form,
            Html.Atom.H1,
            Html.Atom.H2,
            Html.Atom.H3,
            Html.Atom.H4,
            Html.Atom.H5,
            Html.Atom.H6,
            Html.Atom.Header,
            Html.Atom.Hgroup,
            Html.Atom.Hr,
            Html.Atom.Li,
            Html.Atom.Listing,
            Html.Atom.Menu,
            Html.Atom.Nav,
            Html.Atom.Ol,
            Html.Atom.P,
            Html.Atom.Plaintext,
            Html.Atom.Pre,
            Html.Atom.Section,
            Html.Atom.Summary,
            Html.Atom.Table,
            Html.Atom.Ul,
        };

        // CleanNodes calls CleanNode on each node, and additionally wraps inline
        // elements in <p> tags and wraps dangling <li> tags in <ul> tags.
        public static Html.Node[] CleanNodes(Config c, Html.Node[] nodes)
        {
            return cleanNodes(c, deepCopyAll(nodes));
        }

        private static Html.Node[] deepCopyAll(Html.Node[] nodes)
        {
            return nodes.Select(deepCopy).ToArray();
        }

        private static Html.Node deepCopy(Html.Node n)
        {
            var clone = new Html.Node
            {
                Type = n.Type,
                Attr = n.Attr,
                Namespace = n.Namespace,
                Data = n.Data,
                DataAtom = n.DataAtom,
            };
            for (var c = n.FirstChild; c != null; c = c.NextSibling)
            {
                clone.AppendChild(deepCopy(c));
            }
            return clone;
        }

        private static Html.Node[] cleanNodes(Config c, Html.Node[] nodes)
        {
            if (c == null)
            {
                c = Config.DefaultConfig;
            }

            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = filterNode(c, nodes[i]);
                if (nodes[i].DataAtom == Html.Atom.Li)
                {
                    var wrapper = new Html.Node
                    {
                        Type = Html.NodeType.Element,
                        Data = "ul",
                        DataAtom = Html.Atom.Ul,
                    };
                    wrapper.AppendChild(nodes[i]);
                    nodes[i] = wrapper;
                }
            }

            if (c.WrapText)
            {
                nodes = wrapText(nodes);
            }

            return nodes;
        }

        private static Html.Node[] wrapText(Html.Node[] nodes)
        {
            var wrapped = new List<Html.Node>(nodes.Length);
            Html.Node wrapper = null;
            void appendWrapper()
            {
                if (wrapper != null)
                {
                    // render and re-parse so p-inline-p expands
                    wrapped.AddRange(ParseDepth(Render(wrapper), 0));
                    wrapper = null;
                }
            }
            foreach (var n in nodes)
            {
                if (n.Type == Html.NodeType.Element && isBlockElement.Contains(n.DataAtom))
                {
                    appendWrapper();
                    wrapped.Add(n);
                    continue;
                }
                if (wrapper == null && n.Type == Html.NodeType.Text && n.Data.Trim() == "")
                {
                    wrapped.Add(n);
                    continue;
                }
                if (wrapper == null)
                {
                    wrapper = new Html.Node
                    {
                        Type = Html.NodeType.Element,
                        Data = "p",
                        DataAtom = Html.Atom.P,
                    };
                }

                wrapper.AppendChild(n);
            }
            appendWrapper();
            return wrapped.ToArray();
        }

        private static Html.Node text(string s)
        {
            return new Html.Node { Type = Html.NodeType.Text, Data = s };
        }

        // CleanNode cleans an HTML node using the specified config. Text nodes are
        // returned as-is. Element nodes are recursively  checked for legality and have
        // their attributes checked for legality as well. Elements with illegal
        // attributes are copied and the problematic attributes are removed. Elements
        // that are not in the set of legal elements are replaced with a textual
        // version of their source code.
        public static Html.Node CleanNode(Config c, Html.Node n)
        {
            if (c == null)
            {
                c = Config.DefaultConfig;
            }
            return filterNode(c, deepCopy(n));
        }

        private static Html.Node filterNode(Config c, Html.Node n)
        {
            if (n.Type == Html.NodeType.Text)
            {
                return n;
            }
            if (n.Type == Html.NodeType.Comment && !c.EscapeComments)
            {
                return n;
            }
            if (n.Type != Html.NodeType.Element)
            {
                return text(Render(n));
            }
            return cleanNode(c, n);
        }

        private static Html.Node cleanNode(Config c, Html.Node n)
        {
            var ok1 = c.elem.TryGetValue(n.DataAtom, out var allowedAttr);
            var ok2 = c.elemCustom.TryGetValue(n.Data, out var customAttr);
            if (ok1 || ok2)
            {
                cleanChildren(c, n);

                var haveSrc = false;

                var attrs = n.Attr.ToArray();
                n.Attr.Clear();
                foreach (var attr in attrs)
                {
                    var a = Html.Atom.Lookup(Encoding.UTF8.GetBytes(attr.Key));

                    Regex re1 = null, re2 = null;
                    ok1 = allowedAttr?.TryGetValue(a, out re1) ?? false;
                    ok2 = customAttr?.TryGetValue(attr.Key, out re2) ?? false;
                    var ok3 = c.attr.Contains(a);
                    var ok4 = c.attrCustom.Contains(attr.Key);

                    if (attr.Namespace != "" || (!ok1 && !ok2 && !ok3 && !ok4))
                    {
                        continue;
                    }

                    if (!cleanURL(c, a, attr))
                    {
                        continue;
                    }

                    if (re1 != null && !re1.IsMatch(attr.Val))
                    {
                        continue;
                    }
                    if (re2 != null && !re2.IsMatch(attr.Val))
                    {
                        continue;
                    }

                    haveSrc = haveSrc || a == Html.Atom.Src;

                    n.Attr.Add(attr);
                }

                if (n.DataAtom == Html.Atom.Img && !haveSrc)
                {
                    // replace it with an empty text node
                    return text("");
                }

                return n;
            }
            return text(Html.UnescapeString(Render(n)));
        }

        private static HashSet<string> allowedURLSchemes = new HashSet<string>
        {
            Uri.UriSchemeHttp,
            Uri.UriSchemeHttps,
            Uri.UriSchemeMailto,
            "data",
        };

        // SafeURLScheme returns true if u.Scheme is http, https, mailto, data, or an
        // empty string.
        public static bool SafeURLScheme(Uri u)
        {
            return !u.IsAbsoluteUri || allowedURLSchemes.Contains(u.Scheme);
        }

        private static bool cleanURL(Config c, Html.Atom.AtomType a, Html.Attribute attr)
        {
            if (a != Html.Atom.Href && a != Html.Atom.Src && a != Html.Atom.Poster)
            {
                return true;
            }

            if (!Uri.TryCreate(attr.Val, UriKind.RelativeOrAbsolute, out var u))
            {
                return false;
            }
            if (c.ValidateURL != null && !c.ValidateURL(u))
            {
                return false;
            }
            attr.Val = u.ToString();
            return true;
        }

        private static void cleanChildren(Config c, Html.Node parent)
        {
            var children = new List<Html.Node>();
            while (parent.FirstChild != null)
            {
                var child = parent.FirstChild;
                parent.RemoveChild(child);
                children.Add(filterNode(c, child));
            }

            if (c.WrapText)
            {
                var ok = c.wrap.Contains(parent.DataAtom);
                if (!ok && parent.DataAtom == 0)
                {
                    ok = c.wrapCustom.Contains(parent.Data);
                }
                if (ok)
                {
                    var wrapped = wrapText(children.ToArray());
                    children.Clear();
                    children.AddRange(wrapped);
                }
            }

            foreach (var child in children)
            {
                parent.AppendChild(child);
            }
        }

        private static void forceMaxDepth(Html.Node n, int depth)
        {
            if (depth == 0)
            {
                n.Type = Html.NodeType.Text;
                n.FirstChild = null;
                n.LastChild = null;
                n.Attr.Clear();
                n.DataAtom = 0;
                n.Data = "[omitted]";
                while (n.NextSibling != null)
                {
                    n.Parent.RemoveChild(n.NextSibling);
                }
                return;
            }

            if (n.Type != Html.NodeType.Element)
            {
                return;
            }

            for (var c = n.FirstChild; c != null; c = c.NextSibling)
            {
                forceMaxDepth(c, depth - 1);
            }
        }
    }
}