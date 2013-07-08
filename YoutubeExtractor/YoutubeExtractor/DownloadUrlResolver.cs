using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

            string pageSource = HttpHelper.DownloadString(videoUrl);
            string videoTitle = GetVideoTitle(pageSource);

            string id = HttpHelper.ParseQueryString(videoUrl)["v"];

            string requestUrl = String.Format("http://www.youtube.com/get_video_info?&video_id={0}&el=detailpage&ps=default&eurl=&gl=US&hl=en", id);

            string source = HttpHelper.DownloadString(requestUrl);

            IDictionary<string, string> queries = HttpHelper.ParseQueryString(source);

            string status;

            if (queries.TryGetValue("status", out status) && status == "fail")
            {
                throw new VideoNotAvailableException(queries["reason"]);
            }

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

#if PORTABLE

        public static async System.Threading.Tasks.Task<IEnumerable<VideoInfo>> GetDownloadUrlsAsync(string videoUrl)
        {
            return await System.Threading.Tasks.Task.Run(() => GetDownloadUrls(videoUrl));
        }

#endif

        private static string DecryptSignature(string sig)
        {
            switch (sig.Length)
            {
                case 88:
                    {
                        char[] sigA = sig.ToCharArray();

                        sigA = sigA.Slice(2);
                        sigA = Swap(sigA, 1);
                        sigA = Swap(sigA, 10);

                        sigA = sigA.Reverse().ToArray();
                        sigA = sigA.Slice(2);
                        sigA = Swap(sigA, 23);

                        sigA = sigA.Slice(3);
                        sigA = Swap(sigA, 15);
                        sigA = Swap(sigA, 34);

                        sig = new string(sigA);
                    }
                    break;

                case 87:
                    {
                        var sigA = sig.Substring(44, 40).Reverse();
                        var sigB = sig.Substring(3, 40).Reverse();

                        sig = sigA.Substring(21, 1) + sigA.Substring(1, 20) + sigA[0] + sigB.Substring(22, 9) +
                            sig[0] + sigA.Substring(32, 8) + sig[43] + sigB;
                    }
                    break;

                case 86:
                    {
                        var sigA = sig.Substring(2, 40);
                        var sigB = sig.Substring(43, 40);

                        sig = sigA + sig[42] + sigB.Substring(0, 20) + sigB[39] + sigB.Substring(21, 18) + sigB[20];
                    }
                    break;

                case 85:
                    {
                        var sigA = sig.Substring(44, 40).Reverse();
                        var sigB = sig.Substring(3, 40).Reverse();

                        sig = sigA[7] + sigA.Substring(1, 6) + sigA[0] + sigA.Substring(8, 15) + sig[0] +
                            sigA.Substring(24, 9) + sig[1] + sigA.Substring(34, 6) + sig[43] + sigB;
                    }
                    break;

                case 84:
                    {
                        var sigA = sig.Substring(44, 40).Reverse();
                        var sigB = sig.Substring(3, 40).Reverse();

                        sig = sigA + sig[43] + sigB.Substring(0, 6) + sig[2] + sigB.Substring(7, 9) +
                            sigB[39] + sigB.Substring(17, 22) + sigB[16];
                    }
                    break;

                case 83:
                    {
                        var sigA = sig.Substring(43, 40).Reverse();
                        var sigB = sig.Substring(2, 40).Reverse();

                        sig = sigA[30] + sigA.Substring(1, 26) + sigB[39] +
                            sigA.Substring(28, 2) + sigA[0] + sigA.Substring(31, 9) + sig[42] +
                            sigB.Substring(0, 5) + sigA[27] + sigB.Substring(6, 33) + sigB[5];
                    }
                    break;

                case 82:
                    {
                        var sigA = sig.Substring(34, 48).Reverse();
                        var sigB = sig.Substring(0, 33).Reverse();

                        sig = sigA[45] + sigA.Substring(2, 12) + sigA[0] + sigA.Substring(15, 26) +
                            sig[33] + sigA.Substring(42, 3) +
                            sigA[41] + sigA[46] + sigB[32] + sigA[14] +
                            sigB.Substring(0, 32) + sigA[47];
                    }
                    break;
            }

            return sig;
        }

        private static IEnumerable<Uri> ExtractDownloadUrls(string source)
        {
            string urlMap = HttpHelper.ParseQueryString(source)["url_encoded_fmt_stream_map"];

            string[] splitByUrls = urlMap.Split(',');

            foreach (string s in splitByUrls)
            {
                IDictionary<string, string> queries = HttpHelper.ParseQueryString(s);
                string signature = queries.ContainsKey("s") ? DecryptSignature(queries["s"]) : queries["sig"];

                string url = string.Format("{0}&fallback_host={1}&signature={2}", queries["url"], queries["fallback_host"], signature);

                url = HttpHelper.UrlDecode(url);
                url = HttpHelper.UrlDecode(url);

                yield return new Uri(url);
            }
        }

        private static IEnumerable<VideoInfo> GetVideoInfos(IEnumerable<Uri> downloadUrls, string videoTitle)
        {
            var downLoadInfos = new List<VideoInfo>();

            foreach (Uri url in downloadUrls)
            {
                string itag = HttpHelper.ParseQueryString(url.Query)["itag"];

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

                    videoTitle = HttpHelper.HtmlDecode(videoTitle);

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

            url = url.Replace("youtu.be/", "youtube.com/watch?v=");
            url = url.Replace("www.youtube", "youtube");
            
            if(url.Contains("/v/"))
            {
                url = "http://youtube.com" + new Uri(url).AbsolutePath.Replace("/v/", "/watch?v=");
            }

            url = url.Replace("/watch#", "/watch?");

            IDictionary<string, string> query = HttpHelper.ParseQueryString(url);

            string v;

            if(!query.TryGetValue("v", out v))
            {
                throw new ArgumentException("URL is not a valid youtube URL!");
            }

            return "http://youtube.com/watch?v=" + v;
        }

        private static string Reverse(this string s)
        {
            return new string(s.ToCharArray().Reverse().ToArray());
        }

        private static T[] Slice<T>(this T[] source, int start)
        {
            int len = source.Length - start;

            var res = new T[len];

            for (int i = 0; i < len; i++)
            {
                res[i] = source[i + start];
            }

            return res;
        }

        private static char[] Swap(char[] a, int b)
        {
            var c = a[0];
            a[0] = a[b % a.Length];
            a[b] = c;
            return a;
        }

        private static void ThrowYoutubeParseException(Exception innerException)
        {
            throw new YoutubeParseException("Could not parse the Youtube page.\n" +
                                            "This may be due to a change of the Youtube page structure.\n" +
                                            "Please report this bug at www.github.com/flagbug/YoutubeExtractor/issues", innerException);
        }
    }
}