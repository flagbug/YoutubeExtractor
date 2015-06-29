using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using Newtonsoft.Json.Linq;

namespace YoutubeExtractor {
    /// <summary>
    /// Provides a method to get the download link of a YouTube video.
    /// </summary>
    public static class DownloadUrlResolver {
        private const string RateBypassFlag = "ratebypass";
        private const int CorrectSignatureLength = 81;
        private const string SignatureQuery = "signature";

        /// <summary>
        /// Decrypts the signature in the <see cref="VideoInfo.DownloadUrl" /> property and sets it
        /// to the decrypted URL. Use this method, if you have decryptSignature in the <see
        /// cref="GetDownloadUrls" /> method set to false.
        /// </summary>
        /// <param name="videoInfo">The video info which's downlaod URL should be decrypted.</param>
        /// <exception cref="YoutubeParseException">
        /// There was an error while deciphering the signature.
        /// </exception>
        public static void DecryptDownloadUrl(VideoInfo videoInfo) {
            IDictionary<string, string> queries = HttpHelper.ParseQueryString(videoInfo.DownloadUrl);

            if (queries.ContainsKey(SignatureQuery)) {
                string encryptedSignature = queries[SignatureQuery];

                string decrypted = DecryptSignature(encryptedSignature, videoInfo.HtmlPlayerVersion);

                // videoInfo.DownloadUrl = HttpHelper.ReplaceQueryStringParameter(videoInfo.DownloadUrl, SignatureQuery, decrypted);
                videoInfo.DownloadUrl = videoInfo.DownloadUrl.Replace(encryptedSignature, decrypted);
                videoInfo.RequiresDecryption = false;
            }
        }

        /// <summary>
        /// Decrypts specified signature and returns the decrypted URL.
        /// </summary>
        /// <param name="encryptedSignature">The encrypted signature to decrypt.</param>
        /// <param name="htmlPlayerVersion">The html player version.</param>
        /// <exception cref="YoutubeParseException">
        /// There was an error while deciphering the signature.
        /// </exception>
        private static string DecryptSignature(string signature, string htmlPlayerVersion) {
            try {
                if (signature.Length == CorrectSignatureLength) {
                    return signature;
                }

                return Decipherer.DecipherWithVersion(signature, htmlPlayerVersion);
            } catch (Exception ex) {
                throw new YoutubeParseException("Could not decipher signature", ex);
            }
        }

        /// <summary>
        /// Gets a list of <see cref="VideoInfo" />s for the specified URL.
        /// </summary>
        /// <param name="videoUrl">The URL of the YouTube video.</param>
        /// <param name="decryptSignature">
        /// A value indicating whether the video signatures should be decrypted or not. Decrypting
        /// consists of a HTTP request for each <see cref="VideoInfo" />, so you may want to set
        /// this to false and call <see cref="DecryptDownloadUrl" /> on your selected <see
        /// cref="VideoInfo" /> later.
        /// </param>
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
        public static IEnumerable<VideoInfo> GetDownloadUrls(string videoUrl, bool decryptSignature = true) {
            if (videoUrl == null)
                throw new ArgumentNullException("videoUrl");

            bool isYoutubeUrl = TryNormalizeYoutubeUrl(videoUrl, out videoUrl);

            if (!isYoutubeUrl) {
                throw new ArgumentException("URL is not a valid youtube URL!");
            }

            try {
                var json = LoadJson(videoUrl);

                string videoTitle = GetVideoTitle(json);

                IEnumerable<ExtractionInfo> downloadUrls = ExtractDownloadUrls(json);

                List<VideoInfo> infos = GetVideoInfos(downloadUrls, videoTitle).ToList();

                string dashManifestUrl = GetDashManifest(json);

                string htmlPlayerVersion = GetHtml5PlayerVersion(json);

                // Query dash manifest URL for additional formats
                if (!string.IsNullOrEmpty(dashManifestUrl)) {
                    string signature = ExtractSignatureFromManifest(dashManifestUrl);
                    if (!string.IsNullOrEmpty(signature)) {
                        string decrypt = DecryptSignature(signature, htmlPlayerVersion);
                        dashManifestUrl = dashManifestUrl.Replace(signature, decrypt).Replace("/s/", "/signature/");
                    }
                    ParseDashManifest(dashManifestUrl, infos, videoTitle);
                }

                foreach (VideoInfo info in infos) {
                    info.HtmlPlayerVersion = htmlPlayerVersion;

                    if (decryptSignature && info.RequiresDecryption) {
                        DecryptDownloadUrl(info);
                    }
                }

                return infos;
            } catch (Exception ex) {
                if (ex is WebException || ex is VideoNotAvailableException) {
                    throw;
                }

                ThrowYoutubeParseException(ex, videoUrl);
            }

            return null; // Will never happen, but the compiler requires it
        }

#if PORTABLE

