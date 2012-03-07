using System;

namespace YoutubeExtractor
{
    public abstract class Downloader
    {
        public event EventHandler DownloadStarted;

        public event EventHandler<ProgressEventArgs> ProgressChanged;

        public event EventHandler DownloadFinished;

        public VideoInfo Video { get; private set; }

        public string SavePath { get; private set; }

        protected Downloader(VideoInfo video, string savePath)
        {
            this.Video = video;
            this.SavePath = savePath;
        }

        public abstract void Execute();

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

        protected void OnDownloadFinished(EventArgs e)
        {
            if (this.DownloadFinished != null)
            {
                this.DownloadFinished(this, e);
            }
        }
    }
}