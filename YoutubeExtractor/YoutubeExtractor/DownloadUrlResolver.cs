using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

#if NETFX_CORE

using Windows.Foundation;
using System.Net.Http;

#else

using System.Web;

#endif

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

#if NETFX_CORE
            string id = new WwwFormUrlDecoder(videoUrl).GetFirstValueByName("v");
#else
            string id = HttpUtility.ParseQueryString(new Uri(videoUrl).Query)["v"];
#endif

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
#if NETFX_CORE
            string urlMap = new WwwFormUrlDecoder(source).GetFirstValueByName("url_encoded_fmt_stream_map");
#else
            string urlMap = HttpUtility.ParseQueryString(source).Get("url_encoded_fmt_stream_map");
#endif

            string[] splitByUrls = urlMap.Split(',');

            foreach (string s in splitByUrls)
            {
#if NETFX_CORE
                var decoder = new WwwFormUrlDecoder(s);
                string url = string.Format("{0}&fallback_host={1}&signature={2}",
                    decoder.GetFirstValueByName("url"),
                    decoder.GetFirstValueByName("fallback_host"),
                    decoder.GetFirstValueByName("sig"));

                url = WebUtility.UrlDecode(url);
                url = WebUtility.UrlDecode(url);

#else
                var queries = HttpUtility.ParseQueryString(s);
                string url = string.Format("{0}&fallback_host={1}&signature={2}", queries["url"], queries["fallback_host"], queries["sig"]);

                url = HttpUtility.UrlDecode(url);
                url = HttpUtility.UrlDecode(url);
#endif
                yield return new Uri(url);
            }
        }

        private static string GetPageSource(string videoUrl)
        {
#if NETFX_CORE
            using (var client = new HttpClient())
            {
                return client.GetStringAsync(videoUrl).Result;
            }

#else
            using (var client = new WebClient())
            {
                client.Encoding = System.Text.Encoding.UTF8;
                return client.DownloadString(videoUrl);
            }
#endif
        }

        private static IEnumerable<VideoInfo> GetVideoInfos(IEnumerable<Uri> downloadUrls, string videoTitle)
        {
            var downLoadInfos = new List<VideoInfo>();

            foreach (Uri url in downloadUrls)
            {
#if NETFX_CORE
                string itag = new WwwFormUrlDecoder(url.Query).GetFirstValueByName("itag");
#else
                string itag = HttpUtility.ParseQueryString(url.Query)["itag"];
#endif

                // for this version, only get the download URL
                byte formatCode = Byte.Parse(itag);

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
                var videoTitleRegex = new Regex(videoTitlePattern, RegexOptions.IgnoreCase);
                Match videoTitleMatch = videoTitleRegex.Match(pageSource);

                if (videoTitleMatch.Success)
                {
                    videoTitle = videoTitleMatch.Groups["title"].Value;
#if NETFX_CORE
                    videoTitle = WebUtility.HtmlDecode(videoTitle);
#else
                    videoTitle = HttpUtility.HtmlDecode(videoTitle);
#endif

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