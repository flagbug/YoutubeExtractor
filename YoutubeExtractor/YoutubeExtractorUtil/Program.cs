﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using YoutubeExtractor;

//
// Example:
//     YoutubeExtractorUtil http://www.youtube.com/watch?v=O3UBOOZw-FE /IdealResolution=1080 /ExtractAudio /Destination=.
//

namespace YoutubeExtractorUtil
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            commandLine.ProcessArgs(args);

            Console.WriteLine("YoutubeExtractorUtil - Sample util application\n");
            Console.WriteLine(commandLine);

            if (commandLine.IsHelp)
            {
                Console.WriteLine("Usage...");
                return;
            }

            foreach (var link in commandLine.Links.Concat(ScanLinksFile()).Distinct(StringComparer.CurrentCultureIgnoreCase))
            {
                try
                {
                    DownloadVideo(link);
                }
                catch (Exception exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(exception);
                    Console.ResetColor();
                }
            }

            Console.WriteLine("Done");
        }

        private static void DownloadVideo(string link)
        {
            Console.WriteLine("Processing: \"{0}\":", link);
            return;

            IEnumerable<VideoInfo> allVideoInfos = DownloadUrlResolver.GetDownloadUrls(link, false);

            // Filter out types not in VideoTypes and sort by preference
            var items = allVideoInfos
                .Select(info => new
                {
                    Info = info,
                    TypePreferenceIndex = Array.IndexOf(commandLine.VideoTypes, info.VideoType)
                })
                .Where(item => item.TypePreferenceIndex >= 0)
                .OrderBy(item => item.TypePreferenceIndex);

            // Confine query to resolution boundaries
            IEnumerable<VideoInfo> videoInfos = items
                .Select(item => item.Info)
                .Where(info => info.Resolution >= commandLine.MinResolution && info.Resolution <= commandLine.MaxResolution)
                .OrderByDescending(info => info.Resolution);

            VideoInfo selectedVideoFile = null;

            if (commandLine.IdealResolution != -1)
            {
                // Look for ideal resolution first (if specified)
                selectedVideoFile = videoInfos.FirstOrDefault(info => info.Resolution == commandLine.IdealResolution);
            }

            if (selectedVideoFile == null)
            {
                // If ideal res was either not specified or not found go for the next best resolution available
                selectedVideoFile = videoInfos.FirstOrDefault();
            }

            // Find the best audio available
            VideoInfo selectedAudeoFile = allVideoInfos
                .Where(info => info.CanExtractAudio)
                .OrderByDescending(info => info.AudioBitrate)
                .First();

            // List the files that were cnsidered and highlight the ones that were chosen
            foreach (var item in items)
            {
                if (object.ReferenceEquals(selectedVideoFile, item.Info) || object.ReferenceEquals(selectedAudeoFile, item.Info))
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("{0}", VideoInfoDisplayString(item.Info));
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine("{0}", VideoInfoDisplayString(item.Info));
                }
            }

            // Do we need to downoad a separate file for extracting audio or reuse the downloaded video file
            bool separateDownloads = !object.ReferenceEquals(selectedVideoFile, selectedAudeoFile);

            if (selectedVideoFile.RequiresDecryption)
            {
                // If the video has a decrypted signature, decipher it
                DownloadUrlResolver.DecryptDownloadUrl(selectedVideoFile);
            }

            // Video
            string videoFilePathName = null;
            if (selectedVideoFile != null)
            {
                string destination = commandLine.Destination != null ?
                    commandLine.Destination :
                    Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\..\\Videos");

                videoFilePathName = Path.Combine(destination, RemoveIllegalPathCharacters(selectedVideoFile.Title) + selectedVideoFile.VideoExtension);
                Console.WriteLine("Path: \"{0}\"", videoFilePathName);

                VideoDownloader videoDownloader = new VideoDownloader(selectedVideoFile, videoFilePathName);
                videoDownloader.DownloadProgressChanged += DownloadProgress;

                Console.WriteLine("Downloading video...");
                cursorTop = Console.CursorTop;

                videoDownloader.Execute();
            }

            // Audio
            if (commandLine.ExtractAudio && selectedAudeoFile != null)
            {
                string destination = commandLine.Destination != null ?
                    commandLine.Destination :
                    Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

                string audeoFilePathName = Path.Combine(destination, RemoveIllegalPathCharacters(selectedAudeoFile.Title) + selectedAudeoFile.AudioExtension);
                Console.WriteLine("Path: \"{0}\"", audeoFilePathName);

                AudioDownloader audioDownloader = new AudioDownloader(selectedAudeoFile, audeoFilePathName);
                audioDownloader.AudioExtractionProgressChanged += DownloadProgress;

                if (separateDownloads || videoFilePathName == null)
                {
                    audioDownloader.DownloadProgressChanged += DownloadProgress;

                    string tempPath = Path.GetTempFileName();
                    try
                    {
                        Console.WriteLine("Downloading audio...");
                        cursorTop = Console.CursorTop;

                        audioDownloader.DownloadVideo(tempPath);

                        Console.WriteLine("Extracting audio...");
                        cursorTop = Console.CursorTop;

                        audioDownloader.ExtractAudio(tempPath);
                    }
                    finally
                    {
                        File.Delete(tempPath);
                    }
                }
                else
                {
                    cursorTop = Console.CursorTop;
                    Console.WriteLine("Extracting audio...");

                    audioDownloader.ExtractAudio(videoFilePathName);
                }
            }

            Console.WriteLine();
        }

        private static string VideoInfoDisplayString(VideoInfo videoInfo)
        {
            return string.Format("\"{0}{1}\",\tRes: {2}p{3},\tAudio: {4} {5}{6}",
                videoInfo.Title,
                videoInfo.VideoExtension,
                videoInfo.Resolution,
                videoInfo.Is3D ? " 3D" : "",
                videoInfo.AudioType,
                videoInfo.AudioBitrate,
                videoInfo.CanExtractAudio ? " (extractable)" : "");
        }

        private static void DownloadProgress(object sender, ProgressEventArgs e)
        {
            Console.CursorTop = cursorTop;
            Console.CursorLeft = 0;
            Console.WriteLine("{0:0.00}%", e.ProgressPercentage);
        }

        private static string RemoveIllegalPathCharacters(string path)
        {
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(path, "");
        }

        private static IEnumerable<string> ScanLinksFile()
        {
            if (string.IsNullOrEmpty(commandLine.LinksFile)) { yield break; }

            using (var reader = new StreamReader(commandLine.LinksFile))
            {
                string line;
                while (null != (line = reader.ReadLine()))
                {
                    Uri uri;
                    if (Uri.TryCreate(line, UriKind.Absolute, out uri))
                    {
                        yield return line;
                    }
                }
            }
        }

        private static int cursorTop;
        private static readonly CommandLine commandLine = new CommandLine();
    }
}
