using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace YoutubeExtractor
{
    /// <summary>
    /// Provides a method to download a video from YouTube.
    /// </summary>
    public class VideoDownloader : Downloader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VideoDownloader"/> class.
        /// </summary>
        /// <param name="video">The video to download.</param>
        /// <param name="savePath">The path to save the video.</param>
        /// <param name="bytesToDownload">An optional value to limit the number of bytes to download.</param>
        /// <exception cref="ArgumentNullException"><paramref name="video"/> or <paramref name="savePath"/> is <c>null</c>.</exception>
        public VideoDownloader(VideoInfo video, string savePath, int? bytesToDownload = null)
            : base(video, savePath, bytesToDownload)
        { }

        /// <summary>
        /// Occurs when the downlaod progress of the video file has changed.
        /// </summary>
        public event EventHandler<ProgressEventArgs> DownloadProgressChanged;

        /// <summary>
        /// Starts the video download.
        /// </summary>
        /// <exception cref="IOException">The video file could not be saved.</exception>
        /// <exception cref="WebException">An error occured while downloading the video.</exception>
        public override async Task Execute()
        {
            this.OnDownloadStarted(EventArgs.Empty);
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(5);

                var request = new HttpRequestMessage(HttpMethod.Get, this.Video.DownloadUrl);

                using (var response = await client.SendAsync(request).ConfigureAwait(false))
                using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    using (FileStream target = File.Open(this.SavePath, FileMode.Create, FileAccess.Write))
                    {
                        var buffer = new byte[1024];
                        bool cancel = false;
                        int bytes;
                        int copiedBytes = 0;

                        while (!cancel && (bytes = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            target.Write(buffer, 0, bytes);

                            copiedBytes += bytes;

                            var eventArgs = new ProgressEventArgs((copiedBytes * 1.0 / response.Content.Headers.ContentLength.Value) * 100);

                            if (this.DownloadProgressChanged != null)
                            {
                                this.DownloadProgressChanged(this, eventArgs);

                                if (eventArgs.Cancel)
                                {
                                    cancel = true;
                                }
                            }
                        }
                    }
                }
            }

            this.OnDownloadFinished(EventArgs.Empty);
        }
    }
}