using System;
using System.IO;

namespace YoutubeExtractor
{
    /// <summary>
    /// Provides a method to download a video and extract its audio track.
    /// </summary>
    public class AudioDownloader : Downloader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AudioDownloader"/> class.
        /// </summary>
        /// <param name="video">The video to convert.</param>
        /// <param name="savePath">The path to save the audio.</param>
        public AudioDownloader(VideoInfo video, string savePath)
            : base(video, savePath)
        { }

        /// <summary>
        /// Starts the download and then extracts the audio track of the video.
        /// </summary>
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