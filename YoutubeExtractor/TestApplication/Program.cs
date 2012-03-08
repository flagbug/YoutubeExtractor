using System;
using System.Collections.Generic;
using System.Linq;
using YoutubeExtractor;

namespace TestApplication
{
    internal class Program
    {
        private static void Main()
        {
            // Our test youtube link
            const string link = "http://www.youtube.com/watch?v=6bMmhKz6KXg";

            // Get the available video formats
            IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link);

            /*
             * We want the first flash (only flash audio extraction is currently supported)
             * video with the highest audio quality.
             * See the VideoFormat enum for more info about the quality.
             */
            VideoInfo video = videoInfos.First(info =>
                info.VideoFormat == VideoFormat.FlashAacHighQuality ||
                info.VideoFormat == VideoFormat.FlashAacLowQuality ||
                info.VideoFormat == VideoFormat.FlashMp3HighQuality ||
                info.VideoFormat == VideoFormat.FlashMp3LowQuality);

            /*
             * Create the audio downloader.
             * The first argument is the video tpo extract the audio.
             * The second argument is the path to save the audio file.
             * Automatic video title infering will be supported later.
             * */
            var videoDownloader = new AudioDownloader(video, "D:/Downloads/test");

            // Register the ProgressChanged event and print the current progress
            videoDownloader.ProgressChanged += (sender, args) => Console.WriteLine(args.ProgressPercentage);

            /*
             * Execute the video downloader.
             * For GUI applications note that this method runs synchronously
             */
            videoDownloader.Execute();
        }
    }
}