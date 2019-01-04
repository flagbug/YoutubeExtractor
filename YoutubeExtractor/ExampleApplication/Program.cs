using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YoutubeExtractor;

namespace ExampleApplication
{
    internal class Program
    {
        /*ARGUMENTS RELATED ERRORS 1-5*/
        const uint LESSARGS_ERR = 1;
        const uint YLINK_ERR = 2;
        const uint DWNPATH_ERR = 3;
        const uint AVURL_ERR = 4;
        
        /*FILE RELATED ERRORS 5-10*/
        const uint FEXP_ERR = 5;
        const uint FPATH_ERR = 6;

        /*YOUTUBE LINK RELATED ERRORS 10-15*/
        const uint YLINKFORMAT_ERR = 10;
        const uint NETCON_ERR = 11;

        /*OTHER ERRORS*/
        const uint OTHER_ERR = 99;

        private static void DownloadAudio(IEnumerable<VideoInfo> videoInfos, string dwnpath)
        {
            /*
             * We want the first extractable video with the highest audio quality.
             */
            VideoInfo video = videoInfos
                .Where(info => info.CanExtractAudio)
                .OrderByDescending(info => info.AudioBitrate)
                .First();

            /*
             * Create the audio downloader.
             * The first argument is the video where the audio should be extracted from.
             * The second argument is the path to save the audio file.
             */

            var audioDownloader = new AudioDownloader(video,
                Path.Combine(dwnpath, Clean_Title(video.Title) + video.AudioExtension));

            // Register the progress events. We treat the download progress as 85% of the progress
            // and the extraction progress only as 15% of the progress, because the download will
            // take much longer than the audio extraction.
            audioDownloader.DownloadProgressChanged += (sender, args) => Console.WriteLine(args.ProgressPercentage * 0.85);

            audioDownloader.AudioExtractionProgressChanged += (sender, args) => Console.WriteLine(85 + args.ProgressPercentage * 0.15);

            /*
             * Execute the audio downloader.
             * For GUI applications note, that this method runs synchronously.
             */
            audioDownloader.Execute();
        }

        private static void DownloadVideo(IEnumerable<VideoInfo> videoInfos, string dwnpath)
        {
            /*
             * Select the first .mp4 video with 360p resolution
             */
            VideoInfo video = videoInfos
                .First(info => info.VideoType == VideoType.Mp4 && info.Resolution == 360);

            /*
             * Create the video downloader.
             * The first argument is the video to download.
             * The second argument is the path to save the video file.
             */

            var videoDownloader = new VideoDownloader(video,
                Path.Combine(dwnpath, Clean_Title(video.Title) + video.VideoExtension));

            // Register the ProgressChanged event and print the current progress
            videoDownloader.DownloadProgressChanged += (sender, args) => Console.WriteLine(args.ProgressPercentage);

            /*
             * Execute the video downloader.
             * For GUI applications note, that this method runs synchronously.
             */
            videoDownloader.Execute();
        }

        /// <summary>
        ///  Used to maintain the log of the Current service.
        ///  Inputs: strmessage as a string.
        ///  Outputs: A log file containing the status of the Service.
        ///  Notes:
        /// </summary>
        public static void WriteLog(string strMessage)
        {
            string Dt4 = DateTime.Now.AddDays(-2).ToString("dd/MM/yyyy").Substring(0, 2);
            string Dt5 = DateTime.Now.AddDays(-2).ToString("dd/MM/yyyy").Substring(3, 2);
            string Dt6 = DateTime.Now.AddDays(-2).ToString("dd/MM/yyyy").Substring(6, 4);

            string yestPath = AppDomain.CurrentDomain.BaseDirectory + "\\YouTubeDownloader" + Dt4 + Dt5 + Dt6 + ".log";

            if (File.Exists(yestPath))
            {
                File.Delete(yestPath);
            }

            string strPath = null;
            System.IO.StreamWriter file = null;
            string Dt1 = DateTime.Now.Date.ToString("dd/MM/yyyy").Substring(0, 2);
            string Dt2 = DateTime.Now.Date.ToString("dd/MM/yyyy").Substring(3, 2);
            string Dt3 = DateTime.Now.Date.ToString("dd/MM/yyyy").Substring(6, 4);

            strPath = AppDomain.CurrentDomain.BaseDirectory + "\\YouTubeDownloader" + Dt1 + Dt2 + Dt3 + ".log";

            // 06/05/2014 Changes Anil Nair
            try
            {
                file = new System.IO.StreamWriter(strPath, true);
                file.WriteLine(strMessage);
                file.Close();
            }
            catch (IOException)
            {
                file.Flush();
            }
        }

