using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using atom = TheDailyWtf.Common.Html.Atom;

namespace TheDailyWtf.Common.HtmlCleaner
{
    // Config holds the settings for htmlcleaner.
    public sealed class Config
    {
        internal readonly Dictionary<atom.AtomType, Dictionary<atom.AtomType, Regex>> elem = new Dictionary<atom.AtomType, Dictionary<atom.AtomType, Regex>>();
        internal readonly HashSet<atom.AtomType> attr = new HashSet<atom.AtomType>();
        internal readonly Dictionary<string, Dictionary<string, Regex>> elemCustom = new Dictionary<string, Dictionary<string, Regex>>();
        internal readonly HashSet<string> attrCustom = new HashSet<string>();
        internal readonly HashSet<atom.AtomType> wrap = new HashSet<atom.AtomType>();
        internal readonly HashSet<string> wrapCustom = new HashSet<string>();

        // A custom URL validation function. If it is set and returns false,
        // the attribute will be removed. Called for attributes such as src
        // and href.
        public Func<Uri, bool> ValidateURL { get; set; }

        // If true, HTML comments are turned into text.
        public bool EscapeComments { get; set; }

        // Wrap text nodes in at least one tag.
        public bool WrapText { get; set; }

        // Elem ensures an element name is allowed. The receiver is returned to
        // allow call chaining.
        public Config Elem(params string[] names)
        {
            foreach (var name in names)
            {
                var a = atom.Lookup(Encoding.UTF8.GetBytes(name));
                if (a != 0)
                {
                    this.ElemAtom(a);
                    continue;
                }

                if (!this.elemCustom.ContainsKey(name))
                {
                    this.elemCustom.Add(name, new Dictionary<string, Regex>());
                }
            }

            return this;
        }

        // ElemAtom ensures an element name is allowed. The receiver is returned to
        // allow call chaining.
        public Config ElemAtom(params atom.AtomType[] elem)
        {
            foreach (var a in elem)
            {
                if (!this.elem.ContainsKey(a))
                {
                    this.elem.Add(a, new Dictionary<atom.AtomType, Regex>());
                }
            }

            return this;
        }

        // GlobalAttr allows an attribute name on all allowed elements. The
        // receiver is returned to allow call chaining.
        public Config GlobalAttr(params string[] names)
        {
            foreach (var name in names)
            {
                var a = atom.Lookup(Encoding.UTF8.GetBytes(name));
                if (a != 0)
                {
                    this.GlobalAttrAtom(a);
                    continue;
                }

                this.attrCustom.Add(name);
            }

            return this;
        }

        // GlobalAttrAtom allows an attribute name on all allowed elements. The
        // receiver is returned to allow call chaining.
        public Config GlobalAttrAtom(atom.AtomType a)
        {
            this.attr.Add(a);

            return this;
        }

        // ElemAttr allows an attribute name on the specified element. The
        // receiver is returned to allow call chaining.
        public Config ElemAttr(string elem, params string[] attr)
        {
            foreach (var a in attr)
            {
                this.ElemAttrMatch(elem, a, null);
            }
            return this;
        }

        // ElemAttrAtom allows an attribute name on the specified element. The
        // receiver is returned to allow call chaining.
        public Config ElemAttrAtom(atom.AtomType elem, params atom.AtomType[] attr)
        {
            foreach (var a in attr)
            {
                this.ElemAttrAtomMatch(elem, a, null);
            }
            return this;
        }

        // ElemAttrMatch allows an attribute name on the specified element, but
        // only if the value matches a regular expression. The receiver is returned to
        // allow call chaining.
        public Config ElemAttrMatch(string elem, string attr, Regex match)
        {
            var e = atom.Lookup(Encoding.UTF8.GetBytes(elem));
            var a = atom.Lookup(Encoding.UTF8.GetBytes(attr));
            if (e != 0 && a != 0)
            {
                return this.ElemAttrAtomMatch(e, a, match);
            }

            Dictionary<string, Regex> attrs;
            if (!this.elemCustom.TryGetValue(elem, out attrs))
            {
                attrs = new Dictionary<string, Regex>();
                this.elemCustom[elem] = attrs;
            }

            attrs[attr] = match;

            return this;
        }

        // ElemAttrAtomMatch allows an attribute name on the specified element,
        // but only if the value matches a regular expression. The receiver is returned
        // to allow call chaining.
        public Config ElemAttrAtomMatch(atom.AtomType elem, atom.AtomType attr, Regex match)
        {
            Dictionary<atom.AtomType, Regex> attrs;
            if (!this.elem.TryGetValue(elem, out attrs))
            {
                attrs = new Dictionary<atom.AtomType, Regex>();
                this.elem[elem] = attrs;
            }

            attrs[attr] = match;

            return this;
        }

        // WrapTextInside makes an element's children behave as if they are root nodes
        // in the context of WrapText. The receiver is returned to allow call chaining.
        public Config WrapTextInside(params string[] names)
        {
            foreach (var name in names)
            {
                var a = atom.Lookup(Encoding.UTF8.GetBytes(name));
                if (a != 0)
                {
                    this.WrapTextInsideAtom(a);
                    continue;
                }

                this.wrapCustom.Add(name);
            }

            return this;
        }

        // WrapTextInsideAtom makes an element's children behave as if they are root
        // nodes in the context of WrapText. The receiver is returned to allow call
        // chaining.
        public Config WrapTextInsideAtom(params atom.AtomType[] elem)
        {
            foreach (var a in elem)
            {
                this.wrap.Add(a);
            }

            return this;
        }

        // DefaultConfig is the default settings for htmlcleaner.
        public static Config DefaultConfig => (new Config
        {
            ValidateURL = Cleaner.SafeURLScheme,
        }).GlobalAttrAtom(atom.Title).
            ElemAttrAtom(atom.A, atom.Href).
            ElemAttrAtom(atom.Img, atom.Src, atom.Alt).
            ElemAttrAtom(atom.Video, atom.Src, atom.Poster, atom.Controls).
            ElemAttrAtom(atom.Audio, atom.Src, atom.Controls).
            ElemAtom(atom.B, atom.I, atom.U, atom.S).
            ElemAtom(atom.Em, atom.Strong, atom.Strike).
            ElemAtom(atom.Big, atom.Small, atom.Sup, atom.Sub).
            ElemAtom(atom.Ins, atom.Del).
            ElemAtom(atom.Abbr, atom.Address, atom.Cite, atom.Q).
            ElemAtom(atom.P, atom.Blockquote, atom.Pre).
            ElemAtom(atom.Code, atom.Kbd, atom.Tt).
            ElemAttrAtom(atom.Details, atom.Open).
            ElemAtom(atom.Summary);
    }
}