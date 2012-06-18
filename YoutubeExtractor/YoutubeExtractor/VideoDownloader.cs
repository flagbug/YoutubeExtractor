using System;
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
        public VideoDownloader(VideoInfo video, string savePath)
            : base(video, savePath)
        { }

        /// <summary>
        /// Starts the video download.
        /// </summary>
        public override void Execute()
        {
            // We need a handle to keep the method synchronously
            var handle = new ManualResetEvent(false);

            var client = new WebClient();

            client.DownloadFileCompleted += (sender, args) => handle.Set();
            client.DownloadProgressChanged += (sender, args) =>
                this.OnProgressChanged(new ProgressEventArgs(args.ProgressPercentage));

            this.OnDownloadStarted(EventArgs.Empty);

            client.DownloadFileAsync(new Uri(this.Video.DownloadUrl), this.SavePath);

            handle.WaitOne();
            handle.Close();

            this.OnDownloadFinished(EventArgs.Empty);
        }
    }
}