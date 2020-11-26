using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace YoutubeExtractor
{
    /// <summary>
    /// Provides a method to get the download link of a YouTube video.
    /// </summary>
    public static class DownloadUrlResolver
    {
        private const string RateBypassFlag = "ratebypass";
        private const string SignatureQuery = "sig";

        /// <summary>
        /// Decrypts the signature in the <see cref="VideoInfo.DownloadUrl" /> property and sets it
        /// to the decrypted URL. Use this method, if you have decryptSignature in the <see
        /// cref="GetDownloadUrls" /> method set to false.
        /// </summary>
        /// <param name="videoInfo">The video info which's downlaod URL should be decrypted.</param>
        /// <exception cref="YoutubeParseException">
        /// There was an error while deciphering the signature.
        /// </exception>
        public static void DecryptDownloadUrl(VideoInfo videoInfo)
        {
            string decipheredSignature = string.Empty;
            IDictionary<string, string> strs = HttpHelper.ParseQueryString(videoInfo.DownloadUrl);
            if (strs.ContainsKey(SignatureQuery))
            {
                string item = strs[SignatureQuery];
                try
                {
                    if (!string.IsNullOrEmpty(videoInfo.HtmlPlayerVersion))
                        decipheredSignature = GetDecipheredSignature(videoInfo.HtmlPlayerVersion, item);
                    else
                        decipheredSignature = item;
                }
                catch (Exception exception)
                {
                    throw new YoutubeParseException("Could not decipher signature", exception);
                }
                videoInfo.DownloadUrl = HttpHelper.ReplaceQueryStringParameter(videoInfo.DownloadUrl, SignatureQuery, decipheredSignature);
                videoInfo.RequiresDecryption = false;
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
        public static IEnumerable<VideoInfo> GetDownloadUrls(string videoUrl, bool decryptSignature = true)
        {
            if (videoUrl == null)
            {
                throw new ArgumentNullException("videoUrl");
            }

            if (!TryNormalizeYoutubeUrl(videoUrl, out videoUrl))
            {
                throw new ArgumentException("URL is not a valid youtube URL!");
            }

            try
            {
                var model = LoadModel(videoUrl);

                string videoTitle = GetVideoTitle(model);

                IEnumerable<VideoInfo> infos = GetVideoInfos(model).ToList();

                string html5PlayerVersion = GetHtml5PlayerVersion(model.Microformat?.PlayerMicroformatRenderer?.Embed?.IframeUrl);

                foreach (VideoInfo videoInfo in infos)
                {
                    videoInfo.Title = videoTitle;
                    videoInfo.HtmlPlayerVersion = html5PlayerVersion;

                    //It takes a long time to decrypt all of item.
                    /*if (decryptSignature && info.RequiresDecryption)
                    {
                        DecryptDownloadUrl(info);
                    }*/
                }

                return infos;
            }
            catch (Exception ex)
            {
                if (ex is WebException || ex is VideoNotAvailableException)
                {
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
        /// <param name="videoId">The normalized YouTube URL.</param>
        /// <returns>
        /// <c>true</c>, if the normalization was successful; <c>false</c>, if the URL is invalid.
        /// </returns>
        public static bool TryNormalizeYoutubeUrl(string url, out string normalizedUrl)
        {
            url = url.Trim();
            url = url.Replace("youtu.be/", "youtube.com/watch?v=");
            url = url.Replace("www.youtube", "youtube");
            url = url.Replace("youtube.com/embed/", "youtube.com/watch?v=");
            if (url.Contains("/v/"))
            {
                url = string.Concat("http://youtube.com", (new Uri(url)).AbsolutePath.Replace("/v/", "/watch?v="));
            }
            url = url.Replace("/watch#", "/watch?");


            if (!HttpHelper.ParseQueryString(url).TryGetValue("v", out string v))
            {
                normalizedUrl = null;
                return false;
            }

            normalizedUrl = "https://youtube.com/watch?v=" + v;

            return true;
        }

        private static List<Format> GetAdaptiveStreamMap(YoutubeModel model)
        {
            return model.StreamingData?.AdaptiveFormats.ToList();
        }

        private static string GetDecipheredSignature(string htmlPlayerVersion, string signature)
        {
            return Decipherer.DecipherWithVersion(signature, htmlPlayerVersion);
        }

        private static string GetHtml5PlayerVersion(string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            string str = HttpHelper.DownloadString(url);
            Regex regex = new Regex(@"""jsUrl""\s*:\s*""([^""]+)""");
            return regex.Match(str).Result("$1").Replace("\\/", "/");
        }

        private static List<Format> GetStreamMap(YoutubeModel model)
        {
            return model.StreamingData?.Formats.ToList();
        }

        private static IEnumerable<VideoInfo> GetVideoInfos(YoutubeModel model)
        {
            var streamingFormats = GetStreamMap(model);
            if (streamingFormats == null)
                streamingFormats = new List<Format>();

            var adaptiveStream = GetAdaptiveStreamMap(model);
            if (adaptiveStream != null)
                streamingFormats.AddRange(adaptiveStream);

            foreach (var fmt in streamingFormats)
            {
                if (!fmt.Itag.HasValue) continue;

                var itag = (int)fmt.Itag.Value;

                VideoInfo videoInfo = VideoInfo.Defaults.SingleOrDefault(info => info.FormatCode == itag);

                videoInfo = videoInfo ?? new VideoInfo(itag);

                if (!string.IsNullOrEmpty(fmt.Url))
                {
                    videoInfo.DownloadUrl = HttpHelper.UrlDecode(HttpHelper.UrlDecode(fmt.Url));
                }
                else if (!string.IsNullOrEmpty(fmt.Cipher) || !string.IsNullOrEmpty(fmt.SignatureCipher))
                {
                    IDictionary<string, string> cipher = null;

                    if (!string.IsNullOrEmpty(fmt.Cipher))
                        cipher = HttpHelper.ParseQueryString(fmt.Cipher);
                    if (!string.IsNullOrEmpty(fmt.SignatureCipher))
                        cipher = HttpHelper.ParseQueryString(fmt.SignatureCipher);

                    if (!cipher.ContainsKey("url"))
                        continue;
                    if (!cipher.ContainsKey("s"))
                        continue;

                    var url = cipher["url"];
                    var sig = cipher["s"];

                    url = HttpHelper.UrlDecode(url);
                    url = HttpHelper.UrlDecode(url);

                    sig = HttpHelper.UrlDecode(sig);
                    sig = HttpHelper.UrlDecode(sig);

                    url = url.Replace("&s=", "&sig=");
                    videoInfo.DownloadUrl = HttpHelper.ReplaceQueryStringParameter(url, SignatureQuery, sig);
                    videoInfo.RequiresDecryption = true;
                }
                else
                    continue;

                if (!HttpHelper.ParseQueryString(videoInfo.DownloadUrl).ContainsKey(RateBypassFlag))
                {
                    videoInfo.DownloadUrl = string.Concat(videoInfo.DownloadUrl, string.Format("&{0}={1}", "ratebypass", "yes"));
                }

                if (fmt.AudioSampleRate.HasValue)
                    videoInfo.AudioBitrate = (int)fmt.AudioSampleRate.Value;

                if (fmt.ContentLength.HasValue)
                    videoInfo.FileSize = (int)fmt.ContentLength.Value;

                if (!string.IsNullOrEmpty(fmt.QualityLabel))
                    videoInfo.FormatNote = fmt.QualityLabel;
                else
                    videoInfo.FormatNote = fmt.Quality;

                if (fmt.Fps.HasValue)
                    videoInfo.FPS = (int)fmt.Fps.Value;

                if (fmt.Height.HasValue)
                    videoInfo.Height = (int)fmt.Height.Value;

                if (fmt.Width.HasValue)
                    videoInfo.Width = (int)fmt.Width.Value;

                // bitrate for itag 43 is always 2147483647
                if (itag != 43)
                {
                    if (fmt.AverageBitrate.HasValue)
                        videoInfo.AverageBitrate = fmt.AverageBitrate.Value / 1000f;
                    else if (fmt.Bitrate.HasValue)
                        videoInfo.AverageBitrate = fmt.Bitrate.Value / 1000f;
                }

                if (fmt.Height.HasValue)
                    videoInfo.Height = (int)fmt.Height.Value;

                if (fmt.Width.HasValue)
                    videoInfo.Width = (int)fmt.Width.Value;

                yield return videoInfo;
            }
        }

        private static string GetVideoTitle(YoutubeModel model)
        {
            return model.VideoDetails.Title.Replace("+", " ");
        }

        private static YoutubeModel LoadModel(string videoUrl)
        {
            var videoId = videoUrl.Replace("https://youtube.com/watch?v=", "");

            var url = $"https://www.youtube.com/watch?v={videoId}&gl=US&hl=en&has_verified=1&bpctr=9999999999";

            var pageSource = HttpHelper.DownloadString(url);
            var player_response = string.Empty;

            if (Regex.IsMatch(pageSource, "restrictions:age")
                || Regex.IsMatch(pageSource, "player-age-gate-content\">"))
            {
                url = $"https://www.youtube.com/get_video_info?video_id={videoId}&eurl=https://youtube.googleapis.com/v/{videoId}";
                pageSource = HttpHelper.DownloadString(url);
                player_response = HttpHelper.ParseQueryString(pageSource)["player_response"];
                player_response = HttpHelper.UrlDecode(player_response);
            }
            else
            {
                var dataRegex = new Regex(@"ytplayer\.config\s*=\s*(\{.+?\});", RegexOptions.Multiline);

                string extractedJson = dataRegex.Match(pageSource).Result("$1");
                player_response = JObject.Parse(extractedJson)["args"]["player_response"].ToString();
            }

            return YoutubeModel.FromJson(player_response);
        }

        private static void ThrowYoutubeParseException(Exception innerException, string videoUrl)
        {
            throw new YoutubeParseException("Could not parse the Youtube page for URL " + videoUrl + "\n" +
                                            "This may be due to a change of the Youtube page structure.\n" +
                                            "Please report this bug at www.github.com/flagbug/YoutubeExtractor/issues", innerException);
        }
    }
}