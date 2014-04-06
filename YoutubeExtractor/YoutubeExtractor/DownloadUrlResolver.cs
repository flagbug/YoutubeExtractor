using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace YoutubeExtractor
{
    /// <summary>
    /// Provides a method to get the download link of a YouTube video.
    /// </summary>
    public static class DownloadUrlResolver
    {
        private const int CorrectSignatureLength = 81;

        /// <summary>
        /// Gets a list of <see cref="VideoInfo" />s for the specified URL.
        /// </summary>
        /// <param name="videoUrl">The URL of the YouTube video.</param>
        /// <returns>A list of <see cref="VideoInfo" />s that can be used to download the video.</returns>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="videoUrl" /> parameter is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The <paramref name="videoUrl" /> parameter is not a valid YouTube URL.
        /// </exception>
        /// <exception cref="VideoNotAvailableException">The video is not available.</exception>
        /// <exception cref="WebException">
        /// An error occurred while downloading the YouTube page html.
        /// </exception>
        /// <exception cref="YoutubeParseException">The Youtube page could not be parsed.</exception>
        public static IEnumerable<VideoInfo> GetDownloadUrls(string videoUrl)
        {
            if (videoUrl == null)
                throw new ArgumentNullException("videoUrl");

            videoUrl = NormalizeYoutubeUrl(videoUrl);

            try
            {
                var json = LoadJson(videoUrl);

                string videoTitle = GetVideoTitle(json);

                IEnumerable<Uri> downloadUrls = ExtractDownloadUrls(json);

                return GetVideoInfos(downloadUrls, videoTitle);
            }

            catch (Exception ex)
            {
                if (ex is WebException || ex is VideoNotAvailableException)
                {
                    throw;
                }

                ThrowYoutubeParseException(ex);
            }

            return null; // Will never happen, but the compiler requires it
        }

        private static IEnumerable<Uri> ExtractDownloadUrls(JObject json)
        {
            string[] splitByUrls = GetStreamMap(json).Split(',');
            string[] adaptiveFmtSplitByUrls = GetAdaptiveStreamMap(json).Split(',');
            splitByUrls = splitByUrls.Concat(adaptiveFmtSplitByUrls).ToArray();

            foreach (string s in splitByUrls)
            {
                IDictionary<string, string> queries = HttpHelper.ParseQueryString(s);
                string url;

                if (queries.ContainsKey("s") || queries.ContainsKey("sig"))
                {
                    string signature = queries.ContainsKey("s") ? queries["s"] : queries["sig"];

                    string htmlPlayerVersion = GetHtml5PlayerVersion(json);
                    signature = GetDecipheredSignature(htmlPlayerVersion, signature);

                    url = string.Format("{0}&signature={1}", queries["url"], signature);

                    string fallbackHost = queries.ContainsKey("fallback_host") ? "&fallback_host=" + queries["fallback_host"] : String.Empty;

                    url += fallbackHost;
                }

                else
                {
                    url = queries["url"];
                }

                url = HttpHelper.UrlDecode(url);
                url = HttpHelper.UrlDecode(url);

                yield return new Uri(url);
            }
        }

        private static string GetAdaptiveStreamMap(JObject json)
        {
            JToken streamMap = json["args"]["adaptive_fmts"];

            return streamMap.ToString();
        }

        private static string GetDecipheredSignature(string htmlPlayerVersion, string signature)
        {
            if (signature.Length == CorrectSignatureLength)
            {
                return signature;
            }

            return Decipherer.DecipherWithVersion(signature, htmlPlayerVersion);
        }

        private static string GetHtml5PlayerVersion(JObject json)
        {
            var regex = new Regex(@"html5player-(.+?)\.js");

            string js = json["assets"]["js"].ToString();

            return regex.Match(js).Result("$1");
        }

        private static string GetStreamMap(JObject json)
        {
            JToken streamMap = json["args"]["url_encoded_fmt_stream_map"];

            string streamMapString = streamMap == null ? null : streamMap.ToString();

            if (streamMapString == null || streamMapString.Contains("been+removed"))
            {
                throw new VideoNotAvailableException("Video is removed");
            }

            return streamMapString;
        }

        private static IEnumerable<VideoInfo> GetVideoInfos(IEnumerable<Uri> downloadUrls, string videoTitle)
        {
            var downLoadInfos = new List<VideoInfo>();

            foreach (Uri url in downloadUrls)
            {
                string itag = HttpHelper.ParseQueryString(url.Query)["itag"];

                int formatCode = int.Parse(itag);

                VideoInfo info = VideoInfo.Defaults.SingleOrDefault(videoInfo => videoInfo.FormatCode == formatCode);

                if (info != null)
                {
                    info = new VideoInfo(info)
                    {
                        DownloadUrl = url.ToString(),
                        Title = videoTitle
                    };
                }

                else
                {
                    info = new VideoInfo(formatCode);
                }

                downLoadInfos.Add(info);
            }

            return downLoadInfos;
        }

        private static string GetVideoTitle(JObject json)
        {
            return json["args"]["title"].ToString();
        }

        private static bool IsVideoUnavailable(string pageSource)
        {
            const string unavailableContainer = "<div id=\"watch-player-unavailable\">";

            return pageSource.Contains(unavailableContainer);
        }

        private static JObject LoadJson(string url)
        {
            string pageSource = HttpHelper.DownloadString(url);

            if (IsVideoUnavailable(pageSource))
            {
                throw new VideoNotAvailableException();
            }

            var dataRegex = new Regex(@"ytplayer\.config\s*=\s*(\{.+?\});", RegexOptions.Multiline);

            string extractedJson = dataRegex.Match(pageSource).Result("$1");

            return JObject.Parse(extractedJson);
        }

#if PORTABLE

        public static async System.Threading.Tasks.Task<IEnumerable<VideoInfo>> GetDownloadUrlsAsync(string videoUrl)
        {
            return await System.Threading.Tasks.Task.Run(() => GetDownloadUrls(videoUrl));
        }

#endif

        private static string NormalizeYoutubeUrl(string url)
        {
            url = url.Trim();

            url = url.Replace("youtu.be/", "youtube.com/watch?v=");
            url = url.Replace("www.youtube", "youtube");
            url = url.Replace("youtube.com/embed/", "youtube.com/watch?v=");

            if (url.Contains("/v/"))
            {
                url = "http://youtube.com" + new Uri(url).AbsolutePath.Replace("/v/", "/watch?v=");
            }

            url = url.Replace("/watch#", "/watch?");

            IDictionary<string, string> query = HttpHelper.ParseQueryString(url);

            string v;

            if (!query.TryGetValue("v", out v))
            {
                throw new ArgumentException("URL is not a valid youtube URL!");
            }

            return "http://youtube.com/watch?v=" + v;
        }

        private static void ThrowYoutubeParseException(Exception innerException)
        {
            throw new YoutubeParseException("Could not parse the Youtube page.\n" +
                                            "This may be due to a change of the Youtube page structure.\n" +
                                            "Please report this bug at www.github.com/flagbug/YoutubeExtractor/issues", innerException);
        }
    }
}