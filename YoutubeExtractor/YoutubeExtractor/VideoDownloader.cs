using System;
using System.IO;
using System.Net;
using System.Threading;

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
        /// <exception cref="ArgumentNullException"><paramref name="video"/> or <paramref name="savePath"/> is <c>null</c>.</exception>
        public VideoDownloader(VideoInfo video, string savePath)
            : base(video, savePath)
        { }

        /// <summary>
        /// Starts the video download.
        /// </summary>
        /// <exception cref="IOException">The video file could not be saved.</exception>
        /// <exception cref="WebException">An error occured while downloading the video.</exception>
        public override void Execute()
        {
            // We need a handle to keep the method synchronously
            var handle = new ManualResetEvent(false);

            var client = new WebClient();

            client.DownloadFileCompleted += (sender, args) =>
            {
                if (args == null)
                {
                    handle.Set();
                }

                else
                {
                    throw args.Error; // DownloadFileAsync passes the exception to the DownloadFileCompleted event, if one occurs
                }
            };

            client.DownloadProgressChanged += (sender, args) =>
                this.OnProgressChanged(new ProgressEventArgs(args.ProgressPercentage));

            this.OnDownloadStarted(EventArgs.Empty);

            try
            {
                client.DownloadFileAsync(new Uri(this.Video.DownloadUrl), this.SavePath);

                handle.WaitOne();
            }

            finally
            {
                handle.Close();
            }

            this.OnDownloadFinished(EventArgs.Empty);
        }
    }
}