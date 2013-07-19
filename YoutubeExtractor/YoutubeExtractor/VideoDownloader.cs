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
        /// The buffer size used in the method ReadAndWriteFromStream.
        /// Set on 10kb.
        /// </summary>
        private readonly int bufferSize = 10240;

        /// <summary>
        /// A web client used in the Execute method to download the video.
        /// </summary>
        private WebClient webClient = null;

        /// <summary>
        /// An handle which stops the actual thread during the asynchronous webclient method.
        /// </summary>
        private ManualResetEvent handle = null;

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
        /// Initializes a new instance of the <see cref="VideoDownloader"/> class.
        /// </summary>
        /// <param name="video">The video to convert.</param>
        /// <param name="savePath">The path to save the audio.</param>
        /// <param name="bytesToDownload">The number of bytes to download</param>
        /// <exception cref="ArgumentNullException"><paramref name="video"/> or <paramref name="savePath"/> is <c>null</c>.</exception>
        public VideoDownloader(VideoInfo video, string savePath, int bytesToDownload)
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
        public override void Execute()
        {
            // We need a handle to keep the method synchronously
            handle = new ManualResetEvent(false);
            webClient = new WebClient();
            bool isCanceled = false;

            webClient.DownloadFileCompleted += (sender, args) =>
            {
                handle.Close();
                webClient.Dispose();

                // DownloadFileAsync passes the exception to the DownloadFileCompleted event, if one occurs
                if (args.Error != null && !args.Cancelled)
                {
                    throw args.Error;
                }

                handle.Set();
            };

            webClient.DownloadProgressChanged += (sender, args) =>
            {
                if (!NotifyProgress(args.ProgressPercentage))
                {
                    isCanceled = true;
                    webClient.CancelAsync();
                }
            };

            webClient.OpenReadCompleted += new OpenReadCompletedEventHandler(Client_OpenReadCompleted);

            this.OnDownloadStarted(EventArgs.Empty);

            if (this.BytesToDownload < 0)
                webClient.DownloadFileAsync(new Uri(this.Video.DownloadUrl), this.SavePath);
            else
                webClient.OpenReadAsync(new Uri(this.Video.DownloadUrl));
            handle.WaitOne();

            this.OnDownloadFinished(EventArgs.Empty);
        }

        void Client_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                Stream input = e.Result;
                byte[] buffer = new byte[this.bufferSize];
                int bytesRead;
                int totalBytesRead = 0;
                FileStream writer = null;

                try
                {
                    writer = new FileStream(this.SavePath, FileMode.Create, FileAccess.Write);

                    while (totalBytesRead < this.BytesToDownload
                        && (bytesRead = input.Read(buffer, 0, Math.Min(buffer.Length, this.BytesToDownload - totalBytesRead))) > 0)
                    {
                        writer.Write(buffer, 0, bytesRead);
                        totalBytesRead += bytesRead;

                        if (!NotifyProgress(((double)totalBytesRead / (double)this.BytesToDownload) * 100.0)) break;
                    }
                }
                catch (Exception ex)
                {
                    if (writer != null)
                        writer.Close();
                    input.Close();
                    this.webClient.Dispose();
                    this.handle.Close();
                    this.handle.Set();
                }
            }
            else
            {
                
            }
        }

        private bool NotifyProgress(double progress)
        {
            var progressArgs = new ProgressEventArgs(progress);

            this.OnProgressChanged(progressArgs);

            if (this.DownloadProgressChanged != null)
            {
                this.DownloadProgressChanged(this, progressArgs);
            }

            return !progressArgs.Cancel;
        }
    }
}