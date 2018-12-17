using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace YouTubeDownloader
{
    public partial class Frm_DwnYouTube : Form
    {

        #region Initialize

        public Frm_DwnYouTube()
        {
            InitializeComponent();
            init();
        } 

        #endregion

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

            startDownloader("VIDEO", "ExampleApplication.exe", apppath, dwnload_path);

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

            startDownloader("AUDIO", "ExampleApplication.exe", apppath, dwnload_path);

        }

        private void cmd_Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        } 

        #endregion

        #region Functions

        private void startDownloader(string AV, string appname, string path, string dwnloaddir)
        {
            int retCode;

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = path + appname;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = txt_Url.Text.ToString().TrimEnd() + " " + AV.ToUpper().TrimEnd() + " " + dwnloaddir;

            //ServiceName + " " + "\"" + @CurPath + "\\\"" + " " + "\"" + @UpdatePath + "\\\"";

            try
            {
                using (Process exeProcess = Process.Start(startInfo))
                {
                    cmd_Download.Enabled = false;
                    cmd_DownloadAudio.Enabled = false;
                    exeProcess.WaitForExit();
                    retCode = exeProcess.ExitCode;
                    processExitCode(retCode);
                }

                cmd_DownloadAudio.Enabled = true;
                cmd_Download.Enabled = true;
            }

            catch (Exception)
            {
                //ErrorLog
            }

        }

        private void processExitCode(int retval)
        {
            if(retval == 0)
            {
                MessageBox.Show("Download Successful");
            }

            if((retval < 5) && (retval != 0))
            {
                MessageBox.Show("Please check the arguments file path or youtube url maybe incorrect");
            }

            if((retval > 5) && (retval <=10))
            {
                MessageBox.Show("Please check the file path");
            }

            if((retval > 5)&&(retval > 10)&&(retval <=15))
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


    }
}
