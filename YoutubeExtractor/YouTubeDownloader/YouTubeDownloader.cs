using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using YoutubeExtractor;

namespace YouTubeDownloader
{
    public partial class Frm_DwnYouTube : Form
    {
        // To Report Download Progress to GUI
        static BackgroundWorker background_download;   
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

        #region Initialize

        public Frm_DwnYouTube()
        {
            InitializeComponent();
            background_download = new BackgroundWorker();
            background_download.DoWork += Background_download_DoWork;
            background_download.ProgressChanged += Background_download_ProgressChanged;
            background_download.RunWorkerCompleted += Background_download_RunWorkerCompleted;
            background_download.WorkerReportsProgress = true;
            init();
        }

        #endregion

        private void Background_download_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            processExitCode((int)e.Result);
            cmd_Download.Enabled = true;
            cmd_DownloadAudio.Enabled = true;
        }

        private void Background_download_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            prgbar_Download.Value = e.ProgressPercentage;
        }

        private void Background_download_DoWork(object sender, DoWorkEventArgs e)
        {
            string[] args = (string[])e.Argument;

            try
            {
                if (args[0].Equals("VIDEO"))
                {
                    e.Result = StartDownloader("VIDEO", args[1], args[2]);
                }
                else
                {
                    e.Result = StartDownloader("AUDIO", args[1], args[2]);
                }
            }
            catch
            {

            }
        }

        private static void VideoDownloader_DownloadProgressChanged(object sender, ProgressEventArgs e)
        {
            background_download.ReportProgress((int)e.ProgressPercentage);
        }

        private static void AudioDownloader_DownloadProgressChanged(object sender, ProgressEventArgs e)
        {
            background_download.ReportProgress((int)(e.ProgressPercentage * 0.85));
        }

        #region Button Events

        private void cmd_Download_Click(object sender, EventArgs e)
        {
            string apppath = AppDomain.CurrentDomain.BaseDirectory;
            string dwnload_path = String.Empty;


            if (txt_Url.Text.ToString().Length == 0)
            {
                MessageBox.Show("Please input the Youtube Url");
                return;
            }

            DialogResult result = this.folderBrowserYoutube.ShowDialog();

            if (result == DialogResult.OK)
            {
                txt_location.Text = this.folderBrowserYoutube.SelectedPath.ToString();
            }

            dwnload_path = txt_location.Text.ToString();

            cmd_Download.Enabled = false;
            cmd_DownloadAudio.Enabled = false;

            background_download.RunWorkerAsync(new string[] { "VIDEO", apppath, dwnload_path });

        }

        private void cmd_DownloadAudio_Click(object sender, EventArgs e)
        {
            string apppath = AppDomain.CurrentDomain.BaseDirectory;
            string dwnload_path = String.Empty;

            if (txt_Url.Text.ToString().Length == 0)
            {
                MessageBox.Show("Please input the Youtube Url");
                return;
            }

            DialogResult result = this.folderBrowserYoutube.ShowDialog();

            if (result == DialogResult.OK)
            {
                txt_location.Text = this.folderBrowserYoutube.SelectedPath.ToString();
            }

            dwnload_path = txt_location.Text.ToString();

            background_download.RunWorkerAsync(new string[] { "AUDIO", apppath, dwnload_path });

        }

        private void cmd_Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        #endregion

        #region Functions

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
            audioDownloader.DownloadProgressChanged += AudioDownloader_DownloadProgressChanged;

            //audioDownloader.AudioExtractionProgressChanged += (sender, args) => Console.WriteLine(85 + args.ProgressPercentage * 0.15);

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
            //videoDownloader.DownloadProgressChanged += (sender, args) => Console.WriteLine((args.ProgressPercentage));
            videoDownloader.DownloadProgressChanged += VideoDownloader_DownloadProgressChanged;

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

        private  int StartDownloader(string AV, string path, string dwnloaddir)
        {
            IEnumerable<VideoInfo> videoInfos;
            string link = String.Empty;
            string[] normlink;
            bool exists = false;

            try
            {
                path = System.IO.Path.GetFullPath(dwnloaddir.ToString());
                DirectoryInfo info = new DirectoryInfo(path);
                exists = info.Exists;
            }

            catch (Exception ex)
            {
                WriteLog(ex.Message);
                return (int)OTHER_ERR;
            }

            if (!exists)
            {
                WriteLog("Please check the path");
                return (int) DWNPATH_ERR;
            }

            try
            {
                if ((txt_Url.Text.ToString().ToLower().Contains("http://www.youtube.com/watch?v=")) || (txt_Url.Text.ToString().ToLower().Contains("https://www.youtube.com/watch?v=")))
                {

                    normlink = txt_Url.Text.ToString().Split('&');
                    link = normlink[0].ToString();

                    videoInfos = DownloadUrlResolver.GetDownloadUrls(link);

                    if (AV.ToString().ToUpper() == "VIDEO")
                    {
                        DownloadVideo(videoInfos, path);
                        return 0;
                    }

                    if (AV.ToString().ToUpper() == "AUDIO")
                    {
                        DownloadAudio(videoInfos, path);
                        return 0;
                    }

                    if ((AV.Length == 0) || (AV.ToString().ToUpper() != "AUDIO") || (AV.ToString().ToUpper() != "VIDEO"))
                    {
                        WriteLog("Please Specify the Format AUDIO/VIDEO");
                        return (int)AVURL_ERR;
                    }
                }

                else
                {
                    WriteLog("Youtube URL not in correct format");
                    return (int)YLINKFORMAT_ERR;
                }
            }

            catch (Exception ex)
            {
                WriteLog(ex.Message);
                return (int)OTHER_ERR;
            }

            return (int)OTHER_ERR;

        }

        private void processExitCode(int retval)
        {
            if (retval == 0)
            {
                MessageBox.Show("Download Successful");
            }

            if ((retval < 5) && (retval != 0))
            {
                MessageBox.Show("Please check the arguments file path or youtube url maybe incorrect");
            }

            if ((retval > 5) && (retval <= 10))
            {
                MessageBox.Show("Please check the file path");
            }

            if ((retval > 5) && (retval > 10) && (retval <= 15))
            {
                MessageBox.Show("Please check the youtube Url or Internet connection");
            }

            if ((retval > 5) && (retval > 10) && (retval > 15) && (retval == 99))
            {
                MessageBox.Show("Please check the parameters");
            }
        }

        private void init()
        {
            lbl_Resolution.Enabled = false;
            cb_Resolution.Enabled = false;
            this.folderBrowserYoutube.ShowNewFolderButton = false;
            this.folderBrowserYoutube.RootFolder = System.Environment.SpecialFolder.MyComputer;
        }


        #endregion

        private void txt_Url_TextChanged(object sender, EventArgs e)
        {
            if (txt_Url.Text.Length > 5)
            {
                lbl_Resolution.Enabled = true;
                cb_Resolution.Enabled = true;
                IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(txt_Url.Text);
                foreach (VideoInfo videoInfo in videoInfos)
                {

                }
            }
        }

    }
}
