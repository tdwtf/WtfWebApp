using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;

// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// See README.txt for a link to the original source code.

namespace TheDailyWtf.Common
{
    public static partial class Html
    {
        // A TokenType is the type of a Token.
        public enum TokenType
        {
            // Error means that an error occurred during tokenization.
            Error,
            // Text means a text node.
            Text,
            // A StartTag looks like <a>.
            StartTag,
            // An EndTag looks like </a>.
            EndTag,
            // A SelfClosingTag tag looks like <br/>.
            SelfClosingTag,
            // A Comment looks like <!--x-->.
            Comment,
            // A Doctype looks like <!DOCTYPE x>
            Doctype
        }

        // An Attribute is an attribute namespace-key-value triple. Namespace is
        // non-empty for foreign attributes like xlink, Key is alphabetic (and hence
        // does not contain escapable characters like '&', '<' or '>'), and Val is
        // unescaped (it looks like "a<b" rather than "a&lt;b").
        //
        // Namespace is only used by the parser, not the tokenizer.
        public sealed class Attribute
        {
            public string Namespace { get; set; } = "";
            public string Key { get; set; } = "";
            public string Val { get; set; } = "";
        }

        // A Token consists of a TokenType and some Data (tag name for start and end
        // tags, content for text, comments and doctypes). A tag Token may also contain
        // a slice of Attributes. Data is unescaped for all Tokens (it looks like "a<b"
        // rather than "a&lt;b"). For tag Tokens, DataAtom is the atom for Data, or
        // zero if Data is not a known tag name.
        public sealed class Token
        {
            public TokenType Type { get; set; }
            public Atom.AtomType DataAtom { get; set; }
            public string Data { get; set; } = "";
            public List<Attribute> Attr { get; } = new List<Attribute>();

            // tagString returns a string representation of a tag Token's Data and Attr.
            private string tagString()
            {
                if (this.Attr.Count == 0)
                {
                    return this.Data;
                }
                var buf = new MemoryStream();
                buf.WriteString(this.Data);
                foreach (var a in this.Attr)
                {
                    buf.WriteByte((byte)' ');
                    buf.WriteString(a.Key);
                    buf.WriteString("=\"");
                    escape(buf, a.Val);
                    buf.WriteByte((byte)'"');
                }
                return buf.ToString();
            }

            // ToString returns a string representation of the Token.
            public override string ToString()
            {
                switch (this.Type)
                {
                    case TokenType.Error:
                        return "";
                    case TokenType.Text:
                        return EscapeString(this.Data);
                    case TokenType.StartTag:
                        return "<" + this.tagString() + ">";
                    case TokenType.EndTag:
                        return "</" + this.tagString() + ">";
                    case TokenType.SelfClosingTag:
                        return "<" + this.tagString() + "/>";
                    case TokenType.Comment:
                        return "<!--" + this.Data + "-->";
                    case TokenType.Doctype:
                        return "<!DOCTYPE " + this.Data + ">";
                }
                return "Invalid(" + ((int)this.Type) + ")";
            }
        }

        // span is a range of bytes in a Tokenizer's buffer. The start is inclusive,
        // the end is exclusive.
        private sealed class span
        {
            public int start, end;
        }
        private sealed class span2
        {
            public span2() { }
            public span2(span2 copy)
            {
                this.first.start = copy.first.start;
                this.first.end = copy.first.end;
                this.second.start = copy.second.start;
                this.second.end = copy.second.end;
            }
            public readonly span first = new span();
            public readonly span second = new span();
        }

        // A Tokenizer returns a stream of HTML Tokens.
        public sealed class Tokenizer
        {
            // r is the source of the HTML text.
            private Stream r;
            // tt is the TokenType of the current token.
            private TokenType tt;
            // err is the first error encountered during tokenization. It is possible
            // for tt != Error && err != nil to hold: this means that Next returned a
            // valid token but the subsequent Next call will return an error token.
            // For example, if the HTML text input was just "plain", then the first
            // Next call would set z.err to io.EOF but return a TextToken, and all
            // subsequent Next calls would return an ErrorToken.
            // err is never reset. Once it becomes non-nil, it stays non-nil.
            private ExceptionDispatchInfo err;
            // readErr is the error returned by the io.Reader r. It is separate from
            // err because it is valid for an io.Reader to return (n int, err1 error)
            // such that n > 0 && err1 != nil, and callers should always process the
            // n > 0 bytes before considering the error err1.
            private ExceptionDispatchInfo readErr;
            // buf[raw.start:raw.end] holds the raw bytes of the current token.
            // buf[raw.end:] is buffered input that will yield future tokens.
            private span raw = new span();
            private byte[] buf;
            private int bufLength;
            // maxBuf limits the data buffered in buf. A value of 0 means unlimited.
            private int maxBuf;
            // buf[data.start:data.end] holds the raw bytes of the current token's data:
            // a text token's text, a tag token's tag name, etc.
            private span data = new span();
            // pendingAttr is the attribute key and value currently being tokenized.
            // When complete, pendingAttr is pushed onto attr. nAttrReturned is
            // incremented on each call to TagAttr.
            private span2 pendingAttr = new span2();
            private readonly List<span2> attr = new List<span2>();
            private int nAttrReturned;
            // rawTag is the "script" in "</script>" that closes the next token. If
            // non-empty, the subsequent call to Next will return a raw or RCDATA text
            // token: one that treats "<p>" as text instead of an element.
            // rawTag's contents are lower-cased.
            private string rawTag = "";
            // textIsRaw is whether the current text token's data is not escaped.
            private bool textIsRaw;
            // convertNUL is whether NUL bytes in the current token's data should
            // be converted into \ufffd replacement characters.
            private bool convertNUL;
            // allowCDATA is whether CDATA sections are allowed in the current context.
            private bool allowCDATA;

