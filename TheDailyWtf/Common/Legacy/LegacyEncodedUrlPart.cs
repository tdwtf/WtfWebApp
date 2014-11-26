using System;
using System.Text;
using System.Text.RegularExpressions;

namespace TheDailyWtf.Legacy
{
    /// <summary>
    /// Represents TRWTF... or a part of a URL that is encoded in the custom WTF format 
    /// held over from the classic ASP days.
    /// </summary>
    internal sealed class LegacyEncodedUrlPart
    {
        private static readonly Regex escapedRegex = new Regex(" 0x(?<hex>[0-9a-f]{1,4}) ", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly string invalidChars = new string(
                new char[] {'"','<','>','%','\\','^','[',']','`',
                        '+','$',';','/','?',':','@','=','&',
                        '#','*','.','_', '\'', '“','”' });

        /// <summary>
        /// Gets the URL in the wonky encoded format.
        /// </summary>
        public string EncodedForUrl { get; private set; }
        /// <summary>
        /// Gets the decoded string.
        /// </summary>
        public string DecodedValue { get; private set; }

        public static LegacyEncodedUrlPart CreateFromEncodedUrl(string url)
        {
            return new LegacyEncodedUrlPart
            {
                EncodedForUrl = url ?? "",
                DecodedValue = Decode(url)
            };
        }

        public static LegacyEncodedUrlPart CreateFromString(string str)
        {
            return new LegacyEncodedUrlPart
            {
                EncodedForUrl = Encode(str),
                DecodedValue = str ?? ""
            };
        }

        private static string Decode(string val)
        {
            if (val == null) 
                return string.Empty;

            return escapedRegex.Replace(
                val.Replace('_', ' '), 
                match =>
                {
                    int asciiValue = Convert.ToInt32(match.Groups["hex"].Value, 16);
                    return Convert.ToChar(asciiValue).ToString();
                }
            );
        }
        
        private static string Encode(string val)
        {
            if (val == null) 
                return string.Empty;

            //check if name contains hex escapes
            if (escapedRegex.IsMatch(val))
            {
                //escape out the "x" (ASC 78) using spaces, which get replaced
                //with "_" anyway
                val = escapedRegex.Replace(val, " 0 0x78 ${hex} ");
            }
            var ret = new StringBuilder();
            foreach (char chr in val)
            {
                if (invalidChars.IndexOf(chr) != -1)
                    ret.Append(EncodeChar(chr));
                else if (chr == ' ') ret.Append('_');
                else ret.Append(chr);
            }

            return ret.ToString();
        }

        private static string EncodeChar(char chr)
        {
            return string.Format("_0x{0}_", Convert.ToInt32(chr).ToString("x"));
        }
    }
}