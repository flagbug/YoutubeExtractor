using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YoutubeExtractor;

namespace ExampleApplication
{
    internal class Program
    {
        private static void Main()
        {
            // Our test youtube link
            const string link = "http://www.youtube.com/watch?v=6bMmhKz6KXg";

            /*
             * Get the available video formats.
             * We'll work with them in the video and audio download examples.
             */
            IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link);

            //DownloadAudio(videoInfos);
            //DownloadVideo(videoInfos);
        }

        private static void DownloadAudio(IEnumerable<VideoInfo> videoInfos)
        {
            /*
             * We want the first flash (only flash audio extraction is currently supported)
             * video with the highest audio quality.
             * See the VideoFormat enum for more info about the quality.
             */
            VideoInfo video = videoInfos
                .Where(info => info.CanExtractAudio)
                .First(info =>
                       info.VideoFormat == VideoFormat.FlashAacHighQuality ||
                       info.VideoFormat == VideoFormat.FlashAacLowQuality ||
                       info.VideoFormat == VideoFormat.FlashMp3HighQuality ||
                       info.VideoFormat == VideoFormat.FlashMp3LowQuality);

            /*
             * Create the audio downloader.
             * The first argument is the video where the audio should be extracted from.
             * The second argument is the path to save the audio file.
             */
            var audioDownloader = new AudioDownloader(video, Path.Combine("D:/Downloads", video.Title + video.AudioExtension));

            // Register the ProgressChanged event and print the current progress
            audioDownloader.ProgressChanged += (sender, args) => Console.WriteLine(args.ProgressPercentage);

            /*
             * Execute the audio downloader.
             * For GUI applications note, that this method runs synchronously.
             */
            audioDownloader.Execute();
        }

        private static void DownloadVideo(IEnumerable<VideoInfo> videoInfos)
        {
            /*
             * Select the standard youtube quality
             * See the VideoFormat enum for more info about the quality.
             */
            VideoInfo video = videoInfos
                .First(info => info.VideoFormat == VideoFormat.Standard360);

            /*
             * Create the video downloader.
             * The first argument is the video to download.
             * The second argument is the path to save the video file.
             */
            var videoDownloader = new VideoDownloader(video, Path.Combine("D:/Downloads", video.Title + video.VideoExtension));

            // Register the ProgressChanged event and print the current progress
            videoDownloader.ProgressChanged += (sender, args) => Console.WriteLine(args.ProgressPercentage);

            /*
             * Execute the video downloader.
             * For GUI applications note, that this method runs synchronously.
             */
            videoDownloader.Execute();
        }
    }
}