            // AllowCDATA sets whether or not the tokenizer recognizes <![CDATA[foo]]> as
            // the text "foo". The default value is false, which means to recognize it as
            // a bogus comment "<!-- [CDATA[foo]] -->" instead.
            //
            // Strictly speaking, an HTML5 compliant tokenizer should allow CDATA if and
            // only if tokenizing foreign content, such as MathML and SVG. However,
            // tracking foreign-contentness is difficult to do purely in the tokenizer,
            // as opposed to the parser, due to HTML integration points: an <svg> element
            // can contain a <foreignObject> that is foreign-to-SVG but not foreign-to-
            // HTML. For strict compliance with the HTML5 tokenization algorithm, it is the
            // responsibility of the user of a tokenizer to call AllowCDATA as appropriate.
            // In practice, if using the tokenizer without caring whether MathML or SVG
            // CDATA is text or comments, such as tokenizing HTML to find all the anchor
            // text, it is acceptable to ignore this responsibility.
            public bool AllowCDATA
            {
                set
                {
                    this.allowCDATA = value;
                }
            }

            // NextIsNotRawText instructs the tokenizer that the next token should not be
            // considered as 'raw text'. Some elements, such as script and title elements,
            // normally require the next token after the opening tag to be 'raw text' that
            // has no child elements. For example, tokenizing "<title>a<b>c</b>d</title>"
            // yields a start tag token for "<title>", a text token for "a<b>c</b>d", and
            // an end tag token for "</title>". There are no distinct start tag or end tag
            // tokens for the "<b>" and "</b>".
            //
            // This tokenizer implementation will generally look for raw text at the right
            // times. Strictly speaking, an HTML5 compliant tokenizer should not look for
            // raw text if in foreign content: <title> generally needs raw text, but a
            // <title> inside an <svg> does not. Another example is that a <textarea>
            // generally needs raw text, but a <textarea> is not allowed as an immediate
            // child of a <select>; in normal parsing, a <textarea> implies </select>, but
            // one cannot close the implicit element when parsing a <select>'s InnerHTML.
            // Similarly to AllowCDATA, tracking the correct moment to override raw-text-
            // ness is difficult to do purely in the tokenizer, as opposed to the parser.
            // For strict compliance with the HTML5 tokenization algorithm, it is the
            // responsibility of the user of a tokenizer to call NextIsNotRawText as
            // appropriate. In practice, like AllowCDATA, it is acceptable to ignore this
            // responsibility for basic usage.
            //
            // Note that this 'raw text' concept is different from the one offered by the
            // Tokenizer.Raw method.
            public void NextIsNotRawText()
            {
                this.rawTag = "";
            }

            // Err returns the error associated with the most recent ErrorToken token.
            // This is typically io.EOF, meaning the end of tokenization.
            public void ThrowErr()
            {
                if (this.tt == TokenType.Error)
                {
                    this.err?.Throw();
                }
            }

            // readByte returns the next byte from the input stream, doing a buffered read
            // from z.r into z.buf if necessary. z.buf[z.raw.start:z.raw.end] remains a contiguous byte
            // slice that holds all the bytes read so far for the current token.
            // It sets z.err if the underlying reader returns an error.
            // Pre-condition: z.err == nil.
            private byte readByte()
            {
                if (this.raw.end >= this.bufLength)
                {
                    // Our buffer is exhausted and we have to read from z.r. Check if the
                    // previous read resulted in an error.
                    if (this.readErr != null)
                    {
                        this.err = this.readErr;
                        return 0;
                    }
                    // We copy z.buf[z.raw.start:z.raw.end] to the beginning of z.buf. If the length
                    // z.raw.end - z.raw.start is more than half the capacity of z.buf, then we
                    // allocate a new buffer before the copy.
                    var c = this.buf.Length;
                    var d = this.raw.end - this.raw.start;
                    var x = this.raw.start;
                    var buf1 = 2 * d > c ? new byte[2 * c] : this.buf;
                    Array.Copy(this.buf, x, buf1, 0, d);
                    if (x != 0)
                    {
                        // Adjust the data/attr spans to refer to the same contents after the copy.
                        this.data.start -= x;
                        this.data.end -= x;
                        this.pendingAttr.first.start -= x;
                        this.pendingAttr.first.end -= x;
                        this.pendingAttr.second.start -= x;
                        this.pendingAttr.second.end -= x;
                        foreach (var a in this.attr)
                        {
                            a.first.start -= x;
                            a.first.end -= x;
                            a.second.start -= x;
                            a.second.end -= x;
                        }
                    }
                    this.raw.start = 0;
                    this.raw.end = d;
                    this.buf = buf1;
                    this.bufLength = d;
                    // Now that we have copied the live bytes to the start of the buffer,
                    // we read from z.r into the remainder.
                    int n = 0;
                    try
                    {
                        n = readAtLeastOneByte(this.r, buf1, d, buf1.Length);
                    }
                    catch (Exception ex)
                    {
                        this.readErr = ExceptionDispatchInfo.Capture(ex);
                    }
                    if (n == 0)
                    {
                        this.err = this.readErr;
                        return 0;
                    }
                    this.buf = buf1;
                    this.bufLength = d + n;
                }
                var next = this.buf[this.raw.end];
                this.raw.end++;
                if (this.maxBuf > 0 && this.raw.end - this.raw.start >= this.maxBuf)
                {
                    this.err = ExceptionDispatchInfo.Capture(new ArgumentOutOfRangeException("maximum buffer length exceeded"));
                    return 0;
                }
                return next;
            }

