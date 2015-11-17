// ****************************************************************************
//
// YoutubeExtractorUtil
// Copyright (C) 2013-2015 Dennis Daume (daume.dennis@gmail.com)
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// ****************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using YoutubeExtractor;

namespace YoutubeExtractorUtil
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                commandLine.ProcessArgs(args);

                Console.WriteLine(Properties.Resources.Header);
                Console.WriteLine(commandLine);

                if (commandLine.Destination != null)
                {
                    Directory.CreateDirectory(commandLine.Destination);
                }

                var links = commandLine.Links.Concat(ScanLinksFile()).Distinct(StringComparer.CurrentCultureIgnoreCase);

                if (commandLine.IsHelp || !links.Any())
                {
                    Console.WriteLine(Properties.Resources.Help);
                    return;
                }

                foreach (var link in links)
                {
                    try
                    {
                        DownloadVideo(link);
                    }
                    catch (Exception exception)
                    {
                        OutputError(exception);
                    }

                    Console.WriteLine();
                }
            }
            catch (Exception exception)
            {
                OutputError(exception);
            }

            Console.WriteLine("Done");
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

        private static void OutputError(Exception exception)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(exception);
            Console.ResetColor();
        }

        private static void DownloadVideo(string link)
        {
            Console.WriteLine("Processing: \"{0}\":", link);

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

            if (commandLine.IdealResolution != 0)
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
                .FirstOrDefault();

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
        }

        private static string VideoInfoDisplayString(VideoInfo videoInfo)
        {
            return string.Format("VideoType: {0,-6} Res: {1}p{2},\tAudio: {3} {4}{5}",
                videoInfo.VideoType,
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
            return Regex.Replace(path, illegalPathCharacters, "");
        }

        private static int cursorTop;
        private static readonly CommandLine commandLine = new CommandLine();
        private static readonly string illegalPathCharacters = "[" + Regex.Escape(new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars())) + "]";
    }
}