        private static string Clean_Title(string video_title)
        {
            return Path.GetInvalidFileNameChars().Aggregate(video_title, (current, c) => current.Replace(c.ToString(), string.Empty));
        }

        private static int Main(string[] args)
        {
            IEnumerable<VideoInfo> videoInfos;
            string link = String.Empty;
            string[] normlink;
            bool exists;
            string path = String.Empty;

            if (args.Length < 3)
            {
                WriteLog("Please Specify 3 arguments");
                Console.WriteLine("Please Specify 3 arguments");
                return (int)LESSARGS_ERR;
            }

            if (args[0].Length == 0)
            {
                WriteLog("Please Specify the Youtube Link");
                Console.WriteLine("Please Specify the Youtube Link");
                return (int)YLINK_ERR;
            }

            if (args[2].Length == 0)
            {
                WriteLog("Please Specify the Download Path");
                Console.WriteLine("Please Specify the Download Path");
                return (int)DWNPATH_ERR;
            }

            
            else
            {
                try
                {
                    path = System.IO.Path.GetFullPath(args[2].ToString());
                    DirectoryInfo info = new DirectoryInfo(path);
                    exists = info.Exists;
                }

                catch (Exception ex)
                {
                    WriteLog(ex.Message);
                    return (int)FEXP_ERR;
                }
            }

            if (!exists)
            {
                WriteLog("Please check the path");
                Console.WriteLine("Please check the path");
                return (int)FPATH_ERR;
            } 
	
         try
         {
                if ((args[0].ToString().ToLower().Contains("http://www.youtube.com/watch?v=")) || (args[0].ToString().ToLower().Contains("https://www.youtube.com/watch?v=")))
                {

                    normlink = args[0].ToString().Split('&');
                    link = normlink[0].ToString();

                    videoInfos = DownloadUrlResolver.GetDownloadUrls(link);
                
                    if (args[1].ToString().ToUpper() == "VIDEO")
                    {
                        Console.WriteLine(path);
                        DownloadVideo(videoInfos, path);
                        Console.WriteLine(args[2].ToString());
                        return 0;
                    }

                    if (args[1].ToString().ToUpper() == "AUDIO")
                    {
                        DownloadAudio(videoInfos, path);
                        Console.WriteLine(args[2].ToString());
                        return 0;
                    }

                    if ((args[1].Length == 0) || (args[1].ToString().ToUpper() != "AUDIO") || (args[1].ToString().ToUpper() != "VIDEO"))
                    {
                        WriteLog("Please Specify the Format AUDIO/VIDEO");
                        Console.WriteLine("Please Specify the Format AUDIO/VIDEO");
                        return (int)AVURL_ERR;
                    }
                }

                else
                {
                    WriteLog("Youtube URL not in correct format");
                    Console.WriteLine("Youtube URL not in correct format");
                    return (int)YLINKFORMAT_ERR;
                }
        }

        catch(Exception ex)
        {
            WriteLog(ex.Message);
            Console.WriteLine("Please check the internet connection");
            return (int)NETCON_ERR;
        }

        Console.WriteLine("Did not pass the validation checks please retify any mistakes in Parameters");
        return (int)OTHER_ERR;

        }

    }
}