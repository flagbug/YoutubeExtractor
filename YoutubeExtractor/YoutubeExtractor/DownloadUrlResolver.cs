using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json.Linq;

namespace YoutubeExtractor
{
    public static class DownloadUrlResolver
    {
        public static IEnumerable<VideoInfo> GetDownloadUrls(string videoUrl)
        {
            const string startConfig = "yt.playerConfig = ";

            string pageSource;

            var req = WebRequest.Create(videoUrl);

            using (var resp = req.GetResponse())
            {
                pageSource = new StreamReader(resp.GetResponseStream(), Encoding.UTF8).ReadToEnd();
            }

            int playerConfigIndex = pageSource.IndexOf(startConfig, StringComparison.Ordinal);

            if (playerConfigIndex > -1)
            {
                string signature = pageSource.Substring(playerConfigIndex);
                int endOfJsonIndex = signature.IndexOf(");", StringComparison.Ordinal);
                signature = signature.Substring(startConfig.Length, endOfJsonIndex - 17).Trim();

                JObject playerConfig = JObject.Parse(signature);
                JObject playerArgs = JObject.Parse(playerConfig["args"].ToString());
                var availableFormats = (string)playerArgs["url_encoded_fmt_stream_map"];

                const string argument = "url=";
                const string endOfQueryString = "&quality";

                if (availableFormats != String.Empty)
                {
                    var urlList = new List<string>(Regex.Split(availableFormats, argument));

                    var downLoadInfos = new List<VideoInfo>();

                    // Format the URL
                    var urls = urlList
                        .Where(entry => !String.IsNullOrEmpty(entry.Trim()))
                        .Select(entry => entry.Substring(0, entry.IndexOf(endOfQueryString, StringComparison.Ordinal)))
                        .Select(entry => new Uri(Uri.UnescapeDataString(entry)));

                    foreach (Uri url in urls)
                    {
                        NameValueCollection queryString = HttpUtility.ParseQueryString(url.Query);

                        // for this version, only get the download URL
                        byte formatCode = Byte.Parse(queryString["itag"]);
                        // Currently based on youtube specifications (later we'll depend on the MIME type returned from the web request)
                        downLoadInfos.Add(new VideoInfo(url.ToString(), formatCode));
                    }

                    return downLoadInfos;
                }
            }

            return Enumerable.Empty<VideoInfo>();
        }
    }
}