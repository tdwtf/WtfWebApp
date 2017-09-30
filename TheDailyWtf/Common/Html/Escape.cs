using System;
using System.IO;
using System.Linq;
using System.Text;

// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// See README.txt for a link to the original source code.

namespace TheDailyWtf.Common
{
    public static partial class Html
    {
        // These replacements permit compatibility with old numeric entities that
        // assumed Windows-1252 encoding.
        // https://html.spec.whatwg.org/multipage/syntax.html#consume-a-character-reference
        private static readonly char[] replacementTable = new char[]
        {
            '\u20AC', // First entry is what 0x80 should be replaced with.
            '\u0081',
            '\u201A',
            '\u0192',
            '\u201E',
            '\u2026',
            '\u2020',
            '\u2021',
            '\u02C6',
            '\u2030',
            '\u0160',
            '\u2039',
            '\u0152',
            '\u008D',
            '\u017D',
            '\u008F',
            '\u0090',
            '\u2018',
            '\u2019',
            '\u201C',
            '\u201D',
            '\u2022',
            '\u2013',
            '\u2014',
            '\u02DC',
            '\u2122',
            '\u0161',
            '\u203A',
            '\u0153',
            '\u009D',
            '\u017E',
            '\u0178', // Last entry is 0x9F.
            // 0x00->'\uFFFD' is handled programmatically.
            // 0x0D->'\u000D' is a no-op.
        };

        // unescapeEntity reads an entity like "&lt;" from b[src:] and writes the
        // corresponding "<" to b[dst:], returning the incremented dst and src cursors.
        // Precondition: b[src] == '&' && dst <= src.
        // attribute should be true if parsing an attribute value.
        private static (int dst1, int src1) unescapeEntity(byte[] b, int dst, int src, bool attribute)
        {
            // https://html.spec.whatwg.org/multipage/syntax.html#consume-a-character-reference

            // i starts at 1 because we already know that s[0] == '&'.
            int i = 1;

            if (b.Length - src <= 1)
            {
                b[dst] = b[src];
                return (dst + 1, src + 1);
            }

            if (b[src + i] == '#')
            {
                if (b.Length - src <= 3) // We need to have at least "&#.".
                {
                    b[dst] = b[src];
                    return (dst + 1, src + 1);
                }
                i++;
                var c = b[src + i];
                bool hex = false;
                if (c == 'x' || c == 'X')
                {
                    hex = true;
                    i++;
                }

                int x = 0;
                while (i < b.Length - src)
                {
                    c = b[src + i];
                    i++;
                    if (hex)
                    {
                        if ('0' <= c && c <= '9')
                        {
                            x = 16 * x + c - '0';
                            continue;
                        }
                        else if ('a' <= c && c <= 'f')
                        {
                            x = 16 * x + c - 'a' + 10;
                            continue;
                        }
                        else if ('A' <= c && c <= 'F')
                        {
                            x = 16 * x + c - 'A' + 10;
                            continue;
                        }
                    }
                    else if ('0' <= c && c <= '9')
                    {
                        x = 10 * x + c - '0';
                        continue;
                    }
                    if (c != ';')
                    {
                        i--;
                    }
                    break;
                }

                if (i <= 3) // No characters matched.
                {
                    b[dst] = b[src];
                    return (dst + 1, src + 1);
                }

                if (0x80 <= x && x <= 0x9F)
                {
                    // Replace characters from Windows-1252 with UTF-8 equivalents.
                    x = replacementTable[x - 0x80];
                }
                else if (x == 0 || (0xD800 <= x && x <= 0xDFFF) || x > 0x10FFFF)
                {
                    // Replace invalid characters with the replacement character.
                    x = '\uFFFD';
                }

                var xs = char.ConvertFromUtf32(x);
                return (dst + Encoding.UTF8.GetBytes(xs, 0, xs.Length, b, dst), src + i);
            }

            // Consume the maximum number of characters possible, with the
            // consumed characters matching one of the named references.

            while (i < b.Length - src)
            {
                var c = b[src + i];
                i++;
                // Lower-cased characters are more common in entities, so we check for them first.
                if ('a' <= c && c <= 'z' || 'A' <= c && c <= 'Z' || '0' <= c && c <= '9')
                {
                    continue;
                }
                if (c != ';')
                {
                    i--;
                }
                break;
            }

            var entityName = Encoding.UTF8.GetString(b, src + 1, i - 1);
            if (entityName == "")
            {
                // No-op.
            }
            else if (attribute && !entityName.EndsWith(";") && b.Length - src > i && b[src + i] == '=')
            {
                // No-op.
            }
            else if (entity.TryGetValue(entityName, out var x))
            {
                return (dst + Encoding.UTF8.GetBytes(x, 0, x.Length, b, dst), src + i);
            }
            else if (entity2.TryGetValue(entityName, out var x2))
            {
                return (dst + Encoding.UTF8.GetBytes(new[] { x2.first, x2.second }, 0, 2, b, dst), src + i);
            }
            else if (!attribute)
            {
                var maxLen = entityName.Length - 1;
                if (maxLen > longestEntityWithoutSemicolon)
                {
                    maxLen = longestEntityWithoutSemicolon;
                }
                for (var j = maxLen; j > 1; j--)
                {
                    if (entity.TryGetValue(entityName.Substring(0, j), out x))
                    {
                        return (dst + Encoding.UTF8.GetBytes(x, 0, x.Length, b, dst), src + j + 1);
                    }
                }
            }

            Array.Copy(b, src, b, dst, i);
            return (dst + i, src + i);
        }

