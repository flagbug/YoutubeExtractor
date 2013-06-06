/*
 * This file has been copied from the Mono project on August 3rd 2011
 * as the .NET 3.5 Client Profile does not include the System.Web.dll
 * Source: https://github.com/mono/mono/blob/master/mcs/class/System.Web/System.Web.Util/HttpEncoder.cs
 */

//
// Authors:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//   Wictor Wilén (decode/encode functions) (wictor@ibizkit.se)
//   Tim Coleman (tim@timcoleman.com)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)

//   Marek Habersack <mhabersack@novell.com>
//
// (C) 2005-2010 Novell, Inc (http://novell.com/)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace YoutubeExtractor.Portable
{
    internal class HttpEncoder
    {
        private static readonly char[] hexChars = "0123456789abcdef".ToCharArray();
        private static readonly object entitiesLock = new object();
        private static Dictionary<string, char> entities;

        private static readonly HttpEncoder defaultEncoder;
        private static readonly HttpEncoder currentEncoder;

        private static IDictionary<string, char> Entities
        {
            get
            {
                lock (entitiesLock)
                {
                    if (entities == null)
                        InitEntities();

                    return entities;
                }
            }
        }

        public static HttpEncoder Current
        {
            get { return currentEncoder; }
#if NET_4_0
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				currentEncoder = value;
			}
#endif
        }

        public static HttpEncoder Default
        {
            get { return defaultEncoder; }
        }

        static HttpEncoder()
        {
            defaultEncoder = new HttpEncoder();
            currentEncoder = defaultEncoder;
        }

#if NET_4_0	
		protected internal virtual
#else
        internal static
#endif
            void HeaderNameValueEncode(string headerName, string headerValue, out string encodedHeaderName,
                                       out string encodedHeaderValue)
        {
            if (String.IsNullOrEmpty(headerName))
                encodedHeaderName = headerName;
            else
                encodedHeaderName = EncodeHeaderString(headerName);

            if (String.IsNullOrEmpty(headerValue))
                encodedHeaderValue = headerValue;
            else
                encodedHeaderValue = EncodeHeaderString(headerValue);
        }

        private static void StringBuilderAppend(string s, ref StringBuilder sb)
        {
            if (sb == null)
                sb = new StringBuilder(s);
            else
                sb.Append(s);
        }

        private static string EncodeHeaderString(string input)
        {
            StringBuilder sb = null;

            for (int i = 0; i < input.Length; i++)
            {
                char ch = input[i];

                if ((ch < 32 && ch != 9) || ch == 127)
                    StringBuilderAppend(String.Format("%{0:x2}", (int) ch), ref sb);
            }

            if (sb != null)
                return sb.ToString();

            return input;
        }

#if NET_4_0		
		protected internal virtual void HtmlAttributeEncode (string value, TextWriter output)
		{

			if (output == null)
				throw new ArgumentNullException ("output");

			if (String.IsNullOrEmpty (value))
				return;

			output.Write (HtmlAttributeEncode (value));
		}

		protected internal virtual void HtmlDecode (string value, TextWriter output)
		{
			if (output == null)
				throw new ArgumentNullException ("output");

			output.Write (HtmlDecode (value));
		}

		protected internal virtual void HtmlEncode (string value, TextWriter output)
		{
			if (output == null)
				throw new ArgumentNullException ("output");

			output.Write (HtmlEncode (value));
		}

		protected internal virtual byte[] UrlEncode (byte[] bytes, int offset, int count)
		{
			return UrlEncodeToBytes (bytes, offset, count);
		}
#endif
#if NET_4_0
		protected internal virtual
#else
        internal static
#endif
            string UrlPathEncode(string value)
        {
            if (String.IsNullOrEmpty(value))
                return value;

            var result = new MemoryStream();
            int length = value.Length;
            for (int i = 0; i < length; i++)
                UrlPathEncodeChar(value[i], result);

            byte[] asciiChars = result.ToArray();
            return asciiChars.Select(b => (char) b).Aggregate("", (str, c) => str + c);
        }

        internal static byte[] UrlEncodeToBytes(byte[] bytes, int offset, int count)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes");

            int blen = bytes.Length;
            if (blen == 0)
                return new byte[0];

            if (offset < 0 || offset >= blen)
                throw new ArgumentOutOfRangeException("offset");

            if (count < 0 || count > blen - offset)
                throw new ArgumentOutOfRangeException("count");

            var result = new MemoryStream(count);
            int end = offset + count;
            for (int i = offset; i < end; i++)
                UrlEncodeChar((char) bytes[i], result, false);

            return result.ToArray();
        }

        internal static string HtmlEncode(string s)
        {
            if (s == null)
                return null;

            if (s.Length == 0)
                return String.Empty;

            bool needEncode = false;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '&' || c == '"' || c == '<' || c == '>' || c > 159
#if NET_4_0
				    || c == '\''
#endif
                    )
                {
                    needEncode = true;
                    break;
                }
            }

            if (!needEncode)
                return s;

            var output = new StringBuilder();
            int len = s.Length;

            for (int i = 0; i < len; i++)
            {
                switch (s[i])
                {
                    case '&':
                        output.Append("&amp;");
                        break;
                    case '>':
                        output.Append("&gt;");
                        break;
                    case '<':
                        output.Append("&lt;");
                        break;
                    case '"':
                        output.Append("&quot;");
                        break;
#if NET_4_0
					case '\'':
						output.Append ("&#39;");
						break;
#endif
                    case '\uff1c':
                        output.Append("&#65308;");
                        break;

                    case '\uff1e':
                        output.Append("&#65310;");
                        break;

                    default:
                        char ch = s[i];
                        if (ch > 159 && ch < 256)
                        {
                            output.Append("&#");
                            output.Append(((int) ch).ToString(CultureInfo.InvariantCulture));
                            output.Append(";");
                        }
                        else
                            output.Append(ch);
                        break;
                }
            }

            return output.ToString();
        }

        internal static string HtmlAttributeEncode(string s)
        {
#if NET_4_0
			if (String.IsNullOrEmpty (s))
				return String.Empty;
#else
            if (s == null)
                return null;

            if (s.Length == 0)
                return String.Empty;
#endif
            bool needEncode = false;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '&' || c == '"' || c == '<'
#if NET_4_0
				    || c == '\''
#endif
                    )
                {
                    needEncode = true;
                    break;
                }
            }

            if (!needEncode)
                return s;

            var output = new StringBuilder();
            int len = s.Length;
            for (int i = 0; i < len; i++)
                switch (s[i])
                {
                    case '&':
                        output.Append("&amp;");
                        break;
                    case '"':
                        output.Append("&quot;");
                        break;
                    case '<':
                        output.Append("&lt;");
                        break;
#if NET_4_0
				case '\'':
					output.Append ("&#39;");
					break;
#endif
                    default:
                        output.Append(s[i]);
                        break;
                }

            return output.ToString();
        }

        internal static string HtmlDecode(string s)
        {
            if (s == null)
                return null;

            if (s.Length == 0)
                return String.Empty;

            if (s.IndexOf('&') == -1)
                return s;
#if NET_4_0
			StringBuilder rawEntity = new StringBuilder ();
#endif
            var entity = new StringBuilder();
            var output = new StringBuilder();
            int len = s.Length;
            // 0 -> nothing,
            // 1 -> right after '&'
            // 2 -> between '&' and ';' but no '#'
            // 3 -> '#' found after '&' and getting numbers
            int state = 0;
            int number = 0;
            bool is_hex_value = false;
            bool have_trailing_digits = false;

            for (int i = 0; i < len; i++)
            {
                char c = s[i];
                if (state == 0)
                {
                    if (c == '&')
                    {
                        entity.Append(c);
#if NET_4_0
						rawEntity.Append (c);
#endif
                        state = 1;
                    }
                    else
                    {
                        output.Append(c);
                    }
                    continue;
                }

                if (c == '&')
                {
                    state = 1;
                    if (have_trailing_digits)
                    {
                        entity.Append(number.ToString(CultureInfo.InvariantCulture));
                        have_trailing_digits = false;
                    }

                    output.Append(entity);
                    entity.Length = 0;
                    entity.Append('&');
                    continue;
                }

                if (state == 1)
                {
                    if (c == ';')
                    {
                        state = 0;
                        output.Append(entity);
                        output.Append(c);
                        entity.Length = 0;
                    }
                    else
                    {
                        number = 0;
                        is_hex_value = false;
                        if (c != '#')
                        {
                            state = 2;
                        }
                        else
                        {
                            state = 3;
                        }
                        entity.Append(c);
#if NET_4_0
						rawEntity.Append (c);
#endif
                    }
                }
                else if (state == 2)
                {
                    entity.Append(c);
                    if (c == ';')
                    {
                        string key = entity.ToString();
                        if (key.Length > 1 && Entities.ContainsKey(key.Substring(1, key.Length - 2)))
                            key = Entities[key.Substring(1, key.Length - 2)].ToString();

                        output.Append(key);
                        state = 0;
                        entity.Length = 0;
#if NET_4_0
						rawEntity.Length = 0;
#endif
                    }
                }
                else if (state == 3)
                {
                    if (c == ';')
                    {
#if NET_4_0
						if (number == 0)
							output.Append (rawEntity.ToString () + ";");
						else
#endif
                        if (number > 65535)
                        {
                            output.Append("&#");
                            output.Append(number.ToString(CultureInfo.InvariantCulture));
                            output.Append(";");
                        }
                        else
                        {
                            output.Append((char) number);
                        }
                        state = 0;
                        entity.Length = 0;
#if NET_4_0
						rawEntity.Length = 0;
#endif
                        have_trailing_digits = false;
                    }
                    else if (is_hex_value && IsHexDigit(c))
                    {
                        number = number*16 + FromHex(c);
                        have_trailing_digits = true;
#if NET_4_0
						rawEntity.Append (c);
#endif
                    }
                    else if (Char.IsDigit(c))
                    {
                        number = number*10 + (c - '0');
                        have_trailing_digits = true;
#if NET_4_0
						rawEntity.Append (c);
#endif
                    }
                    else if (number == 0 && (c == 'x' || c == 'X'))
                    {
                        is_hex_value = true;
#if NET_4_0
						rawEntity.Append (c);
#endif
                    }
                    else
                    {
                        state = 2;
                        if (have_trailing_digits)
                        {
                            entity.Append(number.ToString(CultureInfo.InvariantCulture));
                            have_trailing_digits = false;
                        }
                        entity.Append(c);
                    }
                }
            }

            if (entity.Length > 0)
            {
                output.Append(entity);
            }
            else if (have_trailing_digits)
            {
                output.Append(number.ToString(CultureInfo.InvariantCulture));
            }
            return output.ToString();
        }

        private static int FromHex(char c)
        {
            if ((c >= '0') && (c <= '9'))
            {
                return (c - '0');
            }
            if ((c >= 'a') && (c <= 'f'))
            {
                return ((c - 'a') + 10);
            }
            if ((c >= 'A') && (c <= 'F'))
            {
                return ((c - 'A') + 10);
            }
            return -1;
        }

        private static bool IsHexDigit(char c)
        {
            return ('0' <= c && '9' >= c)
                   || ('a' <= c && 'f' >= c)
                   || ('A' <= c && 'F' >= c);
        }

        internal static bool NotEncoded(char c)
        {
            return (c == '!' || c == '(' || c == ')' || c == '*' || c == '-' || c == '.' || c == '_'
#if !NET_4_0
                    || c == '\''
#endif
                   );
        }

        internal static void UrlEncodeChar(char c, Stream result, bool isUnicode)
        {
            if (c > 255)
            {
                //FIXME: what happens when there is an internal error?
                //if (!isUnicode)
                //	throw new ArgumentOutOfRangeException ("c", c, "c must be less than 256");
                int idx;
                int i = c;

                result.WriteByte((byte) '%');
                result.WriteByte((byte) 'u');
                idx = i >> 12;
                result.WriteByte((byte) hexChars[idx]);
                idx = (i >> 8) & 0x0F;
                result.WriteByte((byte) hexChars[idx]);
                idx = (i >> 4) & 0x0F;
                result.WriteByte((byte) hexChars[idx]);
                idx = i & 0x0F;
                result.WriteByte((byte) hexChars[idx]);
                return;
            }

            if (c > ' ' && NotEncoded(c))
            {
                result.WriteByte((byte) c);
                return;
            }
            if (c == ' ')
            {
                result.WriteByte((byte) '+');
                return;
            }
            if ((c < '0') ||
                (c < 'A' && c > '9') ||
                (c > 'Z' && c < 'a') ||
                (c > 'z'))
            {
                if (isUnicode && c > 127)
                {
                    result.WriteByte((byte) '%');
                    result.WriteByte((byte) 'u');
                    result.WriteByte((byte) '0');
                    result.WriteByte((byte) '0');
                }
                else
                    result.WriteByte((byte) '%');

                int idx = (c) >> 4;
                result.WriteByte((byte) hexChars[idx]);
                idx = (c) & 0x0F;
                result.WriteByte((byte) hexChars[idx]);
            }
            else
                result.WriteByte((byte) c);
        }

        internal static void UrlPathEncodeChar(char c, Stream result)
        {
            if (c < 33 || c > 126)
            {
                byte[] bIn = Encoding.UTF8.GetBytes(c.ToString());
                for (int i = 0; i < bIn.Length; i++)
                {
                    result.WriteByte((byte) '%');
                    int idx = (bIn[i]) >> 4;
                    result.WriteByte((byte) hexChars[idx]);
                    idx = (bIn[i]) & 0x0F;
                    result.WriteByte((byte) hexChars[idx]);
                }
            }
            else if (c == ' ')
            {
                result.WriteByte((byte) '%');
                result.WriteByte((byte) '2');
                result.WriteByte((byte) '0');
            }
            else
                result.WriteByte((byte) c);
        }

        private static void InitEntities()
        {
            // Build the hash table of HTML entity references.  This list comes
            // from the HTML 4.01 W3C recommendation.
            entities = new Dictionary<string, char>(StringComparer.Ordinal)
                           {
                               {"nbsp", '\u00A0'},
                               {"iexcl", '\u00A1'},
                               {"cent", '\u00A2'},
                               {"pound", '\u00A3'},
                               {"curren", '\u00A4'},
                               {"yen", '\u00A5'},
                               {"brvbar", '\u00A6'},
                               {"sect", '\u00A7'},
                               {"uml", '\u00A8'},
                               {"copy", '\u00A9'},
                               {"ordf", '\u00AA'},
                               {"laquo", '\u00AB'},
                               {"not", '\u00AC'},
                               {"shy", '\u00AD'},
                               {"reg", '\u00AE'},
                               {"macr", '\u00AF'},
                               {"deg", '\u00B0'},
                               {"plusmn", '\u00B1'},
                               {"sup2", '\u00B2'},
                               {"sup3", '\u00B3'},
                               {"acute", '\u00B4'},
                               {"micro", '\u00B5'},
                               {"para", '\u00B6'},
                               {"middot", '\u00B7'},
                               {"cedil", '\u00B8'},
                               {"sup1", '\u00B9'},
                               {"ordm", '\u00BA'},
                               {"raquo", '\u00BB'},
                               {"frac14", '\u00BC'},
                               {"frac12", '\u00BD'},
                               {"frac34", '\u00BE'},
                               {"iquest", '\u00BF'},
                               {"Agrave", '\u00C0'},
                               {"Aacute", '\u00C1'},
                               {"Acirc", '\u00C2'},
                               {"Atilde", '\u00C3'},
                               {"Auml", '\u00C4'},
                               {"Aring", '\u00C5'},
                               {"AElig", '\u00C6'},
                               {"Ccedil", '\u00C7'},
                               {"Egrave", '\u00C8'},
                               {"Eacute", '\u00C9'},
                               {"Ecirc", '\u00CA'},
                               {"Euml", '\u00CB'},
                               {"Igrave", '\u00CC'},
                               {"Iacute", '\u00CD'},
                               {"Icirc", '\u00CE'},
                               {"Iuml", '\u00CF'},
                               {"ETH", '\u00D0'},
                               {"Ntilde", '\u00D1'},
                               {"Ograve", '\u00D2'},
                               {"Oacute", '\u00D3'},
                               {"Ocirc", '\u00D4'},
                               {"Otilde", '\u00D5'},
                               {"Ouml", '\u00D6'},
                               {"times", '\u00D7'},
                               {"Oslash", '\u00D8'},
                               {"Ugrave", '\u00D9'},
                               {"Uacute", '\u00DA'},
                               {"Ucirc", '\u00DB'},
                               {"Uuml", '\u00DC'},
                               {"Yacute", '\u00DD'},
                               {"THORN", '\u00DE'},
                               {"szlig", '\u00DF'},
                               {"agrave", '\u00E0'},
                               {"aacute", '\u00E1'},
                               {"acirc", '\u00E2'},
                               {"atilde", '\u00E3'},
                               {"auml", '\u00E4'},
                               {"aring", '\u00E5'},
                               {"aelig", '\u00E6'},
                               {"ccedil", '\u00E7'},
                               {"egrave", '\u00E8'},
                               {"eacute", '\u00E9'},
                               {"ecirc", '\u00EA'},
                               {"euml", '\u00EB'},
                               {"igrave", '\u00EC'},
                               {"iacute", '\u00ED'},
                               {"icirc", '\u00EE'},
                               {"iuml", '\u00EF'},
                               {"eth", '\u00F0'},
                               {"ntilde", '\u00F1'},
                               {"ograve", '\u00F2'},
                               {"oacute", '\u00F3'},
                               {"ocirc", '\u00F4'},
                               {"otilde", '\u00F5'},
                               {"ouml", '\u00F6'},
                               {"divide", '\u00F7'},
                               {"oslash", '\u00F8'},
                               {"ugrave", '\u00F9'},
                               {"uacute", '\u00FA'},
                               {"ucirc", '\u00FB'},
                               {"uuml", '\u00FC'},
                               {"yacute", '\u00FD'},
                               {"thorn", '\u00FE'},
                               {"yuml", '\u00FF'},
                               {"fnof", '\u0192'},
                               {"Alpha", '\u0391'},
                               {"Beta", '\u0392'},
                               {"Gamma", '\u0393'},
                               {"Delta", '\u0394'},
                               {"Epsilon", '\u0395'},
                               {"Zeta", '\u0396'},
                               {"Eta", '\u0397'},
                               {"Theta", '\u0398'},
                               {"Iota", '\u0399'},
                               {"Kappa", '\u039A'},
                               {"Lambda", '\u039B'},
                               {"Mu", '\u039C'},
                               {"Nu", '\u039D'},
                               {"Xi", '\u039E'},
                               {"Omicron", '\u039F'},
                               {"Pi", '\u03A0'},
                               {"Rho", '\u03A1'},
                               {"Sigma", '\u03A3'},
                               {"Tau", '\u03A4'},
                               {"Upsilon", '\u03A5'},
                               {"Phi", '\u03A6'},
                               {"Chi", '\u03A7'},
                               {"Psi", '\u03A8'},
                               {"Omega", '\u03A9'},
                               {"alpha", '\u03B1'},
                               {"beta", '\u03B2'},
                               {"gamma", '\u03B3'},
                               {"delta", '\u03B4'},
                               {"epsilon", '\u03B5'},
                               {"zeta", '\u03B6'},
                               {"eta", '\u03B7'},
                               {"theta", '\u03B8'},
                               {"iota", '\u03B9'},
                               {"kappa", '\u03BA'},
                               {"lambda", '\u03BB'},
                               {"mu", '\u03BC'},
                               {"nu", '\u03BD'},
                               {"xi", '\u03BE'},
                               {"omicron", '\u03BF'},
                               {"pi", '\u03C0'},
                               {"rho", '\u03C1'},
                               {"sigmaf", '\u03C2'},
                               {"sigma", '\u03C3'},
                               {"tau", '\u03C4'},
                               {"upsilon", '\u03C5'},
                               {"phi", '\u03C6'},
                               {"chi", '\u03C7'},
                               {"psi", '\u03C8'},
                               {"omega", '\u03C9'},
                               {"thetasym", '\u03D1'},
                               {"upsih", '\u03D2'},
                               {"piv", '\u03D6'},
                               {"bull", '\u2022'},
                               {"hellip", '\u2026'},
                               {"prime", '\u2032'},
                               {"Prime", '\u2033'},
                               {"oline", '\u203E'},
                               {"frasl", '\u2044'},
                               {"weierp", '\u2118'},
                               {"image", '\u2111'},
                               {"real", '\u211C'},
                               {"trade", '\u2122'},
                               {"alefsym", '\u2135'},
                               {"larr", '\u2190'},
                               {"uarr", '\u2191'},
                               {"rarr", '\u2192'},
                               {"darr", '\u2193'},
                               {"harr", '\u2194'},
                               {"crarr", '\u21B5'},
                               {"lArr", '\u21D0'},
                               {"uArr", '\u21D1'},
                               {"rArr", '\u21D2'},
                               {"dArr", '\u21D3'},
                               {"hArr", '\u21D4'},
                               {"forall", '\u2200'},
                               {"part", '\u2202'},
                               {"exist", '\u2203'},
                               {"empty", '\u2205'},
                               {"nabla", '\u2207'},
                               {"isin", '\u2208'},
                               {"notin", '\u2209'},
                               {"ni", '\u220B'},
                               {"prod", '\u220F'},
                               {"sum", '\u2211'},
                               {"minus", '\u2212'},
                               {"lowast", '\u2217'},
                               {"radic", '\u221A'},
                               {"prop", '\u221D'},
                               {"infin", '\u221E'},
                               {"ang", '\u2220'},
                               {"and", '\u2227'},
                               {"or", '\u2228'},
                               {"cap", '\u2229'},
                               {"cup", '\u222A'},
                               {"int", '\u222B'},
                               {"there4", '\u2234'},
                               {"sim", '\u223C'},
                               {"cong", '\u2245'},
                               {"asymp", '\u2248'},
                               {"ne", '\u2260'},
                               {"equiv", '\u2261'},
                               {"le", '\u2264'},
                               {"ge", '\u2265'},
                               {"sub", '\u2282'},
                               {"sup", '\u2283'},
                               {"nsub", '\u2284'},
                               {"sube", '\u2286'},
                               {"supe", '\u2287'},
                               {"oplus", '\u2295'},
                               {"otimes", '\u2297'},
                               {"perp", '\u22A5'},
                               {"sdot", '\u22C5'},
                               {"lceil", '\u2308'},
                               {"rceil", '\u2309'},
                               {"lfloor", '\u230A'},
                               {"rfloor", '\u230B'},
                               {"lang", '\u2329'},
                               {"rang", '\u232A'},
                               {"loz", '\u25CA'},
                               {"spades", '\u2660'},
                               {"clubs", '\u2663'},
                               {"hearts", '\u2665'},
                               {"diams", '\u2666'},
                               {"quot", '\u0022'},
                               {"amp", '\u0026'},
                               {"lt", '\u003C'},
                               {"gt", '\u003E'},
                               {"OElig", '\u0152'},
                               {"oelig", '\u0153'},
                               {"Scaron", '\u0160'},
                               {"scaron", '\u0161'},
                               {"Yuml", '\u0178'},
                               {"circ", '\u02C6'},
                               {"tilde", '\u02DC'},
                               {"ensp", '\u2002'},
                               {"emsp", '\u2003'},
                               {"thinsp", '\u2009'},
                               {"zwnj", '\u200C'},
                               {"zwj", '\u200D'},
                               {"lrm", '\u200E'},
                               {"rlm", '\u200F'},
                               {"ndash", '\u2013'},
                               {"mdash", '\u2014'},
                               {"lsquo", '\u2018'},
                               {"rsquo", '\u2019'},
                               {"sbquo", '\u201A'},
                               {"ldquo", '\u201C'},
                               {"rdquo", '\u201D'},
                               {"bdquo", '\u201E'},
                               {"dagger", '\u2020'},
                               {"Dagger", '\u2021'},
                               {"permil", '\u2030'},
                               {"lsaquo", '\u2039'},
                               {"rsaquo", '\u203A'},
                               {"euro", '\u20AC'}
                           };
        }
    }
}