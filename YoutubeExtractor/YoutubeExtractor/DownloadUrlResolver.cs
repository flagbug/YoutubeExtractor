using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

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
        /// <exception cref="ArgumentNullException">The <paramref name="videoUrl"/> parameter is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The <paramref name="videoUrl"/> parameter is not a valid YouTube URL.</exception>
        /// <exception cref="VideoNotAvailableException">The video is not available.</exception>
        /// <exception cref="WebException">An error occurred while downloading the YouTube page html.</exception>
        /// <exception cref="YoutubeParseException">The Youtube page could not be parsed.</exception>
        public static IEnumerable<VideoInfo> GetDownloadUrls(string videoUrl)
        {
            if (videoUrl == null)
                throw new ArgumentNullException("videoUrl");

            videoUrl = NormalizeYoutubeUrl(videoUrl);

            string pageSource = GetPageSource(videoUrl);
            string videoTitle = GetVideoTitle(pageSource);

            string id = HttpUtility.ParseQueryString(new Uri(videoUrl).Query)["v"];

            string requestUrl = String.Format("http://www.youtube.com/get_video_info?&video_id={0}&el=detailpage&ps=default&eurl=&gl=US&hl=en", id);

            string source = GetPageSource(requestUrl);

            try
            {
                IEnumerable<Uri> downloadUrls = ExtractDownloadUrls(source);

                return GetVideoInfos(downloadUrls, videoTitle);
            }

            catch (Exception ex)
            {
                ThrowYoutubeParseException(ex);
            }

            if (IsVideoUnavailable(pageSource))
            {
                throw new VideoNotAvailableException();
            }

            // If everything else fails, throw a generic YoutubeParseException
            ThrowYoutubeParseException(null);

            return null; // Will never happen, but the compiler requires it
        }

        private static IEnumerable<Uri> ExtractDownloadUrls(string source)
        {
            var urls = new List<Uri>();
            var list = ParseFormEncoded(source);
            foreach (var kv in list) {
                if (kv[0] != "url_encoded_fmt_stream_map") continue;
                var list2 = ParseFormEncoded(kv[1], ',');
                foreach (var kv2 in list2) {
                    var list3 = ParseFormEncoded(kv2[1]);
                    string url = "";
                    string fallbackHost = "";
                    string sig = "";
                    foreach (var kv3 in list3) {
                        switch (kv3[0]) {
                            case "url":
                                url = kv3[1];
                                break;
                            case "fallback_host":
                                fallbackHost = kv3[1];
                                break;
                            case "sig":
                                sig = kv3[1];
                                break;
                        }
                        //var list4 = Divide(kv3[1].Substring(kv3[1].IndexOf('?') + 1));
                        //foreach (var kv4 in list4) {
                        //    sb.AppendLine("\t\t\t\t" + kv4[0] + "\t" + kv4[1]);
                        //}
                    }
                    if (url.IndexOf("&fallback_host=", StringComparison.Ordinal) < 0)
                        url += "&fallback_host=" + HttpUtility.UrlEncode(fallbackHost);
                    if (url.IndexOf("&signature=", StringComparison.Ordinal) < 0)
                        url += "&signature=" + HttpUtility.UrlEncode(sig);
                    urls.Add(new Uri(url));
                }
            }
            return urls;
        }

        private static IEnumerable<string[]> ParseFormEncoded(string qs, char split = '&')
        {
            var arr = qs.Split(split);
            var list = new List<string[]>(arr.Length);
            foreach (var kv in arr) {
                if (split == ',') {
                    list.Add(new[] { "\t", kv });
                } else {
                    var akv = kv.Split('=');
                    var k = HttpUtility.UrlDecode(akv[0]);
                    var v = HttpUtility.UrlDecode(akv[1]);
                    list.Add(new[] { k, v });
                }
            }
            return list;
        }

        private static string GetPageSource(string videoUrl)
        {
            string pageSource;
            var req = WebRequest.Create(videoUrl);

            using (var resp = req.GetResponse())
            {
                pageSource = new StreamReader(resp.GetResponseStream(), Encoding.UTF8).ReadToEnd();
            }

            return pageSource;
        }

        private static IEnumerable<VideoInfo> GetVideoInfos(IEnumerable<Uri> downloadUrls, string videoTitle)
        {
            var downLoadInfos = new List<VideoInfo>();

            foreach (Uri url in downloadUrls)
            {
                NameValueCollection queryString = HttpUtility.ParseQueryString(url.Query);

                // for this version, only get the download URL
                byte formatCode = Byte.Parse(queryString["itag"]);

                // Currently based on YouTube specifications (later we'll depend on the MIME type returned from the web request)
                VideoInfo info = VideoInfo.Defaults.SingleOrDefault(videoInfo => videoInfo.FormatCode == formatCode);

                if (info != null)
                {
                    info.DownloadUrl = url.ToString();
                    info.Title = videoTitle;
                }

                else
                {
                    info = new VideoInfo(formatCode);
                }

                downLoadInfos.Add(info);
            }

            return downLoadInfos;
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

        private static bool IsVideoUnavailable(string pageSource)
        {
            const string unavailableContainer = "<div id=\"watch-player-unavailable\">";

            return pageSource.Contains(unavailableContainer);
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

        private static void ThrowYoutubeParseException(Exception innerException)
        {
            throw new YoutubeParseException("Could not parse the Youtube page.\n" +
                                            "This may be due to a change of the Youtube page structure.\n" +
                                            "Please report this bug at www.github.com/flagbug/YoutubeExtractor/issues", innerException);
        }
    }
}
