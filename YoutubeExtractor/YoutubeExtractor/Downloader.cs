using System;

namespace YoutubeExtractor
{
    public abstract class Downloader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Downloader"/> class.
        /// </summary>
        /// <param name="video">The video to download/convert.</param>
        /// <param name="savePath">The path to save the video/audio.</param>
        protected Downloader(VideoInfo video, string savePath)
        {
            this.Video = video;
            this.SavePath = savePath;
        }

        /// <summary>
        /// Occurs when the download finished.
        /// </summary>
        public event EventHandler DownloadFinished;

        /// <summary>
        /// Occurs when the download is starts.
        /// </summary>
        public event EventHandler DownloadStarted;

        /// <summary>
        /// Occurs when the progress has changed.
        /// </summary>
        public event EventHandler<ProgressEventArgs> ProgressChanged;

        /// <summary>
        /// Gets the path to save the video/audio.
        /// </summary>
        public string SavePath { get; private set; }

        /// <summary>
        /// Gets the video to download/convert.
        /// </summary>
        public VideoInfo Video { get; private set; }

        /// <summary>
        /// Starts the work of the <see cref="Downloader"/>.
        /// </summary>
        public abstract void Execute();

        protected void OnDownloadFinished(EventArgs e)
        {
            if (this.DownloadFinished != null)
            {
                this.DownloadFinished(this, e);
            }
        }

        protected void OnDownloadStarted(EventArgs e)
        {
            if (this.DownloadStarted != null)
            {
                this.DownloadStarted(this, e);
            }
        }

        protected void OnProgressChanged(ProgressEventArgs e)
        {
            if (this.ProgressChanged != null)
            {
                this.ProgressChanged(this, e);
            }
        }
    }
}