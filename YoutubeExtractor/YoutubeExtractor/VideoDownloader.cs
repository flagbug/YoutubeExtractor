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
            var handle = new ManualResetEvent(false);
            var client = new WebClient();
            bool isCanceled = false;

            client.DownloadFileCompleted += (sender, args) =>
            {
                handle.Close();
                client.Dispose();

                // DownloadFileAsync passes the exception to the DownloadFileCompleted event, if one occurs
                if (args.Error != null && !args.Cancelled)
                {
                    throw args.Error;
                }

                handle.Set();
            };

            client.DownloadProgressChanged += (sender, args) =>
            {
                var progressArgs = new ProgressEventArgs(args.ProgressPercentage);

                // Backwards compatibility
                this.OnProgressChanged(progressArgs);

                if (this.DownloadProgressChanged != null)
                {
                    this.DownloadProgressChanged(this, progressArgs);

                    if (progressArgs.Cancel && !isCanceled)
                    {
                        isCanceled = true;
                        client.CancelAsync();
                    }
                }
            };

            this.OnDownloadStarted(EventArgs.Empty);

            if (this.BytesToDownload < 0)
            {
                client.DownloadFileAsync(new Uri(this.Video.DownloadUrl), this.SavePath);
                handle.WaitOne();
            }
            else
            {
                Stream inputStream = client.OpenRead(new Uri(this.Video.DownloadUrl));
                ReadAndWriteFromStream(inputStream);
            }

            this.OnDownloadFinished(EventArgs.Empty);
        }

        /// <summary>
        /// Write the stream in the specified file path.
        /// This method is fully synchronous.
        /// </summary>
        /// <param name="input">The stream which contains the video downloaded</param>
        private void ReadAndWriteFromStream(Stream input)
        {
            byte[] buffer = new byte[this.bufferSize];
            int bytesRead;
            int totalBytesRead = 0;

            FileStream writer = new FileStream(this.SavePath, FileMode.Create, FileAccess.Write);

            while (totalBytesRead < this.BytesToDownload
                && (bytesRead = input.Read(buffer, 0, Math.Min(buffer.Length, this.BytesToDownload - totalBytesRead))) > 0)
            {
                writer.Write(buffer, 0, bytesRead);
                totalBytesRead += bytesRead;
            }
            writer.Close();
        }
    }
}