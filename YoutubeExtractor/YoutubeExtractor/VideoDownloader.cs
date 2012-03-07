using System;
using System.Net;
using System.Threading;

namespace YoutubeExtractor
{
    public class VideoDownloader : Downloader
    {
        public VideoDownloader(VideoInfo video, string savePath)
            : base(video, savePath)
        { }

        public override void Execute()
        {
            // We need a handle to keep the method synchronously
            var handle = new ManualResetEvent(false);

            var client = new WebClient();

            client.DownloadFileCompleted += (sender, args) => handle.Set();
            client.DownloadProgressChanged +=
                (sender, args) => this.OnProgressChanged(new ProgressEventArgs(args.ProgressPercentage));

            client.DownloadFileAsync(new Uri(this.Video.DownloadUrl), this.SavePath);

            handle.WaitOne();
            handle.Close();

            this.OnDownloadFinished(EventArgs.Empty);
        }
    }
}