        public static System.Threading.Tasks.Task<IEnumerable<VideoInfo>> GetDownloadUrlsAsync(string videoUrl, bool decryptSignature = true)
        {
            return System.Threading.Tasks.Task.Run(() => GetDownloadUrls(videoUrl, decryptSignature));
        }

#endif

        /// <summary>
        /// Normalizes the given YouTube URL to the format http://youtube.com/watch?v={youtube-id}
        /// and returns whether the normalization was successful or not.
        /// </summary>
        /// <param name="url">The YouTube URL to normalize.</param>
        /// <param name="normalizedUrl">The normalized YouTube URL.</param>
        /// <returns>
        /// <c>true</c>, if the normalization was successful; <c>false</c>, if the URL is invalid.
        /// </returns>
        public static bool TryNormalizeYoutubeUrl(string url, out string normalizedUrl) {
            url = url.Trim();

            url = url.Replace("youtu.be/", "youtube.com/watch?v=");
            url = url.Replace("www.youtube", "youtube");
            url = url.Replace("youtube.com/embed/", "youtube.com/watch?v=");

            if (url.Contains("/v/")) {
                url = "http://youtube.com" + new Uri(url).AbsolutePath.Replace("/v/", "/watch?v=");
            }

            url = url.Replace("/watch#", "/watch?");

            IDictionary<string, string> query = HttpHelper.ParseQueryString(url);

            string v;

            if (!query.TryGetValue("v", out v)) {
                normalizedUrl = null;
                return false;
            }

            normalizedUrl = "http://youtube.com/watch?v=" + v;

            return true;
        }

        private static IEnumerable<ExtractionInfo> ExtractDownloadUrls(JObject json) {
            string[] splitByUrls = GetStreamMap(json).Split(',');
            string[] adaptiveFmtSplitByUrls = GetAdaptiveStreamMap(json).Split(',');
            splitByUrls = splitByUrls.Concat(adaptiveFmtSplitByUrls).ToArray();

            foreach (string s in splitByUrls) {
                IDictionary<string, string> queries = HttpHelper.ParseQueryString(s);
                string url;

                bool requiresDecryption = false;

                if (queries.ContainsKey("s") || queries.ContainsKey("sig")) {
                    requiresDecryption = queries.ContainsKey("s");
                    string signature = queries.ContainsKey("s") ? queries["s"] : queries["sig"];

                    url = string.Format("{0}&{1}={2}", queries["url"], SignatureQuery, signature);

                    string fallbackHost = queries.ContainsKey("fallback_host") ? "&fallback_host=" + queries["fallback_host"] : String.Empty;

                    url += fallbackHost;
                } else {
                    url = queries["url"];
                }

                url = HttpHelper.UrlDecode(url);
                url = HttpHelper.UrlDecode(url);

                IDictionary<string, string> parameters = HttpHelper.ParseQueryString(url);
                if (!parameters.ContainsKey(RateBypassFlag))
                    url += string.Format("&{0}={1}", RateBypassFlag, "yes");

                yield return new ExtractionInfo { RequiresDecryption = requiresDecryption, Uri = new Uri(url) };
            }
        }

        /// <summary>
        /// Extracts the signature from the DASH Manifest which is located after /s/.
        /// </summary>
        /// <param name="manifestUrl">The DASH Manifest URL to extract from.</param>
        /// <returns>The extracted signature.</returns>
        private static string ExtractSignatureFromManifest(string manifestUrl) {
            string[] Params = manifestUrl.Split('/');
            for (int i = 0; i < Params.Length; i++) {
                if (Params[i] == "s" && i < Params.Length - 1)
                    return Params[i + 1];
            }
            return string.Empty;
        }

        private static string GetAdaptiveStreamMap(JObject json) {
            JToken streamMap = json["args"]["adaptive_fmts"];

            if (streamMap != null)
                return streamMap.ToString();
            else
                return string.Empty;
        }

        private static string GetHtml5PlayerVersion(JObject json) {
            var regex = new Regex(@"html5player-(.+?)\.js");

            string js = json["assets"]["js"].ToString();

            return regex.Match(js).Result("$1");
        }

        private static string GetStreamMap(JObject json) {
            JToken streamMap = json["args"]["url_encoded_fmt_stream_map"];

            string streamMapString = streamMap == null ? null : streamMap.ToString();

            if (streamMapString == null || streamMapString.Contains("been+removed")) {
                throw new VideoNotAvailableException("Video is removed or has an age restriction.");
            }

            return streamMapString;
        }

