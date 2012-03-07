using System;
using System.IO;

namespace YoutubeExtractor
{
    public class AudioDownloader : Downloader
    {
        public AudioDownloader(VideoInfo video, string savePath)
            : base(video, savePath)
        {
        }

        public override void Execute()
        {
            string tempPath = Path.GetTempFileName();

            var videoDownloader = new VideoDownloader(this.Video, tempPath);

            videoDownloader.ProgressChanged +=
                (sender, args) => this.OnProgressChanged(new ProgressEventArgs(args.ProgressPercentage / 2));

            videoDownloader.Execute();

            var flvFile = new FlvFile(tempPath, this.SavePath);

            flvFile.ExtractStreams();

            this.OnDownloadFinished(EventArgs.Empty);
        }
    }
}