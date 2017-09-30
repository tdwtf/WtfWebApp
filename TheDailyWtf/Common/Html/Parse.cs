using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using a = TheDailyWtf.Common.Html.Atom;

// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// See README.txt for a link to the original source code.

namespace TheDailyWtf.Common
{
    public static partial class Html
    {
        private static readonly char[] whitespace = " \t\r\n\f".ToCharArray();
        private static readonly char[] whitespaceOrNUL = whitespace.Concat(new[] { '\x00' }).ToArray();

        // A parser implements the HTML5 parsing algorithm:
        // https://html.spec.whatwg.org/multipage/syntax.html#tree-construction
        private sealed class parser
        {
            // tokenizer provides the tokens for the parser.
            internal Tokenizer tokenizer;
            // tok is the most recently read token.
            private Token tok;
            // Self-closing tags like <hr/> are treated as start tags, except that
            // hasSelfClosingToken is set while they are being processed.
            private bool hasSelfClosingToken;
            // doc is the document root element.
            internal Node doc;
            // The stack of open elements (section 12.2.3.2) and active formatting
            // elements (section 12.2.3.3).
            internal nodeStack oe = new nodeStack(), afe = new nodeStack();
            // Element pointers (section 12.2.3.4).
            internal Node head, form;
            // Other parsing state flags (section 12.2.3.5).
            internal bool scripting, framesetOK;
            // im is the current insertion mode.
            internal insertionMode im;
            // originalIM is the insertion mode to go back to after completing a text
            // or inTableText insertion mode.
            private insertionMode originalIM;
            // fosterParenting is whether new elements should be inserted according to
            // the foster parenting rules (section 12.2.5.3).
            private bool fosterParenting;
            // quirks is whether the parser is operating in "quirks mode."
            private bool quirks;
            // fragment is whether the parser is parsing an HTML fragment.
            internal bool fragment;
            // context is the context element when parsing an HTML fragment
            // (section 12.4).
            internal Node context;

            private Node top => this.oe.top() ?? this.doc;

            // Stop tags for use in popUntil. These come from section 12.2.3.2.
            private static readonly Dictionary<string, a.AtomType[]> defaultScopeStopTags = new Dictionary<string, a.AtomType[]>
            {
                {"", new[]{a.Applet, a.Caption, a.Html, a.Table, a.Td, a.Th, a.Marquee, a.Object, a.Template}},
                {"math", new[]{a.AnnotationXml, a.Mi, a.Mn, a.Mo, a.Ms, a.Mtext}},
                {"svg", new[]{a.Desc, a.ForeignObject, a.Title}},
            };

            private enum scope
            {
                defaultScope,
                listItem,
                button,
                table,
                tableRow,
                tableBody,
                select
            }

            // popUntil pops the stack of open elements at the highest element whose tag
            // is in matchTags, provided there is no higher element in the scope's stop
            // tags (as defined in section 12.2.3.2). It returns whether or not there was
            // such an element. If there was not, popUntil leaves the stack unchanged.
            //
            // For example, the set of stop tags for table scope is: "html", "table". If
            // the stack was:
            // ["html", "body", "font", "table", "b", "i", "u"]
            // then popUntil(tableScope, "font") would return false, but
            // popUntil(tableScope, "i") would return true and the stack would become:
            // ["html", "body", "font", "table", "b"]
            //
            // If an element's tag is in both the stop tags and matchTags, then the stack
            // will be popped and the function returns true (provided, of course, there was
            // no higher element in the stack that was also in the stop tags). For example,
            // popUntil(tableScope, "table") returns true and leaves:
            // ["html", "body", "font"]
            private bool popUntil(scope s, params a.AtomType[] matchTags)
            {
                var i = this.indexOfElementInScope(s, matchTags);
                if (i != -1)
                {
                    this.oe = this.oe[0, i];
                    return true;
                }
                return false;
            }