        private static List<VideoInfo> GetVideoInfos(IEnumerable<ExtractionInfo> extractionInfos, string videoTitle) {
            var downLoadInfos = new List<VideoInfo>();

            foreach (ExtractionInfo extractionInfo in extractionInfos) {
                var Params = HttpHelper.ParseQueryString(extractionInfo.Uri.Query);

                int formatCode = int.Parse(Params["itag"]);

                VideoInfo info = GetSingleVideoInfo(formatCode, extractionInfo.Uri.ToString(), videoTitle, extractionInfo.RequiresDecryption);

                downLoadInfos.Add(info);
            }

            return downLoadInfos;
        }

        public static VideoInfo GetSingleVideoInfo(int formatCode, string queryUrl, string videoTitle, bool requiresDecryption) {
            var Params = HttpHelper.ParseQueryString(queryUrl);

            VideoInfo info = VideoInfo.Defaults.SingleOrDefault(videoInfo => videoInfo.FormatCode == formatCode);

            if (info != null) {
                long fileSize = Params.ContainsKey("clen") ? long.Parse(Params["clen"]) : 0;
                info = new VideoInfo(info) {
                    DownloadUrl = queryUrl,
                    Title = videoTitle,
                    RequiresDecryption = requiresDecryption,
                    FileSize = fileSize
                };
            } else {
                info = new VideoInfo(formatCode) {
                    DownloadUrl = queryUrl
                };
            }

            return info;
        }

        private static string GetVideoTitle(JObject json) {
            JToken title = json["args"]["title"];

            return title == null ? String.Empty : title.ToString();
        }

        private static string GetDashManifest(JObject json) {
            JToken manifest = json["args"]["dashmpd"];

            return manifest == null ? String.Empty : manifest.ToString();
        }

        private static bool IsVideoUnavailable(string pageSource) {
            const string unavailableContainer = "<div id=\"watch-player-unavailable\">";

            return pageSource.Contains(unavailableContainer);
        }

        private static JObject LoadJson(string url) {
            string pageSource = HttpHelper.DownloadString(url);

            if (IsVideoUnavailable(pageSource)) {
                throw new VideoNotAvailableException();
            }

            var dataRegex = new Regex(@"ytplayer\.config\s*=\s*(\{.+?\});", RegexOptions.Multiline);

            string extractedJson = dataRegex.Match(pageSource).Result("$1");

            return JObject.Parse(extractedJson);
        }

        private static void ParseDashManifest(string dashManifestUrl, List<VideoInfo> previousFormats, string videoTitle) {
            string pageSource = HttpHelper.DownloadString(dashManifestUrl);

            XmlDocument doc = new XmlDocument();
            XmlNamespaceManager docNamespace = new XmlNamespaceManager(doc.NameTable);
            docNamespace.AddNamespace("urn", "urn:mpeg:DASH:schema:MPD:2011");
            docNamespace.AddNamespace("yd", "http://youtube.com/yt/2012/10/10");
            doc.LoadXml(pageSource);
            XmlNodeList ManifestList = doc.SelectNodes("//urn:Representation", docNamespace);
            foreach (XmlElement item in ManifestList) {
                int FormatCode = int.Parse(item.GetAttribute("id"));
                XmlNode BaseUrl = item.GetElementsByTagName("BaseURL").Item(0);
                VideoInfo info = GetSingleVideoInfo(FormatCode, BaseUrl.InnerText, videoTitle, false);
                if (item.HasAttribute("height"))
                    info.Resolution = int.Parse(item.GetAttribute("height"));
                if (item.HasAttribute("frameRate"))
                    info.FrameRate = int.Parse(item.GetAttribute("frameRate"));

                VideoInfo DeleteItem = previousFormats.SingleOrDefault(v => v.FormatCode == FormatCode);
                if (DeleteItem != null)
                    previousFormats.Remove(DeleteItem);
                previousFormats.Add(info);
            }
        }

        /// <summary>
        /// Non-DASH videos don't provide file size. Queries the server to know the stream size.
        /// </summary>
        /// <param name="info">The information of the stream to get the size for.</param>
        /// <returns>The stream size in bytes.</returns>
        public static void QueryStreamSize(VideoInfo info) {
            if (info.RequiresDecryption)
                DecryptDownloadUrl(info);

            var request = (HttpWebRequest)WebRequest.Create(info.DownloadUrl);
            using (WebResponse response = request.GetResponse()) {
                info.FileSize = (int)response.ContentLength;
            }
        }

        private static void ThrowYoutubeParseException(Exception innerException, string videoUrl) {
            throw new YoutubeParseException("Could not parse the Youtube page for URL " + videoUrl + "\n" +
                                            "This may be due to a change of the Youtube page structure.\n" +
                                            "Please report this bug at www.github.com/flagbug/YoutubeExtractor/issues", innerException);
        }

        private class ExtractionInfo {
            public bool RequiresDecryption { get; set; }

            public Uri Uri { get; set; }
        }
    }
}