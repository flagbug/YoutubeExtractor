using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeExtractor;

namespace ExampleApplication
{
    class Program
    {
        private static string downloadPath;
        private static void Main()
        {
            Program.AsyncMain().Wait();
        }

        private static async Task AsyncMain()
        {
            downloadPath = $"{Environment.GetEnvironmentVariable(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "LocalAppData" : "HOME")}/Movies/"; //Tested in OSX
            Console.WriteLine($"Download Path:{downloadPath}");

            string option = "n";
            List<string> url = new List<string>();
            List<Task> tasks = new List<Task>();
            do
            {
                Console.WriteLine("Add Url");
                url.Add(Console.ReadLine());
                Console.WriteLine("Add More? y or n");
                option = Console.ReadLine().ToLower();

            } while (option != "n");

            foreach (var item in url)
            {
                tasks.Add(Task.Run(async () =>
                    {
                        IEnumerable<VideoInfo> videoInfos = await DownloadUrlResolver.GetDownloadUrls(item, false);
                        await DownloadVideo(videoInfos);
                    }));
            }
            await Task.WhenAll(tasks);
        }


        private static string RemoveIllegalPathCharacters(string path)
        {
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(path, "");
        }
        private static async Task DownloadAudio(IEnumerable<VideoInfo> videoInfos)
        {
            /*
             * We want the first extractable video with the highest audio quality.
             */
            VideoInfo video = videoInfos
                .Where(info => info.CanExtractAudio)
                .OrderByDescending(info => info.AudioBitrate)
                .First();

            /*
             * If the video has a decrypted signature, decipher it
             */
            if (video.RequiresDecryption)
            {
                await DownloadUrlResolver.DecryptDownloadUrl(video);
            }

            /*
             * Create the audio downloader.
             * The first argument is the video where the audio should be extracted from.
             * The second argument is the path to save the audio file.
             */

            var audioDownloader = new AudioDownloader(video, $"{downloadPath}{RemoveIllegalPathCharacters(video.Title)} {video.AudioExtension}");

            // Register the progress events. We treat the download progress as 85% of the progress
            // and the extraction progress only as 15% of the progress, because the download will
            // take much longer than the audio extraction.
            audioDownloader.DownloadProgressChanged += (sender, args) => Console.WriteLine(args.ProgressPercentage * 0.85);
            audioDownloader.AudioExtractionProgressChanged += (sender, args) => Console.WriteLine(85 + args.ProgressPercentage);

            /*
             * Execute the audio downloader.
             * For GUI applications note, that this method runs synchronously.
             */
            await audioDownloader.Execute();
        }

        private static async Task DownloadVideo(IEnumerable<VideoInfo> videoInfos)
        {

            var video = videoInfos.OrderByDescending(x => x.Resolution).FirstOrDefault(info => info.VideoType == VideoType.Mp4);

            if (video is null)
            {
                Console.WriteLine("Not Found");
            }

            Console.WriteLine($"Video: {video.Title}{video.VideoExtension} -> Resolution: {video.Resolution}");

            if (video.RequiresDecryption)
            {
                await DownloadUrlResolver.DecryptDownloadUrl(video);
            }

            /*
             * Create the video downloader.
             * The first argument is the video to download.
             * The second argument is the path to save the video file.
             */
            var videoDownloader = new VideoDownloader(video, $"{downloadPath}{RemoveIllegalPathCharacters(video.Title)}{video.VideoExtension}");

            // Register the ProgressChanged event and print the current progress
            videoDownloader.DownloadProgressChanged += (sender, args) => Console.WriteLine($"{video.Title} -> {(int)args.ProgressPercentage}");

            /*
             * Execute the video downloader.
             * For GUI applications note, that this method runs synchronously.
             */
            await videoDownloader.Execute();
        }
    }
}