            // indexOfElementInScope returns the index in p.oe of the highest element whose
            // tag is in matchTags that is in scope. If no matching element is in scope, it
            // returns -1.
            private int indexOfElementInScope(scope s, params a.AtomType[] matchTags)
            {
                for (var i = this.oe.len - 1; i >= 0; i--)
                {
                    var tagAtom = this.oe[i].DataAtom;
                    if (this.oe[i].Namespace == "")
                    {
                        foreach (var t in matchTags)
                        {
                            if (t == tagAtom)
                            {
                                return i;
                            }
                        }
                        switch (s)
                        {
                            case scope.defaultScope:
                                // No-op.
                                break;
                            case scope.listItem:
                                if (tagAtom == a.Ol || tagAtom == a.Ul)
                                {
                                    return -1;
                                }
                                break;
                            case scope.button:
                                if (tagAtom == a.Button)
                                {
                                    return -1;
                                }
                                break;
                            case scope.table:
                                if (tagAtom == a.Html || tagAtom == a.Table)
                                {
                                    return -1;
                                }
                                break;
                            case scope.select:
                                if (tagAtom != a.Optgroup && tagAtom != a.Option)
                                {
                                    return -1;
                                }
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                    switch (s)
                    {
                        case scope.defaultScope:
                        case scope.listItem:
                        case scope.button:
                            if (defaultScopeStopTags.TryGetValue(this.oe[i].Namespace, out var tags))
                            {
                                foreach (var t in tags)
                                {
                                    if (t == tagAtom)
                                    {
                                        return -1;
                                    }
                                }
                            }
                            break;
                    }
                }
                return -1;
            }

            // elementInScope is like popUntil, except that it doesn't modify the stack of
            // open elements.
            private bool elementInScope(scope s, params a.AtomType[] matchTags)
            {
                return this.indexOfElementInScope(s, matchTags) != -1;
            }

            // clearStackToContext pops elements off the stack of open elements until a
            // scope-defined element is found.
            private void clearStackToContext(scope s)
            {
                for (var i = this.oe.len - 1; i >= 0; i--)
                {
                    var tagAtom = this.oe[i].DataAtom;
                    switch (s)
                    {
                        case scope.table:
                            if (tagAtom == a.Html || tagAtom == a.Table)
                            {
                                this.oe = this.oe[0, i + 1];
                                return;
                            }
                            break;
                        case scope.tableRow:
                            if (tagAtom == a.Html || tagAtom == a.Tr)
                            {
                                this.oe = this.oe[0, i + 1];
                                return;
                            }
                            break;
                        case scope.tableBody:
                            if (tagAtom == a.Html || tagAtom == a.Tbody || tagAtom == a.Tfoot || tagAtom == a.Thead)
                            {
                                this.oe = this.oe[0, i + 1];
                                return;
                            }
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }

            // generateImpliedEndTags pops nodes off the stack of open elements as long as
            // the top node has a tag name of dd, dt, li, option, optgroup, p, rp, or rt.
            // If exceptions are specified, nodes with that name will not be popped off.
            private void generateImpliedEndTags(params string[] exceptions)
            {
                int i;
                for (i = this.oe.len - 1; i >= 0; i--)
                {
                    var n = this.oe[i];
                    if (n.Type == NodeType.Element)
                    {
                        if (n.DataAtom == a.Dd || n.DataAtom == a.Dt || n.DataAtom == a.Li || n.DataAtom == a.Option || n.DataAtom == a.Optgroup || n.DataAtom == a.P || n.DataAtom == a.Rp || n.DataAtom == a.Rt)
                        {
                            foreach (var except in exceptions)
                            {
                                if (n.Data == except)
                                {
                                    goto breakLoop;
                                }
                            }
                            continue;
                        }
                    }
                    break;
                }
                breakLoop:

                this.oe = this.oe[0, i + 1];
            }

            // addChild adds a child node n to the top element, and pushes n onto the stack
            // of open elements if it is an element node.
            private void addChild(Node n)
            {
                if (this.shouldFosterParent())
                {
                    this.fosterParent(n);
                }
                else
                {
                    this.top.AppendChild(n);
                }

                if (n.Type == NodeType.Element)
                {
                    this.oe.push(n);
                }
            }

            // shouldFosterParent returns whether the next node to be added should be
            // foster parented.
            private bool shouldFosterParent()
            {
                if (this.fosterParenting)
                {
                    if (this.top.DataAtom == a.Table || this.top.DataAtom == a.Tbody || this.top.DataAtom == a.Tfoot || this.top.DataAtom == a.Thead || this.top.DataAtom == a.Tr)
                    {
                        return true;
                    }
                }
                return false;
            }

            // fosterParent adds a child node according to the foster parenting rules.
            // Section 12.2.5.3, "foster parenting".
            private void fosterParent(Node n)
            {
                Node table = null, parent = null, prev = null;
                int i;
                for (i = this.oe.len - 1; i >= 0; i--)
                {
                    if (this.oe[i].DataAtom == a.Table)
                    {
                        table = this.oe[i];
                        break;
                    }
                }

                if (table == null)
                {
                    // The foster parent is the html element.
                    parent = this.oe[0];
                }
                else
                {
                    parent = table.Parent;
                }
                if (parent == null)
                {
                    parent = this.oe[i - 1];
                }

                if (table != null)
                {
                    prev = table.PrevSibling;
                }
                else
                {
                    prev = parent.LastChild;
                }
                if (prev != null && prev.Type == NodeType.Text && n.Type == NodeType.Text)
                {
                    prev.Data += n.Data;
                    return;
                }

                parent.InsertBefore(n, table);
            }

            // addText adds text to the preceding node if it is a text node, or else it
            // calls addChild with a new text node.
            private void addText(string text)
            {
                if (text == "")
                {
                    return;
                }

                if (this.shouldFosterParent())
                {
                    this.fosterParent(new Node
                    {
                        Type = NodeType.Text,
                        Data = text,
                    });
                    return;
                }

                var t = this.top;
                var n = t.LastChild;
                if (n != null && n.Type == NodeType.Text)
                {
                    n.Data += text;
                    return;
                }
                this.addChild(new Node
                {
                    Type = NodeType.Text,
                    Data = text,
                });
            }

            // addElement adds a child element based on the current token.
            private void addElement()
            {
                this.addChild(new Node
                {
                    Type = NodeType.Element,
                    DataAtom = this.tok.DataAtom,
                    Data = this.tok.Data,
                    Attr = this.tok.Attr,
                });
            }

            // Section 12.2.3.3.
            private void addFormattingElement()
            {
                var tagAtom = this.tok.DataAtom;
                var attr = this.tok.Attr;
                this.addElement();

                // Implement the Noah's Ark clause, but with three per family instead of two.
                int identicalElements = 0;
                for (var i = this.afe.len - 1; i >= 0; i--)
                {
                    var n = this.afe[i];
                    if (n.Type == NodeType.scopeMarker)
                    {
                        break;
                    }
                    if (n.Type != NodeType.Element)
                    {
                        continue;
                    }
                    if (n.Namespace != "")
                    {
                        continue;
                    }
                    if (n.DataAtom != tagAtom)
                    {
                        continue;
                    }
                    if (n.Attr.Count != attr.Count)
                    {
                        continue;
                    }
                    foreach (var t0 in n.Attr)
                    {
                        foreach (var t1 in attr)
                        {
                            if (t0.Key == t1.Key && t0.Namespace == t1.Namespace && t0.Val == t1.Val)
                            {
                                // Found a match for this attribute, continue with the next attribute.
                                goto continueCompareAttributes;
                            }
                        }
                        // If we get here, there is no attribute that matches a.
                        // Therefore the element is not identical to the new one.
                        goto continueFindIdenticalElements;
                        continueCompareAttributes:
                        ;
                    }

                    identicalElements++;
                    if (identicalElements >= 3)
                    {
                        this.afe.remove(n);
                    }
                    continueFindIdenticalElements:
                    ;
                }

                this.afe.push(this.top);
            }

            // Section 12.2.3.3.
            private void clearActiveFormattingElements()
            {
                while (true)
                {
                    var n = this.afe.pop();
                    if (this.afe.len == 0 || n.Type == NodeType.scopeMarker)
                    {
                        return;
                    }
                }
            }

            // Section 12.2.3.3.
            private void reconstructActiveFormattingElements()
            {
                var n = this.afe.top();
                if (n == null)
                {
                    return;
                }
                if (n.Type == NodeType.scopeMarker || this.oe.index(n) != -1)
                {
                    return;
                }
                var i = this.afe.len - 1;
                while (n.Type != NodeType.scopeMarker && this.oe.index(n) == -1)
                {
                    if (i == 0)
                    {
                        i = -1;
                        break;
                    }
                    i--;
                    n = this.afe[i];
                }
                while (true)
                {
                    i++;
                    var clone = this.afe[i].clone();
                    this.addChild(clone);
                    this.afe[i] = clone;
                    if (i == this.afe.len - 1)
                    {
                        break;
                    }
                }
            }

            // Section 12.2.4.
            private void acknowledgeSelfClosingTag()
            {
                this.hasSelfClosingToken = false;
            }

            // An insertion mode (section 12.2.3.1) is the state transition function from
            // a particular state in the HTML5 parser's state machine. It updates the
            // parser's fields depending on parser.tok (where ErrorToken means EOF).
            // It returns whether the token was consumed.
            public delegate bool insertionMode(parser p);

            // setOriginalIM sets the insertion mode to return to after completing a text or
            // inTableText insertion mode.
            // Section 12.2.3.1, "using the rules for".
            private void setOriginalIM()
            {
                if (this.originalIM != null)
                {
                    throw new NotSupportedException("bad parser state: originalIM was set twice");
                }
                this.originalIM = this.im;
            }

            // Section 12.2.3.1, "reset the insertion mode".
            internal void resetInsertionMode()
            {
                for (var i = this.oe.len - 1; i >= 0; i--)
                {
                    var n = this.oe[i];
                    if (i == 0 && this.context != null)
                    {
                        n = this.context;
                    }

                    if (n.DataAtom == a.Select)
                    {
                        this.im = inSelectIM;
                    }
                    else if (n.DataAtom == a.Td || n.DataAtom == a.Th)
                    {
                        this.im = inCellIM;
                    }
                    else if (n.DataAtom == a.Tr)
                    {
                        this.im = inRowIM;
                    }
                    else if (n.DataAtom == a.Tbody || n.DataAtom == a.Thead || n.DataAtom == a.Tfoot)
                    {
                        this.im = inTableBodyIM;
                    }
                    else if (n.DataAtom == a.Caption)
                    {
                        this.im = inCaptionIM;
                    }
                    else if (n.DataAtom == a.Colgroup)
                    {
                        this.im = inColumnGroupIM;
                    }
                    else if (n.DataAtom == a.Table)
                    {
                        this.im = inTableIM;
                    }
                    else if (n.DataAtom == a.Head)
                    {
                        this.im = inBodyIM;
                    }
                    else if (n.DataAtom == a.Body)
                    {
                        this.im = inBodyIM;
                    }
                    else if (n.DataAtom == a.Frameset)
                    {
                        this.im = inFramesetIM;
                    }
                    else if (n.DataAtom == a.Html)
                    {
                        this.im = beforeHeadIM;
                    }
                    else
                    {
                        continue;
                    }
                    return;
                }
                this.im = inBodyIM;
            }

            // Section 12.2.5.4.1.
            internal static bool initialIM(parser p)
            {
                switch (p.tok.Type)
                {
                    case TokenType.Text:
                        p.tok.Data = p.tok.Data.TrimStart(whitespace);
                        if (p.tok.Data.Length == 0)
                        {
                            // It was all whitespace, so ignore it.
                            return true;
                        }
                        break;
                    case TokenType.Comment:
                        p.doc.AppendChild(new Node
                        {
                            Type = NodeType.Comment,
                            Data = p.tok.Data,
                        });
                        return true;
                    case TokenType.Doctype:
                        var (n, quirks) = parseDoctype(p.tok.Data);
                        p.doc.AppendChild(n);
                        p.quirks = quirks;
                        p.im = beforeHTMLIM;
                        return true;
                }
                p.quirks = true;
                p.im = beforeHTMLIM;
                return false;
            }

            // Section 12.2.5.4.2.
            private static bool beforeHTMLIM(parser p)
            {
                switch (p.tok.Type)
                {
                    case TokenType.Doctype:
                        // Ignore the token.
                        return true;
                    case TokenType.Text:
                        p.tok.Data = p.tok.Data.TrimStart(whitespace);
                        if (p.tok.Data.Length == 0)
                        {
                            // It was all whitespace, so ignore it.
                            return true;
                        }
                        break;
                    case TokenType.StartTag:
                        if (p.tok.DataAtom == a.Html)
                        {
                            p.addElement();
                            p.im = beforeHeadIM;
                            return true;
                        }
                        break;
                    case TokenType.EndTag:
                        if (p.tok.DataAtom == a.Head || p.tok.DataAtom == a.Body || p.tok.DataAtom == a.Html || p.tok.DataAtom == a.Br)
                        {
                            p.parseImpliedToken(TokenType.StartTag, a.Html, a.Html.ToString());
                            return false;
                        }
                        else
                        {
                            // Ignore the token.
                            return true;
                        }
                    case TokenType.Comment:
                        p.doc.AppendChild(new Node
                        {
                            Type = NodeType.Comment,
                            Data = p.tok.Data,
                        });
                        return true;
                }
                p.parseImpliedToken(TokenType.StartTag, a.Html, a.Html.ToString());
                return false;
            }

            // Section 12.2.5.4.3.
            private static bool beforeHeadIM(parser p)
            {
                switch (p.tok.Type)
                {
                    case TokenType.Text:
                        p.tok.Data = p.tok.Data.TrimStart(whitespace);
                        if (p.tok.Data.Length == 0)
                        {
                            // It was all whitespace, so ignore it.
                            return true;
                        }
                        break;
                    case TokenType.StartTag:
                        if (p.tok.DataAtom == a.Head)
                        {
                            p.addElement();
                            p.head = p.top;
                            p.im = inHeadIM;
                            return true;
                        }
                        else if (p.tok.DataAtom == a.Html)
                        {
                            return inBodyIM(p);
                        }
                        break;
                    case TokenType.EndTag:
                        if (p.tok.DataAtom == a.Head || p.tok.DataAtom == a.Body || p.tok.DataAtom == a.Html || p.tok.DataAtom == a.Br)
                        {
                            p.parseImpliedToken(TokenType.StartTag, a.Head, a.Head.ToString());
                            return false;
                        }
                        else
                        {
                            // Ignore the token.
                            return true;
                        }
                    case TokenType.Comment:
                        p.addChild(new Node
                        {
                            Type = NodeType.Comment,
                            Data = p.tok.Data,
                        });
                        return true;
                    case TokenType.Doctype:
                        // Ignore the token.
                        return true;
                }

                p.parseImpliedToken(TokenType.StartTag, a.Head, a.Head.ToString());
                return false;
            }

            // Section 12.2.5.4.4.
            private static bool inHeadIM(parser p)
            {
                switch (p.tok.Type)
                {
                    case TokenType.Text:
                        var s = p.tok.Data.TrimStart(whitespace);
                        if (s.Length < p.tok.Data.Length)
                        {
                            // Add the initial whitespace to the current node.
                            p.addText(p.tok.Data.Substring(0, p.tok.Data.Length - s.Length));
                            if (s == "")
                            {
                                return true;
                            }
                            p.tok.Data = s;
                        }
                        break;
                    case TokenType.StartTag:
                        if (p.tok.DataAtom == a.Html)
                        {
                            return inBodyIM(p);
                        }
                        else if (p.tok.DataAtom == a.Base || p.tok.DataAtom == a.Basefont || p.tok.DataAtom == a.Bgsound || p.tok.DataAtom == a.Command || p.tok.DataAtom == a.Link || p.tok.DataAtom == a.Meta)
                        {
                            p.addElement();
                            p.oe.pop();
                            p.acknowledgeSelfClosingTag();
                            return true;
                        }
                        else if (p.tok.DataAtom == a.Script || p.tok.DataAtom == a.Title || p.tok.DataAtom == a.Noscript || p.tok.DataAtom == a.Noframes || p.tok.DataAtom == a.Style)
                        {
                            p.addElement();
                            p.setOriginalIM();
                            p.im = textIM;
                            return true;
                        }
                        else if (p.tok.DataAtom == a.Head)
                        {
                            // Ignore the token.
                            return true;
                        }
                        break;
                    case TokenType.EndTag:
                        if (p.tok.DataAtom == a.Head)
                        {
                            var n = p.oe.pop();
                            if (n.DataAtom != a.Head)
                            {
                                throw new NotImplementedException("bad parser state: <head> element not found, in the in-head insertion mode");
                            }
                            p.im = afterHeadIM;
                            return true;
                        }
                        else if (p.tok.DataAtom == a.Body || p.tok.DataAtom == a.Html || p.tok.DataAtom == a.Br)
                        {
                            p.parseImpliedToken(TokenType.EndTag, a.Head, a.Head.ToString());
                            return false;
                        }
                        else
                        {
                            // Ignore the token.
                            return true;
                        }
                    case TokenType.Comment:
                        p.addChild(new Node
                        {
                            Type = NodeType.Comment,
                            Data = p.tok.Data,
                        });
                        return true;
                    case TokenType.Doctype:
                        // Ignore the token.
                        return true;
                }

                p.parseImpliedToken(TokenType.EndTag, a.Head, a.Head.ToString());
                return false;
            }

            // Section 12.2.5.4.6.
            private static bool afterHeadIM(parser p)
            {
                switch (p.tok.Type)
                {
                    case TokenType.Text:
                        var s = p.tok.Data.TrimStart(whitespace);
                        if (s.Length < p.tok.Data.Length)
                        {
                            // Add the initial whitespace to the current node.
                            p.addText(p.tok.Data.Substring(0, p.tok.Data.Length - s.Length));
                            if (s == "")
                            {
                                return true;
                            }
                            p.tok.Data = s;
                        }
                        break;
                    case TokenType.StartTag:
                        if (p.tok.DataAtom == a.Html)
                        {
                            return inBodyIM(p);
                        }
                        else if (p.tok.DataAtom == a.Body)
                        {
                            p.addElement();
                            p.framesetOK = false;
                            p.im = inBodyIM;
                            return true;
                        }
                        else if (p.tok.DataAtom == a.Frameset)
                        {
                            p.addElement();
                            p.im = inFramesetIM;
                            return true;
                        }
                        else if (p.tok.DataAtom == a.Base || p.tok.DataAtom == a.Basefont || p.tok.DataAtom == a.Bgsound || p.tok.DataAtom == a.Link || p.tok.DataAtom == a.Meta || p.tok.DataAtom == a.Noframes || p.tok.DataAtom == a.Script || p.tok.DataAtom == a.Style || p.tok.DataAtom == a.Title)
                        {
                            p.oe.push(p.head);
                            try
                            {
                                return inHeadIM(p);
                            }
                            finally
                            {
                                p.oe.remove(p.head);
                            }
                        }
                        else if (p.tok.DataAtom == a.Head)
                        {
                            // Ignore the token.
                            return true;
                        }
                        break;
                    case TokenType.EndTag:
                        if (p.tok.DataAtom == a.Body || p.tok.DataAtom == a.Html || p.tok.DataAtom == a.Br)
                        {
                            // Drop down to creating an implied <body> tag.
                        }
                        else
                        {
                            // Ignore the token.
                            return true;
                        }
                        break;
                    case TokenType.Comment:
                        p.addChild(new Node
                        {
                            Type = NodeType.Comment,
                            Data = p.tok.Data,
                        });
                        return true;
                    case TokenType.Doctype:
                        // Ignore the token.
                        return true;
                }

                p.parseImpliedToken(TokenType.StartTag, a.Body, a.Body.ToString());
                p.framesetOK = true;
                return false;
            }

            // copyAttributes copies attributes of src not found on dst to dst.
            private static void copyAttributes(Node dst, Token src)
            {
                if (src.Attr.Count == 0)
                {
                    return;
                }
                var attr = new Dictionary<string, string>();
                foreach (var t in dst.Attr)
                {
                    attr[t.Key] = t.Val;
                }
                foreach (var t in src.Attr)
                {
                    if (!attr.ContainsKey(t.Key))
                    {
                        dst.Attr.Add(t);
                        attr[t.Key] = t.Val;
                    }
                }
            }

            // Section 12.2.5.4.7.
            private static bool inBodyIM(parser p)
            {
                switch (p.tok.Type)
                {
                    case TokenType.Text:
                        var d = p.tok.Data;
                        var n = p.oe.top();
                        if (n.DataAtom == a.Pre || n.DataAtom == a.Listing)
                        {
                            if (n.FirstChild == null)
                            {
                                // Ignore a newline at the start of a <pre> block.
                                if (d != "" && d[0] == '\r')
                                {
                                    d = d.Substring(1);
                                }
                                if (d != "" && d[0] == '\n')
                                {
                                    d = d.Substring(1);
                                }
                            }
                        }
                        d = d.Replace("\x00", "");
                        if (d == "")
                        {
                            return true;
                        }
                        p.reconstructActiveFormattingElements();
                        p.addText(d);
                        if (p.framesetOK && d.TrimStart(whitespace) != "")
                        {
                            // There were non-whitespace characters inserted.
                            p.framesetOK = false;
                        }
                        break;
                    case TokenType.StartTag:
                        if (p.tok.DataAtom == a.Html)
                        {
                            copyAttributes(p.oe[0], p.tok);
                        }
                        else if (p.tok.DataAtom == a.Base || p.tok.DataAtom == a.Basefont || p.tok.DataAtom == a.Bgsound || p.tok.DataAtom == a.Command || p.tok.DataAtom == a.Link || p.tok.DataAtom == a.Meta || p.tok.DataAtom == a.Noframes || p.tok.DataAtom == a.Script || p.tok.DataAtom == a.Style || p.tok.DataAtom == a.Title)
                        {
                            return inHeadIM(p);
                        }
                        else if (p.tok.DataAtom == a.Body)
                        {
                            if (p.oe.len >= 2)
                            {
                                var body = p.oe[1];
                                if (body.Type == NodeType.Element && body.DataAtom == a.Body)
                                {
                                    p.framesetOK = false;
                                    copyAttributes(body, p.tok);
                                }
                            }
                        }
                        else if (p.tok.DataAtom == a.Frameset)
                        {
                            if (!p.framesetOK || p.oe.len < 2 || p.oe[1].DataAtom != a.Body)
                            {
                                // Ignore the token.
                                return true;
                            }
                            var body = p.oe[1];
                            if (body.Parent != null)
                            {
                                body.Parent.RemoveChild(body);
                            }
                            p.oe = p.oe[0, 1];
                            p.addElement();
                            p.im = inFramesetIM;
                            return true;
                        }
                        else if (p.tok.DataAtom == a.Address || p.tok.DataAtom == a.Article || p.tok.DataAtom == a.Aside || p.tok.DataAtom == a.Blockquote || p.tok.DataAtom == a.Center || p.tok.DataAtom == a.Details || p.tok.DataAtom == a.Dir || p.tok.DataAtom == a.Div || p.tok.DataAtom == a.Dl || p.tok.DataAtom == a.Fieldset || p.tok.DataAtom == a.Figcaption || p.tok.DataAtom == a.Figure || p.tok.DataAtom == a.Footer || p.tok.DataAtom == a.Header || p.tok.DataAtom == a.Hgroup || p.tok.DataAtom == a.Menu || p.tok.DataAtom == a.Nav || p.tok.DataAtom == a.Ol || p.tok.DataAtom == a.P || p.tok.DataAtom == a.Section || p.tok.DataAtom == a.Summary || p.tok.DataAtom == a.Ul)
                        {
                            p.popUntil(scope.button, a.P);
                            p.addElement();
                        }
                        else if (p.tok.DataAtom == a.H1 || p.tok.DataAtom == a.H2 || p.tok.DataAtom == a.H3 || p.tok.DataAtom == a.H4 || p.tok.DataAtom == a.H5 || p.tok.DataAtom == a.H6)
                        {
                            p.popUntil(scope.button, a.P);
                            var n1 = p.top;
                            if (n1.DataAtom == a.H1 || n1.DataAtom == a.H2 || n1.DataAtom == a.H3 || n1.DataAtom == a.H4 || n1.DataAtom == a.H5 || n1.DataAtom == a.H6)
                            {
                                p.oe.pop();
                            }
                            p.addElement();
                        }
                        else if (p.tok.DataAtom == a.Pre || p.tok.DataAtom == a.Listing)
                        {
                            p.popUntil(scope.button, a.P);
                            p.addElement();
                            // The newline, if any, will be dealt with by the TextToken case.
                            p.framesetOK = false;
                        }
                        else if (p.tok.DataAtom == a.Form)
                        {
                            if (p.form == null)
                            {
                                p.popUntil(scope.button, a.P);
                                p.addElement();
                                p.form = p.top;
                            }
                        }
                        else if (p.tok.DataAtom == a.Li)
                        {
                            p.framesetOK = false;
                            for (var i = p.oe.len - 1; i >= 0; i--)
                            {
                                var node = p.oe[i];
                                if (node.DataAtom == a.Li)
                                {
                                    p.oe = p.oe[0, i];
                                }
                                else if (node.DataAtom == a.Address || node.DataAtom == a.Div || node.DataAtom == a.P)
                                {
                                    continue;
                                }
                                else
                                {
                                    if (!isSpecialElement(node))
                                    {
                                        continue;
                                    }
                                }
                                break;
                            }
                            p.popUntil(scope.button, a.P);
                            p.addElement();
                        }
                        else if (p.tok.DataAtom == a.Dd || p.tok.DataAtom == a.Dt)
                        {
                            p.framesetOK = false;
                            for (var i = p.oe.len - 1; i >= 0; i--)
                            {
                                var node = p.oe[i];
                                if (node.DataAtom == a.Dd || node.DataAtom == a.Dt)
                                {
                                    p.oe = p.oe[0, i];
                                }
                                else if (node.DataAtom == a.Address || node.DataAtom == a.Div || node.DataAtom == a.P)
                                {
                                    continue;
                                }
                                else
                                {
                                    if (!isSpecialElement(node))
                                    {
                                        continue;
                                    }
                                }
                                break;
                            }
                            p.popUntil(scope.button, a.P);
                            p.addElement();
                        }
                        else if (p.tok.DataAtom == a.Plaintext)
                        {
                            p.popUntil(scope.button, a.P);
                            p.addElement();
                        }
                        else if (p.tok.DataAtom == a.Button)
                        {
                            p.popUntil(scope.defaultScope, a.Button);
                            p.reconstructActiveFormattingElements();
                            p.addElement();
                            p.framesetOK = false;
                        }
                        else if (p.tok.DataAtom == a.A)
                        {
                            for (var i = p.afe.len - 1; i >= 0 && p.afe[i].Type != NodeType.scopeMarker; i--)
                            {
                                var n1 = p.afe[i];
                                if (n1.Type == NodeType.Element && n1.DataAtom == a.A)
                                {
                                    p.inBodyEndTagFormatting(a.A);
                                    p.oe.remove(n1);
                                    p.afe.remove(n1);
                                    break;
                                }
                            }
                            p.reconstructActiveFormattingElements();
                            p.addFormattingElement();
                        }
                        else if (p.tok.DataAtom == a.B || p.tok.DataAtom == a.Big || p.tok.DataAtom == a.Code || p.tok.DataAtom == a.Em || p.tok.DataAtom == a.Font || p.tok.DataAtom == a.I || p.tok.DataAtom == a.S || p.tok.DataAtom == a.Small || p.tok.DataAtom == a.Strike || p.tok.DataAtom == a.Strong || p.tok.DataAtom == a.Tt || p.tok.DataAtom == a.U)
                        {
                            p.reconstructActiveFormattingElements();
                            p.addFormattingElement();
                        }
                        else if (p.tok.DataAtom == a.Nobr)
                        {
                            p.reconstructActiveFormattingElements();
                            if (p.elementInScope(scope.defaultScope, a.Nobr))
                            {
                                p.inBodyEndTagFormatting(a.Nobr);
                                p.reconstructActiveFormattingElements();
                            }
                            p.addFormattingElement();
                        }
                        else if (p.tok.DataAtom == a.Applet || p.tok.DataAtom == a.Marquee || p.tok.DataAtom == a.Object)
                        {
                            p.reconstructActiveFormattingElements();
                            p.addElement();
                            p.afe.push(scopeMarker);
                            p.framesetOK = false;
                        }
                        else if (p.tok.DataAtom == a.Table)
                        {
                            if (!p.quirks)
                            {
                                p.popUntil(scope.button, a.P);
                            }
                            p.addElement();
                            p.framesetOK = false;
                            p.im = inTableIM;
                            return true;
                        }
                        else if (p.tok.DataAtom == a.Area || p.tok.DataAtom == a.Br || p.tok.DataAtom == a.Embed || p.tok.DataAtom == a.Img || p.tok.DataAtom == a.Input || p.tok.DataAtom == a.Keygen || p.tok.DataAtom == a.Wbr)
                        {
                            p.reconstructActiveFormattingElements();
                            p.addElement();
                            p.oe.pop();
                            p.acknowledgeSelfClosingTag();
                            if (p.tok.DataAtom == a.Input)
                            {
                                foreach (var t in p.tok.Attr)
                                {
                                    if (t.Key == "type")
                                    {
                                        if (t.Val.ToLowerInvariant() == "hidden")
                                        {
                                            // Skip setting framesetOK = false
                                            return true;
                                        }
                                    }
                                }
                            }
                            p.framesetOK = false;
                        }
                        else if (p.tok.DataAtom == a.Param || p.tok.DataAtom == a.Source || p.tok.DataAtom == a.Track)
                        {
                            p.addElement();
                            p.oe.pop();
                            p.acknowledgeSelfClosingTag();
                        }
                        else if (p.tok.DataAtom == a.Hr)
                        {
                            p.popUntil(scope.button, a.P);
                            p.addElement();
                            p.oe.pop();
                            p.acknowledgeSelfClosingTag();
                            p.framesetOK = false;
                        }
                        else if (p.tok.DataAtom == a.Image)
                        {
                            p.tok.DataAtom = a.Img;
                            p.tok.Data = a.Img.ToString();
                            return false;
                        }
                        else if (p.tok.DataAtom == a.Isindex)
                        {
                            if (p.form != null)
                            {
                                // Ignore the token.
                                return true;
                            }
                            string action = "";
                            string prompt = "This is a searchable index. Enter search keywords: ";
                            var attr = new List<Attribute>
                            {
                                new Attribute
                                {
                                    Key = "name",
                                    Val = "isindex"
                                }
                            };
                            foreach (var t in p.tok.Attr)
                            {
                                switch (t.Key)
                                {
                                    case "action":
                                        action = t.Val;
                                        break;
                                    case "name":
                                        // Ignore the attribute.
                                        break;
                                    case "prompt":
                                        prompt = t.Val;
                                        break;
                                    default:
                                        attr.Add(t);
                                        break;
                                }
                            }
                            p.acknowledgeSelfClosingTag();
                            p.popUntil(scope.button, a.P);
                            p.parseImpliedToken(TokenType.StartTag, a.Form, a.Form.ToString());
                            if (action != "")
                            {
                                p.form.Attr = new List<Attribute>
                                {
                                    new Attribute
                                    {
                                        Key = "action",
                                        Val = action,
                                    }
                                };
                            }
                            p.parseImpliedToken(TokenType.StartTag, a.Hr, a.Hr.ToString());
                            p.parseImpliedToken(TokenType.StartTag, a.Label, a.Label.ToString());
                            p.addText(prompt);
                            p.addChild(new Node
                            {
                                Type = NodeType.Element,
                                DataAtom = a.Input,
                                Data = a.Input.ToString(),
                                Attr = attr,
                            });
                            p.oe.pop();
                            p.parseImpliedToken(TokenType.EndTag, a.Label, a.Label.ToString());
                            p.parseImpliedToken(TokenType.StartTag, a.Hr, a.Hr.ToString());
                            p.parseImpliedToken(TokenType.EndTag, a.Form, a.Form.ToString());
                        }
                        else if (p.tok.DataAtom == a.Textarea)
                        {
                            p.addElement();
                            p.setOriginalIM();
                            p.framesetOK = false;
                            p.im = textIM;
                        }
                        else if (p.tok.DataAtom == a.Xmp)
                        {
                            p.popUntil(scope.button, a.P);
                            p.reconstructActiveFormattingElements();
                            p.framesetOK = false;
                            p.addElement();
                            p.setOriginalIM();
                            p.im = textIM;
                        }
                        else if (p.tok.DataAtom == a.Iframe)
                        {
                            p.framesetOK = false;
                            p.addElement();
                            p.setOriginalIM();
                            p.im = textIM;
                        }
                        else if (p.tok.DataAtom == a.Noembed || p.tok.DataAtom == a.Noscript)
                        {
                            p.addElement();
                            p.setOriginalIM();
                            p.im = textIM;
                        }
                        else if (p.tok.DataAtom == a.Select)
                        {
                            p.reconstructActiveFormattingElements();
                            p.addElement();
                            p.framesetOK = false;
                            p.im = inSelectIM;
                            return true;
                        }
                        else if (p.tok.DataAtom == a.Optgroup || p.tok.DataAtom == a.Option)
                        {
                            if (p.top.DataAtom == a.Option)
                            {
                                p.oe.pop();
                            }
                            p.reconstructActiveFormattingElements();
                            p.addElement();
                        }
                        else if (p.tok.DataAtom == a.Rp || p.tok.DataAtom == a.Rt)
                        {
                            if (p.elementInScope(scope.defaultScope, a.Ruby))
                            {
                                p.generateImpliedEndTags();
                            }
                            p.addElement();
                        }
                        else if (p.tok.DataAtom == a.Math || p.tok.DataAtom == a.Svg)
                        {
                            p.reconstructActiveFormattingElements();
                            if (p.tok.DataAtom == a.Math)
                            {
                                adjustAttributeNames(p.tok.Attr, mathMLAttributeAdjustments);
                            }
                            else
                            {
                                adjustAttributeNames(p.tok.Attr, svgAttributeAdjustments);
                            }
                            adjustForeignAttributes(p.tok.Attr);
                            p.addElement();
                            p.top.Namespace = p.tok.Data;
                            if (p.hasSelfClosingToken)
                            {
                                p.oe.pop();
                                p.acknowledgeSelfClosingTag();
                            }
                            return true;
                        }
                        else if (p.tok.DataAtom == a.Caption || p.tok.DataAtom == a.Col || p.tok.DataAtom == a.Colgroup || p.tok.DataAtom == a.Frame || p.tok.DataAtom == a.Head || p.tok.DataAtom == a.Tbody || p.tok.DataAtom == a.Td || p.tok.DataAtom == a.Tfoot || p.tok.DataAtom == a.Th || p.tok.DataAtom == a.Thead || p.tok.DataAtom == a.Tr)
                        {
                            // Ignore the token.
                        }
                        else
                        {
                            p.reconstructActiveFormattingElements();
                            p.addElement();
                        }
                        break;
                    case TokenType.EndTag:
                        if (p.tok.DataAtom == a.Body)
                        {
                            if (p.elementInScope(scope.defaultScope, a.Body))
                            {
                                p.im = afterBodyIM;
                            }
                        }
                        else if (p.tok.DataAtom == a.Html)
                        {
                            if (p.elementInScope(scope.defaultScope, a.Body))
                            {
                                p.parseImpliedToken(TokenType.EndTag, a.Body, a.Body.ToString());
                                return false;
                            }
                            return true;
                        }
                        else if (p.tok.DataAtom == a.Address || p.tok.DataAtom == a.Article || p.tok.DataAtom == a.Aside || p.tok.DataAtom == a.Blockquote || p.tok.DataAtom == a.Button || p.tok.DataAtom == a.Center || p.tok.DataAtom == a.Details || p.tok.DataAtom == a.Dir || p.tok.DataAtom == a.Div || p.tok.DataAtom == a.Dl || p.tok.DataAtom == a.Fieldset || p.tok.DataAtom == a.Figcaption || p.tok.DataAtom == a.Figure || p.tok.DataAtom == a.Footer || p.tok.DataAtom == a.Header || p.tok.DataAtom == a.Hgroup || p.tok.DataAtom == a.Listing || p.tok.DataAtom == a.Menu || p.tok.DataAtom == a.Nav || p.tok.DataAtom == a.Ol || p.tok.DataAtom == a.Pre || p.tok.DataAtom == a.Section || p.tok.DataAtom == a.Summary || p.tok.DataAtom == a.Ul)
                        {
                            p.popUntil(scope.defaultScope, p.tok.DataAtom);
                        }
                        else if (p.tok.DataAtom == a.Form)
                        {
                            var node = p.form;
                            p.form = null;
                            var i = p.indexOfElementInScope(scope.defaultScope, a.Form);
                            if (node == null || i == -1 || p.oe[i] != node)
                            {
                                // Ignore the token.
                                return true;
                            }
                            p.generateImpliedEndTags();
                            p.oe.remove(node);
                        }
                        else if (p.tok.DataAtom == a.P)
                        {
                            if (!p.elementInScope(scope.button, a.P))
                            {
                                p.parseImpliedToken(TokenType.StartTag, a.P, a.P.ToString());
                            }
                            p.popUntil(scope.button, a.P);
                        }
                        else if (p.tok.DataAtom == a.Li)
                        {
                            p.popUntil(scope.listItem, a.Li);
                        }
                        else if (p.tok.DataAtom == a.Dd || p.tok.DataAtom == a.Dt)
                        {
                            p.popUntil(scope.defaultScope, p.tok.DataAtom);
                        }
                        else if (p.tok.DataAtom == a.H1 || p.tok.DataAtom == a.H2 || p.tok.DataAtom == a.H3 || p.tok.DataAtom == a.H4 || p.tok.DataAtom == a.H5 || p.tok.DataAtom == a.H6)
                        {
                            p.popUntil(scope.defaultScope, a.H1, a.H2, a.H3, a.H4, a.H5, a.H6);
                        }
                        else if (p.tok.DataAtom == a.A || p.tok.DataAtom == a.B || p.tok.DataAtom == a.Big || p.tok.DataAtom == a.Code || p.tok.DataAtom == a.Em || p.tok.DataAtom == a.Font || p.tok.DataAtom == a.I || p.tok.DataAtom == a.Nobr || p.tok.DataAtom == a.S || p.tok.DataAtom == a.Small || p.tok.DataAtom == a.Strike || p.tok.DataAtom == a.Strong || p.tok.DataAtom == a.Tt || p.tok.DataAtom == a.U)
                        {
                            p.inBodyEndTagFormatting(p.tok.DataAtom);
                        }
                        else if (p.tok.DataAtom == a.Applet || p.tok.DataAtom == a.Marquee || p.tok.DataAtom == a.Object)
                        {
                            if (p.popUntil(scope.defaultScope, p.tok.DataAtom))
                            {
                                p.clearActiveFormattingElements();
                            }
                        }
                        else if (p.tok.DataAtom == a.Br)
                        {
                            p.tok.Type = TokenType.StartTag;
                            return false;
                        }
                        else
                        {
                            p.inBodyEndTagOther(p.tok.DataAtom);
                        }
                        break;
                    case TokenType.Comment:
                        p.addChild(new Node
                        {
                            Type = NodeType.Comment,
                            Data = p.tok.Data,
                        });
                        break;
                }

                return true;
            }

            private void inBodyEndTagFormatting(a.AtomType tagAtom)
            {
                // This is the "adoption agency" algorithm, described at
                // https://html.spec.whatwg.org/multipage/syntax.html#adoptionAgency

                // TODO: this is a fairly literal line-by-line translation of that algorithm.
                // Once the code successfully parses the comprehensive test suite, we should
                // refactor this code to be more idiomatic.

                // Steps 1-4. The outer loop.
                for (var i = 0; i < 8; i++)
                {
                    // Step 5. Find the formatting element.
                    Node formattingElement = null;
                    for (var j = this.afe.len - 1; j >= 0; j--)
                    {
                        if (this.afe[j].Type == NodeType.scopeMarker)
                        {
                            break;
                        }
                        if (this.afe[j].DataAtom == tagAtom)
                        {
                            formattingElement = this.afe[j];
                            break;
                        }
                    }
                    if (formattingElement == null)
                    {
                        this.inBodyEndTagOther(tagAtom);
                        return;
                    }
                    var feIndex = this.oe.index(formattingElement);
                    if (feIndex == -1)
                    {
                        this.afe.remove(formattingElement);
                        return;
                    }
                    if (!this.elementInScope(scope.defaultScope, tagAtom))
                    {
                        // Ignore the tag.
                        return;
                    }

                    // Steps 9-10. Find the furthest block.
                    Node furthestBlock = null;
                    foreach (var e in this.oe[feIndex, this.oe.len].nodes)
                    {
                        if (isSpecialElement(e))
                        {
                            furthestBlock = e;
                            break;
                        }
                    }
                    if (furthestBlock == null)
                    {
                        var e = this.oe.pop();
                        while (e != formattingElement)
                        {
                            e = this.oe.pop();
                        }
                        this.afe.remove(e);
                        return;
                    }

                    // Steps 11-12. Find the common ancestor and bookmark node.
                    var commonAncestor = this.oe[feIndex - 1];
                    var bookmark = this.afe.index(formattingElement);

                    // Step 13. The inner loop. Find the lastNode to reparent.
                    var lastNode = furthestBlock;
                    var node = furthestBlock;
                    var x = this.oe.index(node);
                    // Steps 13.1-13.2
                    for (int j = 0; j < 3; j++)
                    {
                        // Step 13.3.
                        x--;
                        node = this.oe[x];
                        // Step 13.4 - 13.5.
                        if (this.afe.index(node) == -1)
                        {
                            this.oe.remove(node);
                            continue;
                        }
                        // Step 13.6.
                        if (node == formattingElement)
                        {
                            break;
                        }
                        // Step 13.7.
                        var clone2 = node.clone();
                        this.afe[this.afe.index(node)] = clone2;
                        this.oe[this.oe.index(node)] = clone2;
                        node = clone2;
                        // Step 13.8.
                        if (lastNode == furthestBlock)
                        {
                            bookmark = this.afe.index(node) + 1;
                        }
                        // Step 13.9.
                        if (lastNode.Parent != null)
                        {
                            lastNode.Parent.RemoveChild(lastNode);
                        }
                        node.AppendChild(lastNode);
                        // Step 13.10.
                        lastNode = node;
                    }

                    // Step 14. Reparent lastNode to the common ancestor,
                    // or for misnested table nodes, to the foster parent.
                    if (lastNode.Parent != null)
                    {
                        lastNode.Parent.RemoveChild(lastNode);
                    }
                    if (commonAncestor.DataAtom == a.Table || commonAncestor.DataAtom == a.Tbody || commonAncestor.DataAtom == a.Tfoot || commonAncestor.DataAtom == a.Thead || commonAncestor.DataAtom == a.Tr)
                    {
                        this.fosterParent(lastNode);
                    }
                    else
                    {
                        commonAncestor.AppendChild(lastNode);
                    }

                    // Steps 15-17. Reparent nodes from the furthest block's children
                    // to a clone of the formatting element.
                    var clone = formattingElement.clone();
                    Node.reparentChildren(clone, furthestBlock);
                    furthestBlock.AppendChild(clone);

                    // Step 18. Fix up the list of active formatting elements.
                    var oldLoc = this.afe.index(formattingElement);
                    if (oldLoc != -1 && oldLoc < bookmark)
                    {
                        // Move the bookmark with the rest of the list.
                        bookmark--;
                    }
                    this.afe.remove(formattingElement);
                    this.afe.insert(bookmark, clone);

                    // Step 19. Fix up the stack of open elements.
                    this.oe.remove(formattingElement);
                    this.oe.insert(this.oe.index(furthestBlock) + 1, clone);
                }
            }

            // inBodyEndTagOther performs the "any other end tag" algorithm for inBodyIM.
            // "Any other end tag" handling from 12.2.5.5 The rules for parsing tokens in foreign content
            // https://html.spec.whatwg.org/multipage/syntax.html#parsing-main-inforeign
            private void inBodyEndTagOther(a.AtomType tagAtom)
            {
                for (var i = this.oe.len - 1; i >= 0; i--)
                {
                    if (this.oe[i].DataAtom == tagAtom)
                    {
                        this.oe = this.oe[0, i];
                        break;
                    }
                    if (isSpecialElement(this.oe[i]))
                    {
                        break;
                    }
                }
            }

            // Section 12.2.5.4.8.
            private static bool textIM(parser p)
            {
                switch (p.tok.Type)
                {
                    case TokenType.Error:
                        p.oe.pop();
                        break;
                    case TokenType.Text:
                        var d = p.tok.Data;
                        var n = p.oe.top();
                        if (n.DataAtom == a.Textarea && n.FirstChild == null)
                        {
                            // Ignore a newline at the start of a <textarea> block.
                            if (d != "" && d[0] == '\r')
                            {
                                d = d.Substring(1);
                            }
                            if (d != "" && d[0] == '\n')
                            {
                                d = d.Substring(1);
                            }
                        }
                        if (d == "")
                        {
                            return true;
                        }
                        p.addText(d);
                        return true;
                    case TokenType.EndTag:
                        p.oe.pop();
                        break;
                }
                p.im = p.originalIM;
                p.originalIM = null;
                return p.tok.Type == TokenType.EndTag;
            }

            // Section 12.2.5.4.9.
            private static bool inTableIM(parser p)
            {
                switch (p.tok.Type)
                {
                    case TokenType.Error:
                        // Stop parsing.
                        return true;
                    case TokenType.Text:
                        p.tok.Data = p.tok.Data.Replace("\x00", "");
                        if (p.oe.top().DataAtom == a.Table || p.oe.top().DataAtom == a.Tbody || p.oe.top().DataAtom == a.Tfoot || p.oe.top().DataAtom == a.Thead || p.oe.top().DataAtom == a.Tr)
                        {
                            if (p.tok.Data.Trim(whitespace) == "")
                            {
                                p.addText(p.tok.Data);
                                return true;
                            }
                        }
                        break;
                    case TokenType.StartTag:
                        if (p.tok.DataAtom == a.Caption)
                        {
                            p.clearStackToContext(scope.table);
                            p.afe.push(scopeMarker);
                            p.addElement();
                            p.im = inCaptionIM;
                            return true;
                        }
                        else if (p.tok.DataAtom == a.Colgroup)
                        {
                            p.clearStackToContext(scope.table);
                            p.addElement();
                            p.im = inColumnGroupIM;
                            return true;
                        }
                        else if (p.tok.DataAtom == a.Col)
                        {
                            p.parseImpliedToken(TokenType.StartTag, a.Colgroup, a.Colgroup.ToString());
                            return false;
                        }
                        else if (p.tok.DataAtom == a.Tbody || p.tok.DataAtom == a.Tfoot || p.tok.DataAtom == a.Thead)
                        {
                            p.clearStackToContext(scope.table);
                            p.addElement();
                            p.im = inTableBodyIM;
                            return true;
                        }
                        else if (p.tok.DataAtom == a.Td || p.tok.DataAtom == a.Th || p.tok.DataAtom == a.Tr)
                        {
                            p.parseImpliedToken(TokenType.StartTag, a.Tbody, a.Tbody.ToString());
                            return false;
                        }
                        else if (p.tok.DataAtom == a.Table)
                        {
                            if (p.popUntil(scope.table, a.Table))
                            {
                                p.resetInsertionMode();
                                return false;
                            }
                            // Ignore the token.
                            return true;
                        }
                        else if (p.tok.DataAtom == a.Style || p.tok.DataAtom == a.Script)
                        {
                            return inHeadIM(p);
                        }
                        else if (p.tok.DataAtom == a.Input)
                        {
                            foreach (var t in p.tok.Attr)
                            {
                                if (t.Key == "type" && t.Val.ToLowerInvariant() == "hidden")
                                {
                                    p.addElement();
                                    p.oe.pop();
                                    return true;
                                }
                            }
                            // Otherwise drop down to the default action.
                        }
                        else if (p.tok.DataAtom == a.Form)
                        {
                            if (p.form != null)
                            {
                                // Ignore the token.
                                return true;
                            }
                            p.addElement();
                            p.form = p.oe.pop();
                        }
                        else if (p.tok.DataAtom == a.Select)
                        {
                            p.reconstructActiveFormattingElements();
                            if (p.top.DataAtom == a.Table || p.top.DataAtom == a.Tbody || p.top.DataAtom == a.Tfoot || p.top.DataAtom == a.Thead || p.top.DataAtom == a.Tr)
                            {
                                p.fosterParenting = true;
                            }
                            p.addElement();
                            p.fosterParenting = false;
                            p.framesetOK = false;
                            p.im = inSelectInTableIM;
                            return true;
                        }
                        break;
                    case TokenType.EndTag:
                        if (p.tok.DataAtom == a.Table)
                        {
                            if (p.popUntil(scope.table, a.Table))
                            {
                                p.resetInsertionMode();
                                return true;
                            }
                            // Ignore the token.
                            return true;
                        }
                        else if (p.tok.DataAtom == a.Body || p.tok.DataAtom == a.Caption || p.tok.DataAtom == a.Col || p.tok.DataAtom == a.Colgroup || p.tok.DataAtom == a.Html || p.tok.DataAtom == a.Tbody || p.tok.DataAtom == a.Td || p.tok.DataAtom == a.Tfoot || p.tok.DataAtom == a.Th || p.tok.DataAtom == a.Thead || p.tok.DataAtom == a.Tr)
                        {
                            // Ignore the token.
                            return true;
                        }
                        break;
                    case TokenType.Comment:
                        p.addChild(new Node
                        {
                            Type = NodeType.Comment,
                            Data = p.tok.Data,
                        });
                        return true;
                    case TokenType.Doctype:
                        // Ignore the token.
                        return true;
                }

                p.fosterParenting = true;
                try
                {
                    return inBodyIM(p);
                }
                finally
                {
                    p.fosterParenting = false;
                }
            }

            // Section 12.2.5.4.11.
            private static bool inCaptionIM(parser p)
            {
                switch (p.tok.Type)
                {
                    case TokenType.StartTag:
                        if (p.tok.DataAtom == a.Caption || p.tok.DataAtom == a.Col || p.tok.DataAtom == a.Colgroup || p.tok.DataAtom == a.Tbody || p.tok.DataAtom == a.Td || p.tok.DataAtom == a.Tfoot || p.tok.DataAtom == a.Thead || p.tok.DataAtom == a.Tr)
                        {
                            if (p.popUntil(scope.table, a.Caption))
                            {
                                p.clearActiveFormattingElements();
                                p.im = inTableIM;
                                return false;
                            }
                            else
                            {
                                // Ignore the token.
                                return true;
                            }
                        }
                        else if (p.tok.DataAtom == a.Select)
                        {
                            p.reconstructActiveFormattingElements();
                            p.addElement();
                            p.framesetOK = false;
                            p.im = inSelectInTableIM;
                            return true;
                        }
                        break;
                    case TokenType.EndTag:
                        if (p.tok.DataAtom == a.Caption)
                        {
                            if (p.popUntil(scope.table, a.Caption))
                            {
                                p.clearActiveFormattingElements();
                                p.im = inTableIM;
                            }
                            return true;
                        }
                        else if (p.tok.DataAtom == a.Table)
                        {
                            if (p.popUntil(scope.table, a.Caption))
                            {
                                p.clearActiveFormattingElements();
                                p.im = inTableIM;
                                return false;
                            }
                            else
                            {
                                // Ignore the token.
                                return true;
                            }
                        }
                        else if (p.tok.DataAtom == a.Body || p.tok.DataAtom == a.Col || p.tok.DataAtom == a.Colgroup || p.tok.DataAtom == a.Html || p.tok.DataAtom == a.Tbody || p.tok.DataAtom == a.Td || p.tok.DataAtom == a.Tfoot || p.tok.DataAtom == a.Th || p.tok.DataAtom == a.Thead || p.tok.DataAtom == a.Tr)
                        {
                            // Ignore the token.
                            return true;
                        }
                        break;
                }
                return inBodyIM(p);
            }

            // Section 12.2.5.4.12.
            private static bool inColumnGroupIM(parser p)
            {
                switch (p.tok.Type)
                {
                    case TokenType.Text:
                        var s = p.tok.Data.TrimStart(whitespace);
                        if (s.Length < p.tok.Data.Length)
                        {
                            // Add the initial whitespace to the current node.
                            p.addText(p.tok.Data.Substring(0, p.tok.Data.Length - s.Length));
                            if (s == "")
                            {
                                return true;
                            }
                            p.tok.Data = s;
                        }
                        break;
                    case TokenType.Comment:
                        p.addChild(new Node
                        {
                            Type = NodeType.Comment,
                            Data = p.tok.Data,
                        });
                        return true;
                    case TokenType.Doctype:
                        // Ignore the token.
                        return true;
                    case TokenType.StartTag:
                        if (p.tok.DataAtom == a.Html)
                        {
                            return inBodyIM(p);
                        }
                        else if (p.tok.DataAtom == a.Col)
                        {
                            p.addElement();
                            p.oe.pop();
                            p.acknowledgeSelfClosingTag();
                            return true;
                        }
                        break;
                    case TokenType.EndTag:
                        if (p.tok.DataAtom == a.Colgroup)
                        {
                            if (p.oe.top().DataAtom != a.Html)
                            {
                                p.oe.pop();
                                p.im = inTableIM;
                            }
                            return true;
                        }
                        else if (p.tok.DataAtom == a.Col)
                        {
                            // Ignore the token.
                            return true;
                        }
                        break;
                }
                if (p.oe.top().DataAtom != a.Html)
                {
                    p.oe.pop();
                    p.im = inTableIM;
                    return false;
                }
                return true;
            }

            // Section 12.2.5.4.13.
            private static bool inTableBodyIM(parser p)
            {
                switch (p.tok.Type)
                {
                    case TokenType.StartTag:
                        if (p.tok.DataAtom == a.Tr)
                        {
                            p.clearStackToContext(scope.tableBody);
                            p.addElement();
                            p.im = inRowIM;
                            return true;
                        }
                        else if (p.tok.DataAtom == a.Td || p.tok.DataAtom == a.Th)
                        {
                            p.parseImpliedToken(TokenType.StartTag, a.Tr, a.Tr.ToString());
                            return false;
                        }
                        else if (p.tok.DataAtom == a.Caption || p.tok.DataAtom == a.Col || p.tok.DataAtom == a.Colgroup || p.tok.DataAtom == a.Tbody || p.tok.DataAtom == a.Tfoot || p.tok.DataAtom == a.Thead)
                        {
                            if (p.popUntil(scope.table, a.Tbody, a.Thead, a.Tfoot))
                            {
                                p.im = inTableIM;
                                return false;
                            }
                            // Ignore the token.
                            return true;
                        }
                        break;
                    case TokenType.EndTag:
                        if (p.tok.DataAtom == a.Tbody || p.tok.DataAtom == a.Tfoot || p.tok.DataAtom == a.Thead)
                        {
                            if (p.elementInScope(scope.table, p.tok.DataAtom))
                            {
                                p.clearStackToContext(scope.tableBody);
                                p.oe.pop();
                                p.im = inTableIM;
                            }
                            return true;
                        }
                        else if (p.tok.DataAtom == a.Table)
                        {
                            if (p.popUntil(scope.table, a.Tbody, a.Thead, a.Tfoot))
                            {
                                p.im = inTableIM;
                                return false;
                            }
                            // Ignore the token.
                            return true;
                        }
                        else if (p.tok.DataAtom == a.Body || p.tok.DataAtom == a.Caption || p.tok.DataAtom == a.Col || p.tok.DataAtom == a.Colgroup || p.tok.DataAtom == a.Html || p.tok.DataAtom == a.Td || p.tok.DataAtom == a.Th || p.tok.DataAtom == a.Tr)
                        {
                            // Ignore the token.
                            return true;
                        }
                        break;
                    case TokenType.Comment:
                        p.addChild(new Node
                        {
                            Type = NodeType.Comment,
                            Data = p.tok.Data,
                        });
                        return true;
                }

                return inTableIM(p);
            }

            // Section 12.2.5.4.14.
            private static bool inRowIM(parser p)
            {
                switch (p.tok.Type)
                {
                    case TokenType.StartTag:
                        if (p.tok.DataAtom == a.Td || p.tok.DataAtom == a.Th)
                        {
                            p.clearStackToContext(scope.tableRow);
                            p.addElement();
                            p.afe.push(scopeMarker);
                            p.im = inCellIM;
                            return true;
                        }
                        else if (p.tok.DataAtom == a.Caption || p.tok.DataAtom == a.Col || p.tok.DataAtom == a.Colgroup || p.tok.DataAtom == a.Tbody || p.tok.DataAtom == a.Tfoot || p.tok.DataAtom == a.Thead || p.tok.DataAtom == a.Tr)
                        {
                            if (p.popUntil(scope.table, a.Tr))
                            {
                                p.im = inTableBodyIM;
                                return false;
                            }
                            // Ignore the token.
                            return true;
                        }
                        break;
                    case TokenType.EndTag:
                        if (p.tok.DataAtom == a.Tr)
                        {
                            if (p.popUntil(scope.table, a.Tr))
                            {
                                p.im = inTableBodyIM;
                                return true;
                            }
                            // Ignore the token.
                            return true;
                        }
                        else if (p.tok.DataAtom == a.Table)
                        {
                            if (p.popUntil(scope.table, a.Tr))
                            {
                                p.im = inTableBodyIM;
                                return false;
                            }
                            // Ignore the token.
                            return true;
                        }
                        else if (p.tok.DataAtom == a.Tbody || p.tok.DataAtom == a.Tfoot || p.tok.DataAtom == a.Thead)
                        {
                            if (p.elementInScope(scope.table, p.tok.DataAtom))
                            {
                                p.parseImpliedToken(TokenType.EndTag, a.Tr, a.Tr.ToString());
                                return false;
                            }
                            // Ignore the token.
                            return true;
                        }
                        else if (p.tok.DataAtom == a.Body || p.tok.DataAtom == a.Caption || p.tok.DataAtom == a.Col || p.tok.DataAtom == a.Colgroup || p.tok.DataAtom == a.Html || p.tok.DataAtom == a.Td || p.tok.DataAtom == a.Th)
                        {
                            // Ignore the token.
                            return true;
                        }
                        break;
                }

                return inTableIM(p);
            }

            // Section 12.2.5.4.15.
            private static bool inCellIM(parser p)
            {
                switch (p.tok.Type)
                {
                    case TokenType.StartTag:
                        if (p.tok.DataAtom == a.Caption || p.tok.DataAtom == a.Col || p.tok.DataAtom == a.Colgroup || p.tok.DataAtom == a.Tbody || p.tok.DataAtom == a.Td || p.tok.DataAtom == a.Tfoot || p.tok.DataAtom == a.Th || p.tok.DataAtom == a.Thead || p.tok.DataAtom == a.Tr)
                        {
                            if (p.popUntil(scope.table, a.Td, a.Th))
                            {
                                // Close the cell and reprocess.
                                p.clearActiveFormattingElements();
                                p.im = inRowIM;
                                return false;
                            }
                            // Ignore the token.
                            return true;
                        }
                        else if (p.tok.DataAtom == a.Select)
                        {
                            p.reconstructActiveFormattingElements();
                            p.addElement();
                            p.framesetOK = false;
                            p.im = inSelectInTableIM;
                            return true;
                        }
                        break;
                    case TokenType.EndTag:
                        if (p.tok.DataAtom == a.Td || p.tok.DataAtom == a.Th)
                        {
                            if (!p.popUntil(scope.table, p.tok.DataAtom))
                            {
                                // Ignore the token.
                                return true;
                            }
                            p.clearActiveFormattingElements();
                            p.im = inRowIM;
                            return true;
                        }
                        else if (p.tok.DataAtom == a.Body || p.tok.DataAtom == a.Caption || p.tok.DataAtom == a.Col || p.tok.DataAtom == a.Colgroup || p.tok.DataAtom == a.Html)
                        {
                            // Ignore the token.
                            return true;
                        }
                        else if (p.tok.DataAtom == a.Table || p.tok.DataAtom == a.Tbody || p.tok.DataAtom == a.Tfoot || p.tok.DataAtom == a.Thead || p.tok.DataAtom == a.Tr)
                        {
                            if (!p.elementInScope(scope.table, p.tok.DataAtom))
                            {
                                // Ignore the token.
                                return true;
                            }
                            // Close the cell and reprocess.
                            p.popUntil(scope.table, a.Td, a.Th);
                            p.clearActiveFormattingElements();
                            p.im = inRowIM;
                            return false;
                        }
                        break;
                }
                return inBodyIM(p);
            }

            // Section 12.2.5.4.16.
            private static bool inSelectIM(parser p)
            {
                switch (p.tok.Type)
                {
                    case TokenType.Error:
                        // Stop parsing.
                        return true;
                    case TokenType.Text:
                        p.addText(p.tok.Data.Replace("\x00", ""));
                        break;
                    case TokenType.StartTag:
                        if (p.tok.DataAtom == a.Html)
                        {
                            return inBodyIM(p);
                        }
                        else if (p.tok.DataAtom == a.Option)
                        {
                            if (p.top.DataAtom == a.Option)
                            {
                                p.oe.pop();
                            }
                            p.addElement();
                        }
                        else if (p.tok.DataAtom == a.Optgroup)
                        {
                            if (p.top.DataAtom == a.Option)
                            {
                                p.oe.pop();
                            }
                            if (p.top.DataAtom == a.Optgroup)
                            {
                                p.oe.pop();
                            }
                            p.addElement();
                        }
                        else if (p.tok.DataAtom == a.Select)
                        {
                            p.tok.Type = TokenType.EndTag;
                            return false;
                        }
                        else if (p.tok.DataAtom == a.Input || p.tok.DataAtom == a.Keygen || p.tok.DataAtom == a.Textarea)
                        {
                            if (p.elementInScope(scope.select, a.Select))
                            {
                                p.parseImpliedToken(TokenType.EndTag, a.Select, a.Select.ToString());
                                return false;
                            }
                            // In order to properly ignore <textarea>, we need to change the tokenizer mode.
                            p.tokenizer.NextIsNotRawText();
                            // Ignore the token.
                            return true;
                        }
                        else if (p.tok.DataAtom == a.Script)
                        {
                            return inHeadIM(p);
                        }
                        break;
                    case TokenType.EndTag:
                        if (p.tok.DataAtom == a.Option)
                        {
                            if (p.top.DataAtom == a.Option)
                            {
                                p.oe.pop();
                            }
                        }
                        else if (p.tok.DataAtom == a.Optgroup)
                        {
                            var i = p.oe.len - 1;
                            if (p.oe[i].DataAtom == a.Option)
                            {
                                i--;
                            }
                            if (p.oe[i].DataAtom == a.Optgroup)
                            {
                                p.oe = p.oe[0, i];
                            }
                        }
                        else if (p.tok.DataAtom == a.Select)
                        {
                            if (p.popUntil(scope.select, a.Select))
                            {
                                p.resetInsertionMode();
                            }
                        }
                        break;
                    case TokenType.Comment:
                        p.addChild(new Node
                        {
                            Type = NodeType.Comment,
                            Data = p.tok.Data,
                        });
                        break;
                    case TokenType.Doctype:
                        // Ignore the token.
                        return true;
                }

                return true;
            }

            // Section 12.2.5.4.17.
            private static bool inSelectInTableIM(parser p)
            {
                switch (p.tok.Type)
                {
                    case TokenType.StartTag:
                    case TokenType.EndTag:
                        if (p.tok.DataAtom == a.Caption || p.tok.DataAtom == a.Table || p.tok.DataAtom == a.Tbody || p.tok.DataAtom == a.Tfoot || p.tok.DataAtom == a.Thead || p.tok.DataAtom == a.Tr || p.tok.DataAtom == a.Td || p.tok.DataAtom == a.Th)
                        {
                            if (p.tok.Type == TokenType.StartTag || p.elementInScope(scope.table, p.tok.DataAtom))
                            {
                                p.parseImpliedToken(TokenType.EndTag, a.Select, a.Select.ToString());
                                return false;
                            }
                            else
                            {
                                // Ignore the token.
                                return true;
                            }
                        }
                        break;
                }
                return inSelectIM(p);
            }

            // Section 12.2.5.4.18.
            private static bool afterBodyIM(parser p)
            {
                switch (p.tok.Type)
                {
                    case TokenType.Error:
                        // Stop parsing.
                        return true;
                    case TokenType.Text:
                        var s = p.tok.Data.TrimStart(whitespace);
                        if (s.Length == 0)
                        {
                            // It was all whitespace.
                            return inBodyIM(p);
                        }
                        break;
                    case TokenType.StartTag:
                        if (p.tok.DataAtom == a.Html)
                        {
                            return inBodyIM(p);
                        }
                        break;
                    case TokenType.EndTag:
                        if (p.tok.DataAtom == a.Html)
                        {
                            if (!p.fragment)
                            {
                                p.im = afterAfterBodyIM;
                            }
                            return true;
                        }
                        break;
                    case TokenType.Comment:
                        // The comment is attached to the <html> element.
                        if (p.oe.len < 1 || p.oe[0].DataAtom != a.Html)
                        {
                            throw new NotImplementedException("bad parser state: <html> element not found, in the after-body insertion mode");
                        }
                        p.oe[0].AppendChild(new Node
                        {
                            Type = NodeType.Comment,
                            Data = p.tok.Data,
                        });
                        return true;
                }
                p.im = inBodyIM;
                return false;
            }

            // Section 12.2.5.4.19.
            private static bool inFramesetIM(parser p)
            {
                switch (p.tok.Type)
                {
                    case TokenType.Comment:
                        p.addChild(new Node
                        {
                            Type = NodeType.Comment,
                            Data = p.tok.Data,
                        });
                        break;
                    case TokenType.Text:
                        // Ignore all text but whitespace.
                        var s = new String(p.tok.Data.Where(c => c == ' ' || c == '\t' || c == '\n' || c == '\f' || c == '\r').ToArray());
                        if (s != "")
                        {
                            p.addText(s);
                        }
                        break;
                    case TokenType.StartTag:
                        if (p.tok.DataAtom == a.Html)
                        {
                            return inBodyIM(p);
                        }
                        else if (p.tok.DataAtom == a.Frameset)
                        {
                            p.addElement();
                        }
                        else if (p.tok.DataAtom == a.Frame)
                        {
                            p.addElement();
                            p.oe.pop();
                            p.acknowledgeSelfClosingTag();
                        }
                        else if (p.tok.DataAtom == a.Noframes)
                        {
                            return inHeadIM(p);
                        }
                        break;
                    case TokenType.EndTag:
                        if (p.tok.DataAtom == a.Frameset)
                        {
                            if (p.oe.top().DataAtom != a.Html)
                            {
                                p.oe.pop();
                                if (p.oe.top().DataAtom != a.Frameset)
                                {
                                    p.im = afterFramesetIM;
                                    return true;
                                }
                            }
                        }
                        break;
                    default:
                        // Ignore the token.
                        break;
                }
                return true;
            }

            // Section 12.2.5.4.20.
            private static bool afterFramesetIM(parser p)
            {
                switch (p.tok.Type)
                {
                    case TokenType.Comment:
                        p.addChild(new Node
                        {
                            Type = NodeType.Comment,
                            Data = p.tok.Data,
                        });
                        break;
                    case TokenType.Text:
                        // Ignore all text but whitespace.
                        var s = new String(p.tok.Data.Where(c => c == ' ' || c == '\t' || c == '\n' || c == '\f' || c == '\r').ToArray());
                        if (s != "")
                        {
                            p.addText(s);
                        }
                        break;
                    case TokenType.StartTag:
                        if (p.tok.DataAtom == a.Html)
                        {
                            return inBodyIM(p);
                        }
                        else if (p.tok.DataAtom == a.Noframes)
                        {
                            return inHeadIM(p);
                        }
                        break;
                    case TokenType.EndTag:
                        if (p.tok.DataAtom == a.Html)
                        {
                            p.im = afterAfterFramesetIM;
                            return true;
                        }
                        break;
                    default:
                        // Ignore the token.
                        break;
                }
                return true;
            }

            // Section 12.2.5.4.21.
            private static bool afterAfterBodyIM(parser p)
            {
                switch (p.tok.Type)
                {
                    case TokenType.Error:
                        // Stop parsing.
                        return true;
                    case TokenType.Text:
                        var s = p.tok.Data.TrimStart(whitespace);
                        if (s.Length == 0)
                        {
                            // It was all whitespace.
                            return inBodyIM(p);
                        }
                        break;
                    case TokenType.StartTag:
                        if (p.tok.DataAtom == a.Html)
                        {
                            return inBodyIM(p);
                        }
                        break;
                    case TokenType.Comment:
                        p.doc.AppendChild(new Node
                        {
                            Type = NodeType.Comment,
                            Data = p.tok.Data,
                        });
                        return true;
                    case TokenType.Doctype:
                        return inBodyIM(p);
                }
                p.im = inBodyIM;
                return false;
            }

            // Section 12.2.5.4.22.
            private static bool afterAfterFramesetIM(parser p)
            {
                switch (p.tok.Type)
                {
                    case TokenType.Comment:
                        p.doc.AppendChild(new Node
                        {
                            Type = NodeType.Comment,
                            Data = p.tok.Data,
                        });
                        break;
                    case TokenType.Text:
                        // Ignore all text but whitespace.
                        var s = new String(p.tok.Data.Where(c => c == ' ' || c == '\t' || c == '\n' || c == '\f' || c == '\r').ToArray());
                        if (s != "")
                        {
                            p.tok.Data = s;
                            return inBodyIM(p);
                        }
                        break;
                    case TokenType.StartTag:
                        if (p.tok.DataAtom == a.Html)
                        {
                            return inBodyIM(p);
                        }
                        else if (p.tok.DataAtom == a.Noframes)
                        {
                            return inHeadIM(p);
                        }
                        break;
                    case TokenType.Doctype:
                        return inBodyIM(p);
                    default:
                        // Ignore the token.
                        break;
                }
                return true;
            }

            // Section 12.2.5.5.
            private static bool parseForeignContent(parser p)
            {
                switch (p.tok.Type)
                {
                    case TokenType.Text:
                        if (p.framesetOK)
                        {
                            p.framesetOK = p.tok.Data.TrimStart(whitespaceOrNUL) == "";
                        }
                        p.tok.Data = p.tok.Data.Replace("\x00", "\ufffd");
                        p.addText(p.tok.Data);
                        break;
                    case TokenType.Comment:
                        p.addChild(new Node
                        {
                            Type = NodeType.Comment,
                            Data = p.tok.Data,
                        });
                        break;
                    case TokenType.StartTag:
                        var b = breakout.Contains(p.tok.Data);
                        if (p.tok.DataAtom == a.Font)
                        {
                            foreach (var attr in p.tok.Attr)
                            {
                                switch (attr.Key)
                                {
                                    case "color":
                                    case "face":
                                    case "size":
                                        b = true;
                                        goto breakLoop;
                                }
                            }
                            breakLoop:;
                        }
                        if (b)
                        {
                            for (var i = p.oe.len - 1; i >= 0; i--)
                            {
                                var n = p.oe[i];
                                if (n.Namespace == "" || htmlIntegrationPoint(n) || mathMLTextIntegrationPoint(n))
                                {
                                    p.oe = p.oe[0, i + 1];
                                    break;
                                }
                            }
                            return false;
                        }
                        switch (p.top.Namespace)
                        {
                            case "math":
                                adjustAttributeNames(p.tok.Attr, mathMLAttributeAdjustments);
                                break;
                            case "svg":
                                // Adjust SVG tag names. The tokenizer lower-cases tag names, but
                                // SVG wants e.g. "foreignObject" with a capital second "O".
                                if (svgTagNameAdjustments.TryGetValue(p.tok.Data, out var x))
                                {
                                    p.tok.DataAtom = a.Lookup(Encoding.UTF8.GetBytes(x));
                                    p.tok.Data = x;
                                }
                                adjustAttributeNames(p.tok.Attr, svgAttributeAdjustments);
                                break;
                            default:
                                throw new NotImplementedException("bad parser state: unexpected namespace");
                        }
                        adjustForeignAttributes(p.tok.Attr);
                        var ns = p.top.Namespace;
                        p.addElement();
                        p.top.Namespace = ns;
                        if (ns != "")
                        {
                            // Don't let the tokenizer go into raw text mode in foreign content
                            // (e.g. in an SVG <title> tag).
                            p.tokenizer.NextIsNotRawText();
                        }
                        if (p.hasSelfClosingToken)
                        {
                            p.oe.pop();
                            p.acknowledgeSelfClosingTag();
                        }
                        break;
                    case TokenType.EndTag:
                        for (var i = p.oe.len - 1; i >= 0; i--)
                        {
                            if (p.oe[i].Namespace == "")
                            {
                                return p.im(p);
                            }
                            if (p.oe[i].Data.Equals(p.tok.Data, StringComparison.InvariantCultureIgnoreCase))
                            {
                                p.oe = p.oe[0, i];
                                break;
                            }
                        }
                        return true;
                    default:
                        // Ignore the token.
                        break;
                }
                return true;
            }

            // Section 12.2.5.
            private bool inForeignContent()
            {
                if (this.oe.len == 0)
                {
                    return false;
                }
                var n = this.oe[this.oe.len - 1];
                if (n.Namespace == "")
                {
                    return false;
                }
                if (mathMLTextIntegrationPoint(n))
                {
                    if (this.tok.Type == TokenType.StartTag && this.tok.DataAtom != a.Mglyph && this.tok.DataAtom != a.Malignmark)
                    {
                        return false;
                    }
                    if (this.tok.Type == TokenType.Text)
                    {
                        return false;
                    }
                }
                if (n.Namespace == "math" && n.DataAtom == a.AnnotationXml && this.tok.Type == TokenType.StartTag && this.tok.DataAtom == a.Svg)
                {
                    return false;
                }
                if (htmlIntegrationPoint(n) && (this.tok.Type == TokenType.StartTag || this.tok.Type == TokenType.Text))
                {
                    return false;
                }
                if (this.tok.Type == TokenType.Error)
                {
                    return false;
                }
                return true;
            }

            // parseImpliedToken parses a token as though it had appeared in the parser's
            // input.
            private void parseImpliedToken(TokenType t, a.AtomType dataAtom, string data)
            {
                var realToken = this.tok;
                var selfClosing = this.hasSelfClosingToken;
                this.tok = new Token
                {
                    Type = t,
                    DataAtom = dataAtom,
                    Data = data,
                };
                this.hasSelfClosingToken = false;
                this.parseCurrentToken();
                this.tok = realToken;
                this.hasSelfClosingToken = selfClosing;
            }

            // parseCurrentToken runs the current token through the parsing routines
            // until it is consumed.
            private void parseCurrentToken()
            {
                if (this.tok.Type == TokenType.SelfClosingTag)
                {
                    this.hasSelfClosingToken = true;
                    this.tok.Type = TokenType.StartTag;
                }

                var consumed = false;
                while (!consumed)
                {
                    if (this.inForeignContent())
                    {
                        consumed = parseForeignContent(this);
                    }
                    else
                    {
                        consumed = this.im(this);
                    }
                }

                if (this.hasSelfClosingToken)
                {
                    // This is a parse error, but ignore it.
                    this.hasSelfClosingToken = false;
                }
            }

            internal void parse()
            {
                // Iterate until EOF. Any other error will cause an early return.
                while (true)
                {
                    // CDATA sections are allowed only in foreign content.
                    var n = this.oe.top();
                    this.tokenizer.AllowCDATA = n != null && n.Namespace != "";
                    // Read and parse the next token.
                    this.tokenizer.Next();
                    this.tok = this.tokenizer.Token();
                    if (this.tok.Type == TokenType.Error)
                    {
                        try
                        {
                            this.tokenizer.ThrowErr();
                        }
                        catch (EndOfStreamException)
                        {
                            break;
                        }
                    }
                    this.parseCurrentToken();
                }
            }
        }

        // Parse returns the parse tree for the HTML from the given Reader.
        // The input is assumed to be UTF-8 encoded.
        public static Node Parse(Stream r)
        {
            var p = new parser
            {
                tokenizer = new Tokenizer(r),
                doc = new Node
                {
                    Type = NodeType.Document,
                },
                scripting = true,
                framesetOK = true,
                im = parser.initialIM,
            };
            p.parse();
            return p.doc;
        }

        // ParseFragment parses a fragment of HTML and returns the nodes that were
        // found. If the fragment is the InnerHTML for an existing element, pass that
        // element in context.
        public static Node[] ParseFragment(Stream r, Node context)
        {
            var contextTag = "";
            if (context != null)
            {
                if (context.Type != NodeType.Element)
                {
                    throw new ArgumentException("ParseFragment of non-element Node", nameof(context));
                }
                // The next check isn't just context.DataAtom.String() == context.Data because
                // it is valid to pass an element whose tag isn't a known atom. For example,
                // DataAtom == 0 and Data = "tagfromthefuture" is perfectly consistent.
                if (context.DataAtom != a.Lookup(Encoding.UTF8.GetBytes(context.Data)))
                {
                    throw new ArgumentException($"inconsistent Node: DataAtom={context.DataAtom}, Data={context.Data}", nameof(context));
                }
                contextTag = context.DataAtom.ToString();
            }
            var p = new parser
            {
                tokenizer = new Tokenizer(r, contextTag),
                doc = new Node
                {
                    Type = NodeType.Document,
                },
                scripting = true,
                fragment = true,
                context = context,
            };

            var root = new Node
            {
                Type = NodeType.Element,
                DataAtom = a.Html,
                Data = a.Html.ToString(),
            };
            p.doc.AppendChild(root);
            p.oe.push(root);
            p.resetInsertionMode();

            for (var n = context; n != null; n = n.Parent)
            {
                if (n.Type == NodeType.Element && n.DataAtom == a.Form)
                {
                    p.form = n;
                    break;
                }
            }

            p.parse();

            var parent = p.doc;
            if (context != null)
            {
                parent = root;
            }

            var result = new List<Node>();
            for (var c = parent.FirstChild; c != null; )
            {
                var next = c.NextSibling;
                parent.RemoveChild(c);
                result.Add(c);
                c = next;
            }
            return result.ToArray();
        }
    }
}