            // Buffered returns a slice containing data buffered but not yet tokenized.

            public byte[] Buffered
            {
                get
                {
                    var b = new byte[this.raw.end - this.bufLength];
                    Array.Copy(this.buf, this.raw.end, b, 0, b.Length);
                    return b;
                }
            }

            // readAtLeastOneByte wraps an io.Reader so that reading cannot return (0, nil).
            // It returns io.ErrNoProgress if the underlying r.Read method returns (0, nil)
            // too many times in succession.
            private static int readAtLeastOneByte(Stream r, byte[] b, int start, int end)
            {
                var n = r.Read(b, start, end - start);
                if (n != 0)
                {
                    return n;
                }
                throw new EndOfStreamException();
            }

            // skipWhiteSpace skips past any white space.
            private void skipWhiteSpace()
            {
                if (this.err != null)
                {
                    return;
                }

                while (true)
                {
                    var c = this.readByte();
                    if (this.err != null)
                    {
                        return;
                    }

                    switch (c)
                    {
                        case (byte)' ':
                        case (byte)'\n':
                        case (byte)'\r':
                        case (byte)'\t':
                        case (byte)'\f':
                            break; // No-op.
                        default:
                            this.raw.end--;
                            return;
                    }
                }
            }

            // readRawOrRCDATA reads until the next "</foo>", where "foo" is z.rawTag and
            // is typically something like "script" or "textarea".
            private void readRawOrRCDATA()
            {
                if (this.rawTag == "script")
                {
                    this.readScript();
                    this.textIsRaw = true;
                    this.rawTag = "";
                    return;
                }

                while (true)
                {
                    var c = this.readByte();
                    if (this.err != null)
                    {
                        break;
                    }
                    if (c != '<')
                    {
                        continue;
                    }
                    c = this.readByte();
                    if (this.err != null)
                    {
                        break;
                    }
                    if (c != '/')
                    {
                        continue;
                    }
                    if (this.readRawEndTag() || this.err != null)
                    {
                        break;
                    }
                }

                this.data.end = this.raw.end;

                // A textarea's or title's RCDATA can contain escaped entities.
                this.textIsRaw = this.rawTag != "textarea" && this.rawTag != "title";
                this.rawTag = "";
            }

            // readRawEndTag attempts to read a tag like "</foo>", where "foo" is z.rawTag.
            // If it succeeds, it backs up the input position to reconsume the tag and
            // returns true. Otherwise it returns false. The opening "</" has already been
            // consumed.
            private bool readRawEndTag()
            {
                byte c;
                var rawTagBytes = Encoding.UTF8.GetBytes(this.rawTag);
                for (int i = 0; i < rawTagBytes.Length; i++)
                {
                    c = this.readByte();
                    if (this.err != null)
                    {
                        return false;
                    }
                    if (c != rawTagBytes[i] && c != rawTagBytes[i] - ((byte)('a') - (byte)('A')))
                    {
                        this.raw.end--;
                        return false;
                    }
                }
                c = this.readByte();
                if (this.err != null)
                {
                    return false;
                }
                switch (c)
                {
                    case (byte)' ':
                    case (byte)'\n':
                    case (byte)'\r':
                    case (byte)'\t':
                    case (byte)'\f':
                    case (byte)'/':
                    case (byte)'>':
                        // The 3 is 2 for the leading "</" plus 1 for the trailing character c.
                        this.raw.end -= 3 + rawTagBytes.Length;
                        return true;
                }
                this.raw.end--;
                return false;
            }

