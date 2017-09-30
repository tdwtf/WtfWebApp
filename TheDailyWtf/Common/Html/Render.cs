using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// See README.txt for a link to the original source code.

namespace TheDailyWtf.Common
{
    public static partial class Html
    {
        private static void WriteString(this Stream w, string s)
        {
            var b = Encoding.UTF8.GetBytes(s);
            w.Write(b, 0, b.Length);
        }

        // Render renders the parse tree n to the given writer.
        //
        // Rendering is done on a 'best effort' basis: calling Parse on the output of
        // Render will always result in something similar to the original tree, but it
        // is not necessarily an exact clone unless the original tree was 'well-formed'.
        // 'Well-formed' is not easily specified; the HTML5 specification is
        // complicated.
        //
        // Calling Parse on arbitrary input typically results in a 'well-formed' parse
        // tree. However, it is possible for Parse to yield a 'badly-formed' parse tree.
        // For example, in a 'well-formed' parse tree, no <a> element is a child of
        // another <a> element: parsing "<a><a>" results in two sibling elements.
        // Similarly, in a 'well-formed' parse tree, no <a> element is a child of a
        // <table> element: parsing "<p><table><a>" results in a <p> with two sibling
        // children; the <a> is reparented to the <table>'s parent. However, calling
        // Parse on "<a><table><a>" does not return an error, but the result has an <a>
        // element with an <a> child, and is therefore not 'well-formed'.
        //
        // Programmatically constructed trees are typically also 'well-formed', but it
        // is possible to construct a tree that looks innocuous but, when rendered and
        // re-parsed, results in a different tree. A simple example is that a solitary
        // text node would become a tree containing <html>, <head> and <body> elements.
        // Another example is that the programmatic equivalent of "a<head>b</head>c"
        // becomes "<html><head><head/><body>abc</body></html>".
        public static void Render(Stream w, Node n)
        {
            render1(w, n);
        }

        // plaintextAbort is returned from render1 when a <plaintext> element
        // has been rendered. No more end tags should be rendered after that.
        private static bool render1(Stream w, Node n)
        {
            Node c;

            // Render non-element nodes; these are the easy cases.
            switch (n.Type)
            {
                case NodeType.Error:
                    throw new NotSupportedException("cannot render an ErrorNode node");
                case NodeType.Text:
                    escape(w, n.Data);
                    return false;
                case NodeType.Document:
                    for (c = n.FirstChild; c != null; c = c.NextSibling)
                    {
                        if (render1(w, c))
                        {
                            return true;
                        }
                    }
                    return false;
                case NodeType.Element:
                    // No-op.
                    break;
                case NodeType.Comment:
                    w.WriteString("<!--");
                    w.WriteString(n.Data);
                    w.WriteString("-->");
                    return false;
                case NodeType.Doctype:
                    w.WriteString("<!DOCTYPE ");
                    w.WriteString(n.Data);
                    string p = "", s = "";
                    foreach (var a in n.Attr)
                    {
                        switch (a.Key)
                        {
                            case "public":
                                p = a.Val;
                                break;
                            case "system":
                                s = a.Val;
                                break;
                        }

                        if (p != "")
                        {
                            w.WriteString(" PUBLIC ");
                            writeQuoted(w, p);
                            if (!string.IsNullOrEmpty(s))
                            {
                                w.WriteByte((byte)' ');
                                writeQuoted(w, s);
                            }
                        }
                        else if (s != "")
                        {
                            w.WriteString(" SYSTEM ");
                            writeQuoted(w, s);
                        }
                    }
                    w.WriteByte((byte)'>');
                    return false;
                default:
                    throw new NotImplementedException("unknown node type");
            }

            // Render the <xxx> opening tag.
            w.WriteByte((byte)'<');
            w.WriteString(n.Data);
            foreach (var a in n.Attr)
            {
                w.WriteByte((byte)' ');
                if (a.Namespace != "")
                {
                    w.WriteString(a.Namespace);
                    w.WriteByte((byte)':');
                }
                w.WriteString(a.Key);
                w.WriteString("=\"");
                escape(w, a.Val);
                w.WriteByte((byte)'"');
            }
            if (voidElements.Contains(n.Data))
            {
                if (n.FirstChild != null)
                {
                    throw new NotSupportedException($"void element <{n.Data}> has child nodes");
                }
                w.WriteString("/>");
                return false;
            }
            w.WriteByte((byte)'>');

            // Add initial newline where there is danger of a newline beging ignored.
            c = n.FirstChild;
            if (c != null && c.Type == NodeType.Text && c.Data.StartsWith("\n"))
            {
                switch (n.Data)
                {
                    case "pre":
                    case "listing":
                    case "textarea":
                        w.WriteByte((byte)'\n');
                        break;
                }
            }

            // Render any child nodes.
            switch (n.Data)
            {
                case "iframe":
                case "noembed":
                case "noframes":
                case "noscript":
                case "plaintext":
                case "script":
                case "style":
                case "xmp":
                    for (c = n.FirstChild; c != null; c = c.NextSibling)
                    {
                        if (c.Type == NodeType.Text)
                        {
                            w.WriteString(c.Data);
                        }
                        else
                        {
                            if (render1(w, c))
                            {
                                return true;
                            }
                        }
                    }
                    if (n.Data == "plaintext")
                    {
                        // Don't render anything else. <plaintext> must be the
                        // last element in the file, with no closing tag.
                        return true;
                    }
                    break;
                default:
                    for (c = n.FirstChild; c != null; c = c.NextSibling)
                    {
                        if (render1(w, c))
                        {
                            return true;
                        }
                    }
                    break;
            }

            // Render the </xxx> closing tag.
            w.WriteString("</");
            w.WriteString(n.Data);
            w.WriteByte((byte)'>');
            return false;
        }

        // writeQuoted writes s to w surrounded by quotes. Normally it will use double
        // quotes, but if s contains a double quote, it will use single quotes.
        // It is used for writing the identifiers in a doctype declaration.
        // In valid HTML, they can't contain both types of quotes.
        private static void writeQuoted(Stream w, string s)
        {
            var q = (byte)'"';
            if (s.Contains("\""))
            {
                q = (byte)'\'';
            }
            w.WriteByte(q);
            w.WriteString(s);
            w.WriteByte(q);
        }

        // Section 12.1.2, "Elements", gives this list of void elements. Void elements
        // are those that can't have any contents.
        private static readonly HashSet<string> voidElements = new HashSet<string>
        {
            "area",
            "base",
            "br",
            "col",
            "command",
            "embed",
            "hr",
            "img",
            "input",
            "keygen",
            "link",
            "meta",
            "param",
            "source",
            "track",
            "wbr",
        };
    }
}
