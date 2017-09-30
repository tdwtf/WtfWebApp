using System.Collections.Generic;

// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// See README.txt for a link to the original source code.

namespace TheDailyWtf.Common
{
    public static partial class Html
    {
        // Section 12.2.3.2 of the HTML5 specification says "The following elements
        // have varying levels of special parsing rules".
        // https://html.spec.whatwg.org/multipage/syntax.html#the-stack-of-open-elements
        private static HashSet<string> isSpecialElementMap = new HashSet<string>
        {
            "address",
            "applet",
            "area",
            "article",
            "aside",
            "base",
            "basefont",
            "bgsound",
            "blockquote",
            "body",
            "br",
            "button",
            "caption",
            "center",
            "col",
            "colgroup",
            "dd",
            "details",
            "dir",
            "div",
            "dl",
            "dt",
            "embed",
            "fieldset",
            "figcaption",
            "figure",
            "footer",
            "form",
            "frame",
            "frameset",
            "h1",
            "h2",
            "h3",
            "h4",
            "h5",
            "h6",
            "head",
            "header",
            "hgroup",
            "hr",
            "html",
            "iframe",
            "img",
            "input",
            "isindex", // The 'isindex' element has been removed, but keep it for backwards compatibility.
	        "keygen",
            "li",
            "link",
            "listing",
            "main",
            "marquee",
            "menu",
            "meta",
            "nav",
            "noembed",
            "noframes",
            "noscript",
            "object",
            "ol",
            "p",
            "param",
            "plaintext",
            "pre",
            "script",
            "section",
            "select",
            "source",
            "style",
            "summary",
            "table",
            "tbody",
            "td",
            "template",
            "textarea",
            "tfoot",
            "th",
            "thead",
            "title",
            "tr",
            "track",
            "ul",
            "wbr",
            "xmp"
        };

        private static bool isSpecialElement(Node element)
        {
            switch (element.Namespace)
            {
                case "":
                case "html":
                    return isSpecialElementMap.Contains(element.Data);
                case "svg":
                    return element.Data == "foreignObject";
            }
            return false;
        }
    }
}
