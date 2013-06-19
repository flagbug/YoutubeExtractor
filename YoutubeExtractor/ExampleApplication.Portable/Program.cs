using System;
using System.Collections.Generic;
using YoutubeExtractor;

namespace ExampleApplication.Portable
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls("http://www.youtube.com/watch?v=6bMmhKz6KXg");

            foreach (VideoInfo videoInfo in videoInfos)
            {
                Console.WriteLine(videoInfo.DownloadUrl);
                Console.WriteLine();
            }

            Console.ReadLine();
        }
    }
}