            // readScript reads until the next </script> tag, following the byzantine
            // rules for escaping/hiding the closing tag.
            private void readScript()
            {
                try
                {
                    byte c;

                    scriptData:
                    c = this.readByte();
                    if (this.err != null)
                    {
                        return;
                    }
                    if (c == '<')
                    {
                        goto scriptDataLessThanSign;
                    }
                    goto scriptData;

                    scriptDataLessThanSign:
                    c = this.readByte();
                    if (this.err != null)
                    {
                        return;
                    }
                    switch (c)
                    {
                        case (byte)'/':
                            goto scriptDataEndTagOpen;
                        case (byte)'!':
                            goto scriptDataEscapeStart;
                    }
                    this.raw.end--;
                    goto scriptData;

                    scriptDataEndTagOpen:
                    if (this.readRawEndTag() || this.err != null)
                    {
                        return;
                    }
                    goto scriptData;

                    scriptDataEscapeStart:
                    c = this.readByte();
                    if (this.err != null)
                    {
                        return;
                    }
                    if (c == '-')
                    {
                        goto scriptDataEscapeStartDash;
                    }
                    this.raw.end--;
                    goto scriptData;

                    scriptDataEscapeStartDash:
                    c = this.readByte();
                    if (this.err != null)
                    {
                        return;
                    }
                    if (c == '-')
                    {
                        goto scriptDataEscapedDashDash;
                    }
                    this.raw.end--;
                    goto scriptData;

                    scriptDataEscaped:
                    c = this.readByte();
                    if (this.err != null)
                    {
                        return;
                    }
                    switch (c)
                    {
                        case (byte)'-':
                            goto scriptDataEscapedDash;
                        case (byte)'<':
                            goto scriptDataEscapedLessThanSign;
                    }
                    goto scriptDataEscaped;

                    scriptDataEscapedDash:
                    c = this.readByte();
                    if (this.err != null)
                    {
                        return;
                    }
                    switch (c)
                    {
                        case (byte)'-':
                            goto scriptDataEscapedDashDash;
                        case (byte)'<':
                            goto scriptDataEscapedLessThanSign;
                    }
                    goto scriptDataEscaped;

                    scriptDataEscapedDashDash:
                    c = this.readByte();
                    if (this.err != null)
                    {
                        return;
                    }
                    switch (c)
                    {
                        case (byte)'-':
                            goto scriptDataEscapedDashDash;
                        case (byte)'<':
                            goto scriptDataEscapedLessThanSign;
                        case (byte)'>':
                            goto scriptData;
                    }
                    goto scriptDataEscaped;

                    scriptDataEscapedLessThanSign:
                    c = this.readByte();
                    if (this.err != null)
                    {
                        return;
                    }
                    if (c == '/')
                    {
                        goto scriptDataEscapedEndTagOpen;
                    }
                    if (('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z'))
                    {
                        goto scriptDataDoubleEscapeStart;
                    }
                    this.raw.end--;
                    goto scriptData;

                    scriptDataEscapedEndTagOpen:
                    if (this.readRawEndTag() || this.err != null)
                    {
                        return;
                    }
                    goto scriptDataEscaped;

                    scriptDataDoubleEscapeStart:
                    this.raw.end--;
                    for (int i = 0; i < "script".Length; i++)
                    {
                        c = this.readByte();
                        if (this.err != null)
                        {
                            return;
                        }
                        if (c != "script"[i] && c != "SCRIPT"[i])
                        {
                            this.raw.end--;
                            goto scriptDataEscaped;
                        }
                    }
                    c = this.readByte();
                    if (this.err != null)
                    {
                        return;
                    }
                    switch (c)
                    {
                        case (byte)' ':
                        case (byte)'\n':
                        case (byte)'\r':
                        case (byte)'\t':
                        case (byte)'\f':
                        case (byte)'/':
                        case (byte)'>':
                            goto scriptDataDoubleEscaped;
                    }
                    this.raw.end--;
                    goto scriptDataEscaped;

                    scriptDataDoubleEscaped:
                    c = this.readByte();
                    if (this.err != null)
                    {
                        return;
                    }
                    switch (c)
                    {
                        case (byte)'-':
                            goto scriptDataDoubleEscapedDash;
                        case (byte)'<':
                            goto scriptDataDoubleEscapedLessThanSign;
                    }
                    goto scriptDataDoubleEscaped;

                    scriptDataDoubleEscapedDash:
                    c = this.readByte();
                    if (this.err != null)
                    {
                        return;
                    }
                    switch (c)
                    {
                        case (byte)'-':
                            goto scriptDataDoubleEscapedDashDash;
                        case (byte)'<':
                            goto scriptDataDoubleEscapedLessThanSign;
                    }
                    goto scriptDataDoubleEscaped;

                    scriptDataDoubleEscapedDashDash:
                    c = this.readByte();
                    if (this.err != null)
                    {
                        return;
                    }
                    switch (c)
                    {
                        case (byte)'-':
                            goto scriptDataDoubleEscapedDashDash;
                        case (byte)'<':
                            goto scriptDataDoubleEscapedLessThanSign;
                        case (byte)'>':
                            goto scriptData;
                    }
                    goto scriptDataDoubleEscaped;

                    scriptDataDoubleEscapedLessThanSign:
                    c = this.readByte();
                    if (this.err != null)
                    {
                        return;
                    }
                    if (c == '/')
                    {
                        goto scriptDataDoubleEscapeEnd;
                    }
                    this.raw.end--;
                    goto scriptDataDoubleEscaped;

                    scriptDataDoubleEscapeEnd:
                    if (this.readRawEndTag())
                    {
                        this.raw.end += "</script>".Length;
                        goto scriptDataEscaped;
                    }
                    if (this.err != null)
                    {
                        return;
                    }
                    goto scriptDataDoubleEscaped;
                }
                finally
                {
                    this.data.end = this.raw.end;
                }
            }