        // unescape unescapes b's entities in-place, so that "a&lt;b" becomes "a<b".
        // attribute should be true if parsing an attribute value.
        private static byte[] unescape(byte[] b, bool attribute)
        {
            for (int i = 0; i < b.Length; i++)
            {
                if (b[i] == '&')
                {
                    var (dst, src) = unescapeEntity(b, i, i, attribute);
                    while (src < b.Length)
                    {
                        if (b[src] == '&')
                        {
                            (dst, src) = unescapeEntity(b, dst, src, attribute);
                        }
                        else
                        {
                            b[dst] = b[src];
                            dst++;
                            src++;
                        }
                    }
                    return b.Take(dst).ToArray();
                }
            }
            return b;
        }

        // lower lower-cases the A-Z bytes in b in-place, so that "aBc" becomes "abc".
        private static byte[] lower(byte[] b)
        {
            for (int i = 0; i < b.Length; i++)
            {
                if ('A' <= b[i] && b[i] <= 'Z')
                {
                    b[i] = (byte)(b[i] + 'a' - 'A');
                }
            }
            return b;
        }

        private static readonly char[] escapedChars = "&'<>\"\r".ToCharArray();

        private static void escape(Stream w, string s)
        {
            int i = s.IndexOfAny(escapedChars);
            while (i != -1)
            {
                w.WriteString(s.Substring(0, i));
                string esc;
                switch (s[i])
                {
                    case '&':
                        esc = "&amp;";
                        break;
                    case '\'':
                        // "&#39;" is shorter than "&apos;" and apos was not in HTML until HTML5.
                        esc = "&#39;";
                        break;
                    case '<':
                        esc = "&lt;";
                        break;
                    case '>':
                        esc = "&gt;";
                        break;
                    case '"':
                        // "&#34;" is shorter than "&quot;".
                        esc = "&#34;";
                        break;
                    case '\r':
                        esc = "&#13;";
                        break;
                    default:
                        throw new NotImplementedException("unrecognized escape character: " + s[i]);
                }
                s = s.Substring(i + 1);
                w.WriteString(esc);
                i = s.IndexOfAny(escapedChars);
            }
            w.WriteString(s);
        }

        // EscapeString escapes special characters like "<" to become "&lt;". It
        // escapes only five such characters: <, >, &, ' and ".
        // UnescapeString(EscapeString(s)) == s always holds, but the converse isn't
        // always true.
        public static string EscapeString(string s)
        {
            if (s.IndexOfAny(escapedChars) == -1)
            {
                return s;
            }
            using (var buf = new MemoryStream())
            {
                escape(buf, s);
                return Encoding.UTF8.GetString(buf.ToArray());
            }
        }

        // UnescapeString unescapes entities like "&lt;" to become "<". It unescapes a
        // larger range of entities than EscapeString escapes. For example, "&aacute;"
        // unescapes to "á", as does "&#225;" and "&xE1;".
        // UnescapeString(EscapeString(s)) == s always holds, but the converse isn't
        // always true.
        public static string UnescapeString(string s)
        {
            if (s.Contains('&'))
            {
                return Encoding.UTF8.GetString(unescape(Encoding.UTF8.GetBytes(s), false));
            }
            return s;
        }
    }
}
