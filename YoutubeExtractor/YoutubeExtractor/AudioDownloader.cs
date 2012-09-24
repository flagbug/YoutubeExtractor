using System;
using System.IO;
using System.Net;

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
        /// Downloads the video from YouTube and then extracts the audio track out if it.
        /// </summary>
        /// <exception cref="IOException">
        /// The temporary video file could not be created.
        /// - or -
        /// The audio file could not be created.
        /// </exception>
        /// <exception cref="AudioExtractionException">An error occured during audio extraction.</exception>
        /// <exception cref="WebException">An error occured while downloading the video.</exception>
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