            // readComment reads the next comment token starting with "<!--". The opening
            // "<!--" has already been consumed.
            private void readComment()
            {
                this.data.start = this.raw.end;
                try
                {
                    for (int dashCount = 2; ;)
                    {
                        var c = this.readByte();
                        if (this.err != null)
                        {
                            // Ignore up to two dashes at EOF.
                            if (dashCount > 2)
                            {
                                dashCount = 2;
                            }
                            this.data.end = this.raw.end - dashCount;
                            return;
                        }
                        switch (c)
                        {
                            case (byte)'-':
                                dashCount++;
                                continue;
                            case (byte)'>':
                                if (dashCount >= 2)
                                {
                                    this.data.end = this.raw.end - "-->".Length;
                                    return;
                                }
                                break;
                            case (byte)'!':
                                if (dashCount >= 2)
                                {
                                    c = this.readByte();
                                    if (this.err != null)
                                    {
                                        this.data.end = this.raw.end;
                                        return;
                                    }
                                    if (c == '>')
                                    {
                                        this.data.end = this.raw.end - "--!>".Length;
                                        return;
                                    }
                                }
                                break;
                        }
                        dashCount = 0;
                    }
                }
                finally
                {
                    if (this.data.end < this.data.start)
                    {
                        // It's a comment with no data, like <!-->.
                        this.data.end = this.data.start;
                    }
                }
            }

            // readUntilCloseAngle reads until the next ">".
            private void readUntilCloseAngle()
            {
                this.data.start = this.raw.end;
                while (true)
                {
                    var c = this.readByte();
                    if (this.err != null)
                    {
                        this.data.end = this.raw.end;
                        return;
                    }
                    if (c == '>')
                    {
                        this.data.end = this.raw.end - ">".Length;
                        return;
                    }
                }
            }

            // readMarkupDeclaration reads the next token starting with "<!". It might be
            // a "<!--comment-->", a "<!DOCTYPE foo>", a "<![CDATA[section]]>" or
            // "<!a bogus comment". The opening "<!" has already been consumed.
            private TokenType readMarkupDeclaration()
            {
                this.data.start = this.raw.end;
                var c0 = this.readByte();
                if (this.err != null)
                {
                    this.data.end = this.raw.end;
                    return TokenType.Comment;
                }
                var c1 = this.readByte();
                if (this.err != null)
                {
                    this.data.end = this.raw.end;
                    return TokenType.Comment;
                }
                if (c0 == '-' && c1 == '-')
                {
                    this.readComment();
                    return TokenType.Comment;
                }
                this.raw.end -= 2;
                if (this.readDoctype())
                {
                    return TokenType.Doctype;
                }
                if (this.allowCDATA && this.readCDATA())
                {
                    this.convertNUL = true;
                    return TokenType.Text;
                }
                // It's a bogus comment.
                this.readUntilCloseAngle();
                return TokenType.Comment;
            }

            // readDoctype attempts to read a doctype declaration and returns true if
            // successful. The opening "<!" has already been consumed.
            private bool readDoctype()
            {
                const string s = "DOCTYPE";
                for (var i = 0; i < s.Length; i++)
                {
                    var c = this.readByte();
                    if (this.err != null)
                    {
                        this.data.end = this.raw.end;
                        return false;
                    }
                    if (c != s[i] && c != s[i] + ('a' - 'A'))
                    {
                        // Back up to read the fragment of "DOCTYPE" again.
                        this.raw.end = this.data.start;
                        return false;
                    }
                }
                this.skipWhiteSpace();
                if (this.err != null)
                {
                    this.data.start = this.raw.end;
                    this.data.end = this.raw.end;
                    return true;
                }
                this.readUntilCloseAngle();
                return true;
            }

            // readCDATA attempts to read a CDATA section and returns true if
            // successful. The opening "<!" has already been consumed.
            private bool readCDATA()
            {
                const string s = "[CDATA[";
                for (int i = 0; i < s.Length; i++)
                {
                    var c = this.readByte();
                    if (this.err != null)
                    {
                        this.data.end = this.raw.end;
                        return false;
                    }
                    if (c != s[i])
                    {
                        // Back up to read the fragment of "[CDATA[" again.
                        this.raw.end = this.data.start;
                        return false;
                    }
                }
                this.data.start = this.raw.end;
                int brackets = 0;
                while (true)
                {
                    var c = this.readByte();
                    if (this.err != null)
                    {
                        this.data.end = this.raw.end;
                        return true;
                    }
                    switch (c)
                    {
                        case (byte)']':
                            brackets++;
                            break;
                        case (byte)'>':
                            if (brackets >= 2)
                            {
                                this.data.end = this.raw.end - "]]>".Length;
                                return true;
                            }
                            brackets = 0;
                            break;
                        default:
                            brackets = 0;
                            break;
                    }
                }
            }

