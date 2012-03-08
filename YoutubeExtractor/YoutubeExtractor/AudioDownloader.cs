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

            this.DownloadVideo(tempPath);

            this.ExtractAudio(tempPath);

            this.OnDownloadFinished(EventArgs.Empty);
        }

        private void DownloadVideo(string path)
        {
            var videoDownloader = new VideoDownloader(this.Video, path);

            videoDownloader.ProgressChanged +=
                (sender, args) => this.OnProgressChanged(new ProgressEventArgs(args.ProgressPercentage / 2));

            videoDownloader.Execute();
        }

        private void ExtractAudio(string path)
        {
            var flvFile = new FlvFile(path, this.SavePath);

            flvFile.ConversionProgressChanged +=
                (sender, args) => this.OnProgressChanged(new ProgressEventArgs(50 + args.ProgressPercentage / 2));

            flvFile.ExtractStreams();
        }
    }
}