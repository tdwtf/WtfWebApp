using System;
using System.Collections.Generic;

// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// See README.txt for a link to the original source code.

namespace TheDailyWtf.Common
{
    public static partial class Html
    {
        // A NodeType is the type of a Node.
        public enum NodeType
        {
            Error,
            Text,
            Document,
            Element,
            Comment,
            Doctype,
            scopeMarker
        }

        // Section 12.2.3.3 says "scope markers are inserted when entering applet
        // elements, buttons, object elements, marquees, table cells, and table
        // captions, and are used to prevent formatting from 'leaking'".
        private static readonly Node scopeMarker = new Node { Type = NodeType.scopeMarker };

        // A Node consists of a NodeType and some Data (tag name for element nodes,
        // content for text) and are part of a tree of Nodes. Element nodes may also
        // have a Namespace and contain a slice of Attributes. Data is unescaped, so
        // that it looks like "a<b" rather than "a&lt;b". For element nodes, DataAtom
        // is the atom for Data, or zero if Data is not a known tag name.
        //
        // An empty Namespace implies a "http://www.w3.org/1999/xhtml" namespace.
        // Similarly, "math" is short for "http://www.w3.org/1998/Math/MathML", and
        // "svg" is short for "http://www.w3.org/2000/svg".
        public sealed class Node
        {
            public Node Parent { get; internal set; }
            public Node FirstChild { get; internal set; }
            public Node LastChild { get; internal set; }
            public Node PrevSibling { get; internal set; }
            public Node NextSibling { get; internal set; }

            public NodeType Type { get; set; }
            public Atom.AtomType DataAtom { get; set; }
            public string Data { get; set; } = "";
            public string Namespace { get; set; } = "";
            private readonly List<Attribute> attr = new List<Attribute>();
            public List<Attribute> Attr
            {
                get => attr;
                set
                {
                    this.attr.Clear();
                    foreach (var a in value)
                    {
                        this.attr.Add(new Attribute
                        {
                            Namespace = a.Namespace,
                            Key = a.Key,
                            Val = a.Val,
                        });
                    }
                }
            }

            // InsertBefore inserts newChild as a child of n, immediately before oldChild
            // in the sequence of n's children. oldChild may be nil, in which case newChild
            // is appended to the end of n's children.
            //
            // It will panic if newChild already has a parent or siblings.
            public void InsertBefore(Node newChild, Node oldChild)
            {
                if (newChild.Parent != null || newChild.PrevSibling != null || newChild.NextSibling != null)
                {
                    throw new ArgumentException("InsertBefore called for an attached child Node", nameof(newChild));
                }
                Node prev = null, next = null;
                if (oldChild != null)
                {
                    prev = oldChild.PrevSibling;
                    next = oldChild;
                }
                else
                {
                    prev = this.LastChild;
                }
                if (prev != null)
                {
                    prev.NextSibling = newChild;
                }
                else
                {
                    this.FirstChild = newChild;
                }
                if (next != null)
                {
                    next.PrevSibling = newChild;
                }
                else
                {
                    this.LastChild = newChild;
                }
                newChild.Parent = this;
                newChild.PrevSibling = prev;
                newChild.NextSibling = next;
            }

            // AppendChild adds a node c as a child of n.
            //
            // It will panic if c already has a parent or siblings.
            public void AppendChild(Node c)
            {
                if (c.Parent != null || c.PrevSibling != null || c.NextSibling != null)
                {
                    throw new ArgumentException("AppendChild called for an attached child Node", nameof(c));
                }
                var last = this.LastChild;
                if (last != null)
                {
                    last.NextSibling = c;
                }
                else
                {
                    this.FirstChild = c;
                }
                this.LastChild = c;
                c.Parent = this;
                c.PrevSibling = last;
            }

            // RemoveChild removes a node c that is a child of n. Afterwards, c will have
            // no parent and no siblings.
            //
            // It will panic if c's parent is not n.
            public void RemoveChild(Node c) {
                if (c.Parent != this)
                {
                    throw new ArgumentException("RemoveChild called for a non-child Node", nameof(c));
                }
                if (this.FirstChild == c)
                {
                    this.FirstChild = c.NextSibling;
                }
                if (c.NextSibling != null)
                {
                    c.NextSibling.PrevSibling = c.PrevSibling;
                }
                if (this.LastChild == c)
                {
                    this.LastChild = c.PrevSibling;
                }
                if (c.PrevSibling != null)
                {
                    c.PrevSibling.NextSibling = c.NextSibling;
                }
                c.Parent = null;
                c.PrevSibling = null;
                c.NextSibling = null;
            }

            // reparentChildren reparents all of src's child nodes to dst.
            internal static void reparentChildren(Node dst, Node src)
            {
                while (true)
                {
                    var child = src.FirstChild;
                    if (child == null)
                    {
                        break;
                    }
                    src.RemoveChild(child);
                    dst.AppendChild(child);
                }
            }

            // clone returns a new node with the same type, data and attributes.
            // The clone has no parent, no siblings and no children.
            internal Node clone()
            {
                var m = new Node
                {
                    Type = this.Type,
                    DataAtom = this.DataAtom,
                    Data = this.Data
                };
                m.Attr.AddRange(this.Attr);
                return m;
            }
        }

        // nodeStack is a stack of nodes.
        private class nodeStack
        {
            internal readonly List<Node> nodes = new List<Node>();

            public Node this[int i]
            {
                get => this.nodes[i];
                set => this.nodes[i] = value;
            }

            public nodeStack this[int i, int j]
            {
                get
                {
                    var s = new nodeStack();
                    s.nodes.AddRange(this.nodes.GetRange(i, j - i));
                    return s;
                }
            }

            public int len => this.nodes.Count;

            public void push(Node n)
            {
                this.nodes.Add(n);
            }

            // pop pops the stack. It will panic if s is empty.
            public Node pop()
            {
                var n = this.nodes[this.nodes.Count - 1];
                this.nodes.RemoveAt(this.nodes.Count - 1);
                return n;
            }

            // top returns the most recently pushed node, or nil if s is empty.
            public Node top()
            {
                if (this.nodes.Count > 0)
                {
                    return this.nodes[this.nodes.Count - 1];
                }
                return null;
            }

            // index returns the index of the top-most occurrence of n in the stack, or -1
            // if n is not present.
            public int index(Node n)
            {
                for (var i = this.nodes.Count - 1; i >= 0; i--)
                {
                    if (this.nodes[i] == n)
                    {
                        return i;
                    }
                }
                return -1;
            }

            // insert inserts a node at the given index.
            public void insert(int i, Node n)
            {
                this.nodes.Insert(i, n);
            }

            // remove removes a node from the stack. It is a no-op if n is not present.
            public void remove(Node n)
            {
                var i = this.nodes.LastIndexOf(n);
                if (i != -1)
                {
                    this.nodes.RemoveAt(i);
                }
            }
        }
    }
}