            // startTagIn returns whether the start tag in z.buf[z.data.start:z.data.end]
            // case-insensitively matches any element of ss.
            private bool startTagIn(params string[] ss)
            {
                foreach (var s in ss)
                {
                    var b = Encoding.UTF8.GetBytes(s);
                    if (this.data.end - this.data.start != b.Length)
                    {
                        continue;
                    }
                    bool match = true;
                    for (var i = 0; i < b.Length; i++)
                    {
                        var c = this.buf[this.data.start + i];
                        if ('A' <= c && c <= 'Z')
                        {
                            c += 'a' - 'A';
                        }
                        if (c != s[i])
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                    {
                        return true;
                    }
                }
                return false;
            }

            // readStartTag reads the next start tag token. The opening "<a" has already
            // been consumed, where 'a' means anything in [A-Za-z].
            private TokenType readStartTag()
            {
                this.readTag(true);
                if (this.err != null)
                {
                    return TokenType.Error;
                }
                // Several tags flag the tokenizer's next token as raw.
                var c = this.buf[this.data.start];
                bool raw = false;
                if ('A' <= c && c <= 'Z')
                {
                    c += 'a' - 'A';
                }
                switch (c)
                {
                    case (byte)'i':
                        raw = this.startTagIn("iframe");
                        break;
                    case (byte)'n':
                        raw = this.startTagIn("noembed", "noframes", "noscript");
                        break;
                    case (byte)'p':
                        raw = this.startTagIn("plaintext");
                        break;
                    case (byte)'s':
                        raw = this.startTagIn("script", "style");
                        break;
                    case (byte)'t':
                        raw = this.startTagIn("textarea", "title");
                        break;
                    case (byte)'x':
                        raw = this.startTagIn("xmp");
                        break;
                }
                if (raw)
                {
                    this.rawTag = Encoding.UTF8.GetString(this.buf, this.data.start, this.data.end - this.data.start).ToLowerInvariant();
                }
                // Look for a self-closing token like "<br/>".
                if (this.err == null && this.buf[this.raw.end - 2] == '/')
                {
                    return TokenType.SelfClosingTag;
                }
                return TokenType.StartTag;
            }

            // readTag reads the next tag token and its attributes. If saveAttr, those
            // attributes are saved in z.attr, otherwise z.attr is set to an empty slice.
            // The opening "<a" or "</a" has already been consumed, where 'a' means anything
            // in [A-Za-z].
            private void readTag(bool saveAttr)
            {
                this.attr.Clear();
                this.nAttrReturned = 0;
                // Read the tag name and attribute key/value pairs.
                this.readTagName();
                this.skipWhiteSpace();
                if (this.err != null)
                {
                    return;
                }
                while (true)
                {
                    var c = this.readByte();
                    if (this.err != null || c == '>')
                    {
                        break;
                    }
                    this.raw.end--;
                    this.readTagAttrKey();
                    this.readTagAttrVal();
                    // Save pendingAttr if saveAttr and that attribute has a non-empty key.
                    if (saveAttr && this.pendingAttr.first.start != this.pendingAttr.first.end)
                    {
                        this.attr.Add(new span2(this.pendingAttr));
                    }
                    this.skipWhiteSpace();
                    if (this.err != null)
                    {
                        break;
                    }
                }
            }

            // readTagName sets z.data to the "div" in "<div k=v>". The reader (z.raw.end)
            // is positioned such that the first byte of the tag name (the "d" in "<div")
            // has already been consumed.
            private void readTagName()
            {
                this.data.start = this.raw.end - 1;
	            while (true)
                {
                    var c = this.readByte();
                    if (this.err != null)
                    {
                        this.data.end = this.raw.end;
                        return;
                    }
                    switch (c) {
                        case (byte)' ':
                        case (byte)'\n':
                        case (byte)'\r':
                        case (byte)'\t':
                        case (byte)'\f':
                            this.data.end = this.raw.end - 1;
                            return;
                        case (byte)'/':
                        case (byte)'>':
                            this.raw.end--;
                            this.data.end = this.raw.end;
                            return;
                    }
                }
            }

            // readTagAttrKey sets z.pendingAttr[0] to the "k" in "<div k=v>".
            // Precondition: z.err == nil.
            private void readTagAttrKey()
            {
                this.pendingAttr.first.start = this.raw.end;
                while (true)
                {
                    var c = this.readByte();
                    if (this.err != null)
                    {
                        this.pendingAttr.first.end = this.raw.end;
                        return;
                    }
                    switch (c)
                    {
                        case (byte)' ':
                        case (byte)'\n':
                        case (byte)'\r':
                        case (byte)'\t':
                        case (byte)'\f':
                        case (byte)'/':
                            this.pendingAttr.first.end = this.raw.end - 1;
                            return;
                        case (byte)'=':
                        case (byte)'>':
                            this.raw.end--;
                            this.pendingAttr.first.end = this.raw.end;
                            return;
                    }
                }
            }

            // readTagAttrVal sets z.pendingAttr[1] to the "v" in "<div k=v>".
            private void readTagAttrVal()
            {
                this.pendingAttr.second.start = this.raw.end;
                this.pendingAttr.second.end = this.raw.end;
                this.skipWhiteSpace();
                if (this.err != null)
                {
                    return;
                }
                var c = this.readByte();
                if (this.err != null)
                {
                    return;
                }
                if (c != '=')
                {
                    this.raw.end--;
                    return;
                }
                this.skipWhiteSpace();
                if (this.err != null)
                {
                    return;
                }
                var quote = this.readByte();
                if (this.err != null)
                {
                    return;
                }
	            switch (quote) {
                    case (byte)'>':
                        this.raw.end--;
                        return;

                    case (byte)'\'':
                    case (byte)'"':
                        this.pendingAttr.second.start = this.raw.end;
                        while (true)
                        {
                            c = this.readByte();
                            if (this.err != null)
                            {
                                this.pendingAttr.second.end = this.raw.end;
                                return;
                            }
                            if (c == quote)
                            {
                                this.pendingAttr.second.end = this.raw.end - 1;
                                return;
                            }
                        }

                    default:
                        this.pendingAttr.second.start = this.raw.end - 1;
                        while (true)
                        {
                            c = this.readByte();
                            if (this.err != null)
                            {
                                this.pendingAttr.second.end = this.raw.end;
                                return;
                            }
                            switch (c)
                            {
                                case (byte)' ':
                                case (byte)'\n':
                                case (byte)'\r':
                                case (byte)'\t':
                                case (byte)'\f':
                                    this.pendingAttr.second.end = this.raw.end - 1;
                                    return;
                                case (byte)'>':
                                    this.raw.end--;
                                    this.pendingAttr.second.end = this.raw.end;
                                    return;
                            }
                        }
                }
            }

            // Next scans the next token and returns its type.
            public TokenType Next()
            {
                this.raw.start = this.raw.end;
                this.data.start = this.raw.end;
                this.data.end = this.raw.end;
                if (this.err != null)
                {
                    this.tt = TokenType.Error;
                    return this.tt;
                }
                if (this.rawTag != "")
                {
                    if (this.rawTag == "plaintext")
                    {
                        // Read everything up to EOF.
                        while (this.err == null)
                        {
                            this.readByte();
                        }
                        this.data.end = this.raw.end;
                        this.textIsRaw = true;
                    }
                    else
                    {
                        this.readRawOrRCDATA();
                    }
                    if (this.data.end > this.data.start)
                    {
                        this.tt = TokenType.Text;
                        this.convertNUL = true;
                        return this.tt;
                    }
                }
                this.textIsRaw = false;
                this.convertNUL = false;

                while (true)
                {
                    var c = this.readByte();
                    if (this.err != null)
                    {
                        break;
                    }
                    if (c != '<')
                    {
                        continue;
                    }

                    // Check if the '<' we have just read is part of a tag, comment
                    // or doctype. If not, it's part of the accumulated text token.
                    c = this.readByte();
                    if (this.err != null)
                    {
                        break;
                    }
                    TokenType tokenType;
                    if (('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z'))
                    {
                        tokenType = TokenType.StartTag;
                    }
                    else if (c == '/')
                    {
                        tokenType = TokenType.EndTag;
                    }
                    else if (c == '!' || c == '?')
                    {
                        // We use CommentToken to mean any of "<!--actual comments-->",
                        // "<!DOCTYPE declarations>" and "<?xml processing instructions?>".
                        tokenType = TokenType.Comment;
                    }
                    else
                    {
                        // Reconsume the current character.
                        this.raw.end--;
                        continue;
                    }

                    // We have a non-text token, but we might have accumulated some text
                    // before that. If so, we return the text first, and return the non-
                    // text token on the subsequent call to Next.
                    var x = this.raw.end - "<a".Length;
                    if (this.raw.start < x)
                    {
                        this.raw.end = x;
                        this.data.end = x;
                        this.tt = TokenType.Text;
                        return this.tt;
                    }
                    switch (tokenType)
                    {
                        case TokenType.StartTag:
                            this.tt = this.readStartTag();
                            return this.tt;
                        case TokenType.EndTag:
                            c = this.readByte();
                            if (this.err != null)
                            {
                                goto breakLoop;
                            }
                            if (c == '>')
                            {
                                // "</>" does not generate a token at all. Generate an empty comment
                                // to allow passthrough clients to pick up the data using Raw.
                                // Reset the tokenizer state and start again.
                                this.tt = TokenType.Comment;
                                return this.tt;
                            }
                            if (('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z'))
                            {
                                this.readTag(false);
                                if (this.err != null)
                                {
                                    this.tt = TokenType.Error;
                                }
                                else
                                {
                                    this.tt = TokenType.EndTag;
                                }
                                return this.tt;
                            }
                            this.raw.end--;
                            this.readUntilCloseAngle();
                            this.tt = TokenType.Comment;
                            return this.tt;

                        case TokenType.Comment:
                            if (c == '!')
                            {
                                this.tt = this.readMarkupDeclaration();
                                return this.tt;
                            }
                            this.raw.end--;
                            this.readUntilCloseAngle();
                            this.tt = TokenType.Comment;
                            return this.tt;
                    }
                }
                breakLoop:
                if (this.raw.start < this.raw.end)
                {
                    this.data.end = this.raw.end;
                    this.tt = TokenType.Text;
                    return this.tt;
                }
                this.tt = TokenType.Error;
                return this.tt;
            }

            // Raw returns the unmodified text of the current token. Calling Next, Token,
            // Text, TagName or TagAttr may change the contents of the returned slice.
            public byte[] Raw
            {
                get
                {
                    var b = new byte[this.raw.end - this.raw.start];
                    Array.Copy(this.buf, this.raw.start, b, 0, b.Length);
                    return b;
                }
            }

            // convertNewlines converts "\r" and "\r\n" in s to "\n".
            // The conversion happens in place, but the resulting slice may be shorter.
            private byte[] convertNewlines(byte[] s)
            {
                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] != '\r')
                    {
                        continue;
                    }

