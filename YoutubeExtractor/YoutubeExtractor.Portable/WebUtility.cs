using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace YoutubeExtractor.Portable
{
    public static class WebUtility
    {
        private static readonly Regex QueryStringRegex =
            new Regex(@"[\?&](?<name>[^&=]+)=(?<value>[^&=]+)");

        public static IEnumerable<KeyValuePair<string, string>> ParseQueryString(Uri uri)
        {
            if (uri == null)
                throw new ArgumentException("uri");

            return ParseQueryString(uri.Query);
        }

        public static IEnumerable<KeyValuePair<string, string>> ParseQueryString(string query)
        {
            if (query == null)
                throw new ArgumentException("query");
            
            if (query[0] != '?') query = string.Format("?{0}", query);
            MatchCollection matches = QueryStringRegex.Matches(query);
            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                yield return new KeyValuePair<string, string>(match.Groups["name"].Value, match.Groups["value"].Value);
            }
        }

        public static string BuildQueryString(IDictionary<string, string> parameters)
        {
            IEnumerable<string> keys = parameters.Keys.ToArray();
            return string.Join("&",
                               keys.Select(x => string.Format("{0}={1}",
                                                              Uri.EscapeDataString(x),
                                                              Uri.EscapeDataString(parameters[x]))));
        }

        /// <summary>
        ///     Decodes an HTML-encoded string and returns the decoded string.
        /// </summary>
        /// <param name="s">The HTML string to decode. </param>
        /// <returns>The decoded text.</returns>
        public static string HtmlDecode(string s)
        {
            return HttpEncoder.HtmlDecode(s);
        }

        public static string UrlDecode(string url)
        {
            return Uri.UnescapeDataString(url);
        }
    }
}