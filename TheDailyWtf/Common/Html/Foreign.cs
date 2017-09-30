using System.Collections.Generic;

// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// See README.txt for a link to the original source code.

namespace TheDailyWtf.Common
{
    public static partial class Html
    {
        private static void adjustAttributeNames(List<Attribute> aa, Dictionary<string, string> nameMap)
        {
            foreach (var a in aa)
            {
                if (nameMap.TryGetValue(a.Key, out var newName))
                {
                    a.Key = newName;
                }
            }
        }

        private static void adjustForeignAttributes(List<Attribute> aa)
        {
            foreach (var a in aa)
            {
                if (a.Key == "" || a.Key[0] != 'x')
                {
                    continue;
                }
                switch (a.Key)
                {
                    case "xlink:actuate":
                    case "xlink:arcrole":
                    case "xlink:href":
                    case "xlink:role":
                    case "xlink:show":
                    case "xlink:title":
                    case "xlink:type":
                    case "xml:base":
                    case "xml:lang":
                    case "xml:space":
                    case "xmlns:xlink":
                        var j = a.Key.IndexOf(':');
                        a.Namespace = a.Key.Substring(0, j);
                        a.Key = a.Key.Substring(j + 1);
                        break;
                }
            }
        }

        private static bool htmlIntegrationPoint(Node n)
        {
            if (n.Type != NodeType.Element)
            {
                return false;
            }
            switch (n.Namespace)
            {
                case "math":
                    if (n.Data == "annotation-xml")
                    {
                        foreach (var a in n.Attr)
                        {
                            if (a.Key == "encoding")
                            {
                                var val = a.Val.ToLowerInvariant();
                                if (val == "text/html" || val == "application/xhtml+xml")
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    break;
                case "svg":
                    switch (n.Data)
                    {
                        case "desc":
                        case "foreignObject":
                        case "title":
                            return true;
                    }
                    break;
            }
            return false;
        }

        private static bool mathMLTextIntegrationPoint(Node n)
        {
            if (n.Namespace != "math")
            {
                return false;
            }
            switch (n.Data)
            {
                case "mi":
                case "mo":
                case "mn":
                case "ms":
                case "mtext":
                    return true;
            }
            return false;
        }

        // Section 12.2.5.5.
        private static readonly HashSet<string> breakout = new HashSet<string>
        {
            "b",
            "big",
            "blockquote",
            "body",
            "br",
            "center",
            "code",
            "dd",
            "div",
            "dl",
            "dt",
            "em",
            "embed",
            "h1",
            "h2",
            "h3",
            "h4",
            "h5",
            "h6",
            "head",
            "hr",
            "i",
            "img",
            "li",
            "listing",
            "menu",
            "meta",
            "nobr",
            "ol",
            "p",
            "pre",
            "ruby",
            "s",
            "small",
            "span",
            "strong",
            "strike",
            "sub",
            "sup",
            "table",
            "tt",
            "u",
            "ul",
            "var",
        };

        // Section 12.2.5.5.
        private static readonly Dictionary<string, string> svgTagNameAdjustments = new Dictionary<string, string>
        {
            {"altglyph", "altGlyph"},
            {"altglyphdef", "altGlyphDef"},
            {"altglyphitem", "altGlyphItem"},
            {"animatecolor", "animateColor"},
            {"animatemotion", "animateMotion"},
            {"animatetransform", "animateTransform"},
            {"clippath", "clipPath"},
            {"feblend", "feBlend"},
            {"fecolormatrix", "feColorMatrix"},
            {"fecomponenttransfer", "feComponentTransfer"},
            {"fecomposite", "feComposite"},
            {"feconvolvematrix", "feConvolveMatrix"},
            {"fediffuselighting", "feDiffuseLighting"},
            {"fedisplacementmap", "feDisplacementMap"},
            {"fedistantlight", "feDistantLight"},
            {"feflood", "feFlood"},
            {"fefunca", "feFuncA"},
            {"fefuncb", "feFuncB"},
            {"fefuncg", "feFuncG"},
            {"fefuncr", "feFuncR"},
            {"fegaussianblur", "feGaussianBlur"},
            {"feimage", "feImage"},
            {"femerge", "feMerge"},
            {"femergenode", "feMergeNode"},
            {"femorphology", "feMorphology"},
            {"feoffset", "feOffset"},
            {"fepointlight", "fePointLight"},
            {"fespecularlighting", "feSpecularLighting"},
            {"fespotlight", "feSpotLight"},
            {"fetile", "feTile"},
            {"feturbulence", "feTurbulence"},
            {"foreignobject", "foreignObject"},
            {"glyphref", "glyphRef"},
            {"lineargradient", "linearGradient"},
            {"radialgradient", "radialGradient"},
            {"textpath", "textPath"},
        };

        // Section 12.2.5.1
        private static readonly Dictionary<string, string> mathMLAttributeAdjustments = new Dictionary<string, string>
        {
            {"definitionurl", "definitionURL"},
        };

        private static readonly Dictionary<string, string> svgAttributeAdjustments = new Dictionary<string, string>
        {
            {"attributename", "attributeName"},
            {"attributetype", "attributeType"},
            {"basefrequency", "baseFrequency"},
            {"baseprofile", "baseProfile"},
            {"calcmode", "calcMode"},
            {"clippathunits", "clipPathUnits"},
            {"contentscripttype", "contentScriptType"},
            {"contentstyletype", "contentStyleType"},
            {"diffuseconstant", "diffuseConstant"},
            {"edgemode", "edgeMode"},
            {"externalresourcesrequired", "externalResourcesRequired"},
            {"filterres", "filterRes"},
            {"filterunits", "filterUnits"},
            {"glyphref", "glyphRef"},
            {"gradienttransform", "gradientTransform"},
            {"gradientunits", "gradientUnits"},
            {"kernelmatrix", "kernelMatrix"},
            {"kernelunitlength", "kernelUnitLength"},
            {"keypoints", "keyPoints"},
            {"keysplines", "keySplines"},
            {"keytimes", "keyTimes"},
            {"lengthadjust", "lengthAdjust"},
            {"limitingconeangle", "limitingConeAngle"},
            {"markerheight", "markerHeight"},
            {"markerunits", "markerUnits"},
            {"markerwidth", "markerWidth"},
            {"maskcontentunits", "maskContentUnits"},
            {"maskunits", "maskUnits"},
            {"numoctaves", "numOctaves"},
            {"pathlength", "pathLength"},
            {"patterncontentunits", "patternContentUnits"},
            {"patterntransform", "patternTransform"},
            {"patternunits", "patternUnits"},
            {"pointsatx", "pointsAtX"},
            {"pointsaty", "pointsAtY"},
            {"pointsatz", "pointsAtZ"},
            {"preservealpha", "preserveAlpha"},
            {"preserveaspectratio", "preserveAspectRatio"},
            {"primitiveunits", "primitiveUnits"},
            {"refx", "refX"},
            {"refy", "refY"},
            {"repeatcount", "repeatCount"},
            {"repeatdur", "repeatDur"},
            {"requiredextensions", "requiredExtensions"},
            {"requiredfeatures", "requiredFeatures"},
            {"specularconstant", "specularConstant"},
            {"specularexponent", "specularExponent"},
            {"spreadmethod", "spreadMethod"},
            {"startoffset", "startOffset"},
            {"stddeviation", "stdDeviation"},
            {"stitchtiles", "stitchTiles"},
            {"surfacescale", "surfaceScale"},
            {"systemlanguage", "systemLanguage"},
            {"tablevalues", "tableValues"},
            {"targetx", "targetX"},
            {"targety", "targetY"},
            {"textlength", "textLength"},
            {"viewbox", "viewBox"},
            {"viewtarget", "viewTarget"},
            {"xchannelselector", "xChannelSelector"},
            {"ychannelselector", "yChannelSelector"},
            {"zoomandpan", "zoomAndPan"},
        };
    }
}