                    var src = i + 1;
                    if (src >= s.Length || s[src] != '\n')
                    {
                        s[i] = (byte)'\n';
                        continue;
                    }

                    var dst = i;
                    while (src < s.Length)
                    {
                        if (s[src] == '\r')
                        {
                            if (src + 1 < s.Length && s[src + 1] == '\n')
                            {
                                src++;
                            }
                            s[dst] = (byte)'\n';
                        }
                        else
                        {
                            s[dst] = s[src];
                        }
                        src++;
                        dst++;
                    }
                    return s.Take(dst).ToArray();
                }
                return s;
            }

            private static readonly byte[] replacement = Encoding.UTF8.GetBytes("\ufffd");

            // Text returns the unescaped text of a text, comment or doctype token. The
            // contents of the returned slice may change on the next call to Next.
            internal byte[] Text()
            {
                switch (this.tt)
                {
                    case TokenType.Text:
                    case TokenType.Comment:
                    case TokenType.Doctype:
                        var s = new byte[this.data.end - this.data.start];
                        Array.Copy(this.buf, this.data.start, s, 0, s.Length);
                        this.data.start = this.raw.end;
                        this.data.end = this.raw.end;
                        s = convertNewlines(s);
                        if ((this.convertNUL || this.tt == TokenType.Comment) && s.Contains((byte)0))
                        {
                            s = s.SelectMany(b => b == 0 ? replacement : new[] { b }).ToArray();
                        }
                        if (!this.textIsRaw)
                        {
                            s = unescape(s, false);
                        }
                        return s;
                }
                return null;
            }

