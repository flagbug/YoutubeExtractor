﻿using System;

namespace YoutubeExtractor
{
    /// <summary>
    /// Provides the base class for the <see cref="AudioDownloader"/> and <see cref="VideoDownloader"/> class.
    /// </summary>
    public abstract class Downloader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Downloader"/> class.
        /// </summary>
        /// <param name="video">The video to download/convert.</param>
        /// <param name="savePath">The path to save the video/audio.</param>
        /// <param name="bytesToDownload">The amount of bytes which will be downloaded</param>
        /// <exception cref="ArgumentNullException"><paramref name="video"/> or <paramref name="savePath"/> is <c>null</c>.</exception>
        protected Downloader(VideoInfo video, string savePath, int bytesToDownload = -1)
        {
            if (video == null)
                throw new ArgumentNullException("video");

            if (savePath == null)
                throw new ArgumentNullException("savePath");

            if (bytesToDownload < -1)
                throw new ArgumentNullException("bytesToDownload");

            this.Video = video;
            this.SavePath = savePath;
            this.BytesToDownload = bytesToDownload;
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
        [Obsolete("Use the downloader specific progress events instead.")]
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
        /// Gets the number of bytes to download from a video.
        /// </summary>
        public int BytesToDownload { get; protected set; }

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

        [Obsolete("Each downloader has to implement its own progress notification.")]
        protected void OnProgressChanged(ProgressEventArgs e)
        {
            if (this.ProgressChanged != null)
            {
                this.ProgressChanged(this, e);
            }
        }
    }
}