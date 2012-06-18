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
    /// <summary>
    /// Provides a method to get the download link of a YouTube video.
    /// </summary>
    public static class DownloadUrlResolver
    {
        /// <summary>
        /// Gets a list of <see cref="VideoInfo"/>s for the specified URL.
        /// </summary>
        /// <param name="videoUrl">The URL of the YouTube video.</param>
        /// <returns>A list of <see cref="VideoInfo"/>s that can be used to download the video.</returns>
        /// <exception cref="ArgumentException">videoUrl is not a valid YouTube URL.</exception>
        /// <exception cref="WebException">An error occured while downloading the video infos.</exception>
        public static IEnumerable<VideoInfo> GetDownloadUrls(string videoUrl)
        {
            videoUrl = NormalizeYoutubeUrl(videoUrl);

            const string startConfig = "yt.playerConfig = ";

            string pageSource;

            var req = WebRequest.Create(videoUrl);

            using (var resp = req.GetResponse())
            {
                pageSource = new StreamReader(resp.GetResponseStream(), Encoding.UTF8).ReadToEnd();
            }

            string videoTitle = GetVideoTitle(pageSource);

            int playerConfigIndex = pageSource.IndexOf(startConfig, StringComparison.Ordinal);

            if (playerConfigIndex > -1)
            {
                string signature = pageSource.Substring(playerConfigIndex);
                int endOfJsonIndex = signature.TrimEnd(' ').IndexOf("yt.setConfig", StringComparison.Ordinal);
                signature = signature.Substring(startConfig.Length, endOfJsonIndex - 26);

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
                        downLoadInfos.Add(new VideoInfo(url.ToString(), videoTitle, formatCode));
                    }

                    return downLoadInfos;
                }
            }

            return Enumerable.Empty<VideoInfo>();
        }

        private static string GetVideoTitle(string pageSource)
        {
            string videoTitle = null;

            try
            {
                const string videoTitlePattern = @"\<meta name=""title"" content=""(?<title>.*)""\>";
                var videoTitleRegex = new Regex(videoTitlePattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                Match videoTitleMatch = videoTitleRegex.Match(pageSource);

                if (videoTitleMatch.Success)
                {
                    videoTitle = videoTitleMatch.Groups["title"].Value;
                    videoTitle = HttpUtility.HtmlDecode(videoTitle);

                    // Remove the invalid characters in file names
                    // In Windows they are: \ / : * ? " < > |
                    videoTitle = Regex.Replace(videoTitle, @"[:\*\?""\<\>\|]", String.Empty);
                    videoTitle = videoTitle.Replace("\\", "-").Replace("/", "-").Trim();
                }
            }
            catch (Exception)
            {
                videoTitle = null;
            }

            return videoTitle;
        }

        private static string NormalizeYoutubeUrl(string url)
        {
            url = url.Trim();

            if (url.StartsWith("https://"))
            {
                url = "http://" + url.Substring(8);
            }

            else if (!url.StartsWith("http://"))
            {
                url = "http://" + url;
            }

            url = url.Replace("youtu.be/", "youtube.com/watch?v=");
            url = url.Replace("www.youtube.com", "youtube.com");

            if (url.StartsWith("http://youtube.com/v/"))
            {
                url = url.Replace("youtube.com/v/", "youtube.com/watch?v=");
            }
            else if (url.StartsWith("http://youtube.com/watch#"))
            {
                url = url.Replace("youtube.com/watch#", "youtube.com/watch?");
            }

            if (!url.StartsWith("http://youtube.com/watch"))
            {
                throw new ArgumentException("URL is not a valid youtube URL!");
            }

            return url;
        }
    }
}