            // TagName returns the lower-cased name of a tag token (the `img` out of
            // `<IMG SRC="foo">`) and whether the tag has attributes.
            // The contents of the returned slice may change on the next call to Next.
            internal (byte[] name, bool hasAttr) TagName()
            {
                if (this.data.start < this.data.end)
                {
                    switch (this.tt)
                    {
                        case TokenType.StartTag:
                        case TokenType.EndTag:
                        case TokenType.SelfClosingTag:
                            var s = new byte[this.data.end - this.data.start];
                            Array.Copy(this.buf, this.data.start, s, 0, s.Length);
                            this.data.start = this.raw.end;
                            this.data.end = this.raw.end;
                            return (lower(s), this.nAttrReturned < this.attr.Count);
                    }
                }
                return (null, false);
            }

            // TagAttr returns the lower-cased key and unescaped value of the next unparsed
            // attribute for the current tag token and whether there are more attributes.
            // The contents of the returned slices may change on the next call to Next.
            internal (byte[] key, byte[] val, bool moreAttr) TagAttr()
            {
                if (this.nAttrReturned < this.attr.Count)
                {
                    switch (this.tt)
                    {
                        case TokenType.StartTag:
                        case TokenType.SelfClosingTag:
                            var x = this.attr[this.nAttrReturned];
                            this.nAttrReturned++;
                            var key = new byte[x.first.end - x.first.start];
                            Array.Copy(this.buf, x.first.start, key, 0, key.Length);
                            var val = new byte[x.second.end - x.second.start];
                            Array.Copy(this.buf, x.second.start, val, 0, val.Length);
                            return (lower(key), unescape(convertNewlines(val), true), this.nAttrReturned < this.attr.Count);
                    }
                }
                return (null, null, false);
            }

            // Token returns the next Token. The result's Data and Attr values remain valid
            // after subsequent Next calls.
            public Token Token()
            {
                var t = new Token
                {
                    Type = this.tt
                };
                switch (this.tt)
                {
                    case TokenType.Text:
                    case TokenType.Comment:
                    case TokenType.Doctype:
                        t.Data = Encoding.UTF8.GetString(this.Text());
                        break;
                    case TokenType.StartTag:
                    case TokenType.SelfClosingTag:
                    case TokenType.EndTag:
                        var (name, moreAttr) = this.TagName();
                        while (moreAttr)
                        {
                            byte[] key, val;
                            (key, val, moreAttr) = this.TagAttr();
                            t.Attr.Add(new Attribute
                            {
                                Namespace = "",
                                Key = Atom.String(key),
                                Val = Encoding.UTF8.GetString(val)
                            });
                        }
                        var a = Atom.Lookup(name);
                        if (a != 0)
                        {
                            t.DataAtom = a;
                            t.Data = a.ToString();
                        }
                        else
                        {
                            t.DataAtom = 0;
                            t.Data = Encoding.UTF8.GetString(name);
                        }
                        break;
                }
                return t;
            }

            // SetMaxBuf sets a limit on the amount of data buffered during tokenization.
            // A value of 0 means unlimited.
            public int MaxBuf
            {
                set
                {
                    this.maxBuf = value;
                }
            }

            // NewTokenizer returns a new HTML Tokenizer for the given Reader.
            // The input is assumed to be UTF-8 encoded.
            public Tokenizer(Stream r) : this(r, "")
            {
            }

            // NewTokenizerFragment returns a new HTML Tokenizer for the given Reader, for
            // tokenizing an existing element's InnerHTML fragment. contextTag is that
            // element's tag, such as "div" or "iframe".
            //
            // For example, how the InnerHTML "a<b" is tokenized depends on whether it is
            // for a <p> tag or a <script> tag.
            //
            // The input is assumed to be UTF-8 encoded.
            public Tokenizer(Stream r, string contextTag)
            {
                this.r = r;
                this.buf = new byte[4096];
                this.bufLength = 0;
                if (!string.IsNullOrEmpty(contextTag))
                {
                    var s = contextTag.ToLowerInvariant();
                    switch (s)
                    {
                        case "iframe":
                        case "noembed":
                        case "noframes":
                        case "noscript":
                        case "plaintext":
                        case "script":
                        case "style":
                        case "title":
                        case "textarea":
                        case "xmp":
                            this.rawTag = s;
                            break;
                    }
                }
            }
        }
    }
}
