namespace YouTubeDownloader
{
    partial class Frm_DwnYouTube
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cmd_Download = new System.Windows.Forms.Button();
            this.txt_Url = new System.Windows.Forms.TextBox();
            this.cb_Resolution = new System.Windows.Forms.ComboBox();
            this.cmd_DownloadAudio = new System.Windows.Forms.Button();
            this.txt_location = new System.Windows.Forms.TextBox();
            this.lbl_locationSave = new System.Windows.Forms.Label();
            this.Lbl_youtubeUrl = new System.Windows.Forms.Label();
            this.lbl_Resolution = new System.Windows.Forms.Label();
            this.cmd_Exit = new System.Windows.Forms.Button();
            this.pnl_YouTubeDownloader = new System.Windows.Forms.Panel();
            this.folderBrowserYoutube = new System.Windows.Forms.FolderBrowserDialog();
            this.pnl_YouTubeDownloader.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmd_Download
            // 
            this.cmd_Download.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmd_Download.Location = new System.Drawing.Point(6, 149);
            this.cmd_Download.Name = "cmd_Download";
            this.cmd_Download.Size = new System.Drawing.Size(172, 39);
            this.cmd_Download.TabIndex = 0;
            this.cmd_Download.Text = "Download Video";
            this.cmd_Download.UseVisualStyleBackColor = true;
            this.cmd_Download.Click += new System.EventHandler(this.cmd_Download_Click);
            // 
            // txt_Url
            // 
            this.txt_Url.Location = new System.Drawing.Point(149, 60);
            this.txt_Url.Name = "txt_Url";
            this.txt_Url.Size = new System.Drawing.Size(315, 20);
            this.txt_Url.TabIndex = 1;
            // 
            // cb_Resolution
            // 
            this.cb_Resolution.FormattingEnabled = true;
            this.cb_Resolution.Location = new System.Drawing.Point(149, 103);
            this.cb_Resolution.Name = "cb_Resolution";
            this.cb_Resolution.Size = new System.Drawing.Size(315, 21);
            this.cb_Resolution.TabIndex = 2;
            // 
            // cmd_DownloadAudio
            // 
            this.cmd_DownloadAudio.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmd_DownloadAudio.Location = new System.Drawing.Point(292, 149);
            this.cmd_DownloadAudio.Name = "cmd_DownloadAudio";
            this.cmd_DownloadAudio.Size = new System.Drawing.Size(172, 42);
            this.cmd_DownloadAudio.TabIndex = 3;
            this.cmd_DownloadAudio.Text = "Download Audio";
            this.cmd_DownloadAudio.UseVisualStyleBackColor = true;
            this.cmd_DownloadAudio.Click += new System.EventHandler(this.cmd_DownloadAudio_Click);
            // 
            // txt_location
            // 
            this.txt_location.Location = new System.Drawing.Point(149, 12);
            this.txt_location.Name = "txt_location";
            this.txt_location.Size = new System.Drawing.Size(315, 20);
            this.txt_location.TabIndex = 4;
            // 
            // lbl_locationSave
            // 
            this.lbl_locationSave.AutoSize = true;
            this.lbl_locationSave.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_locationSave.Location = new System.Drawing.Point(3, 12);
            this.lbl_locationSave.Name = "lbl_locationSave";
            this.lbl_locationSave.Size = new System.Drawing.Size(135, 18);
            this.lbl_locationSave.TabIndex = 5;
            this.lbl_locationSave.Text = "Location To Save";
            // 
            // Lbl_youtubeUrl
            // 
            this.Lbl_youtubeUrl.AutoSize = true;
            this.Lbl_youtubeUrl.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Lbl_youtubeUrl.Location = new System.Drawing.Point(6, 60);
            this.Lbl_youtubeUrl.Name = "Lbl_youtubeUrl";
            this.Lbl_youtubeUrl.Size = new System.Drawing.Size(100, 18);
            this.Lbl_youtubeUrl.TabIndex = 6;
            this.Lbl_youtubeUrl.Text = "YouTube Url";
            // 
            // lbl_Resolution
            // 
            this.lbl_Resolution.AutoSize = true;
            this.lbl_Resolution.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_Resolution.Location = new System.Drawing.Point(6, 103);
            this.lbl_Resolution.Name = "lbl_Resolution";
            this.lbl_Resolution.Size = new System.Drawing.Size(88, 18);
            this.lbl_Resolution.TabIndex = 7;
            this.lbl_Resolution.Text = "Resolution";
            // 
            // cmd_Exit
            // 
            this.cmd_Exit.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmd_Exit.Location = new System.Drawing.Point(149, 208);
            this.cmd_Exit.Name = "cmd_Exit";
            this.cmd_Exit.Size = new System.Drawing.Size(172, 42);
            this.cmd_Exit.TabIndex = 8;
            this.cmd_Exit.Text = "Exit";
            this.cmd_Exit.UseVisualStyleBackColor = true;
            this.cmd_Exit.Click += new System.EventHandler(this.cmd_Exit_Click);
            // 
            // pnl_YouTubeDownloader
            // 
            this.pnl_YouTubeDownloader.Controls.Add(this.cmd_Exit);
            this.pnl_YouTubeDownloader.Controls.Add(this.lbl_Resolution);
            this.pnl_YouTubeDownloader.Controls.Add(this.Lbl_youtubeUrl);
            this.pnl_YouTubeDownloader.Controls.Add(this.lbl_locationSave);
            this.pnl_YouTubeDownloader.Controls.Add(this.txt_location);
            this.pnl_YouTubeDownloader.Controls.Add(this.cmd_DownloadAudio);
            this.pnl_YouTubeDownloader.Controls.Add(this.cb_Resolution);
            this.pnl_YouTubeDownloader.Controls.Add(this.txt_Url);
            this.pnl_YouTubeDownloader.Controls.Add(this.cmd_Download);
            this.pnl_YouTubeDownloader.Location = new System.Drawing.Point(12, 16);
            this.pnl_YouTubeDownloader.Name = "pnl_YouTubeDownloader";
            this.pnl_YouTubeDownloader.Size = new System.Drawing.Size(478, 274);
            this.pnl_YouTubeDownloader.TabIndex = 9;
            // 
            // Frm_DwnYouTube
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(498, 292);
            this.Controls.Add(this.pnl_YouTubeDownloader);
            this.Name = "Frm_DwnYouTube";
            this.Text = "YouTubeDownloader";
            this.pnl_YouTubeDownloader.ResumeLayout(false);
            this.pnl_YouTubeDownloader.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cmd_Download;
        private System.Windows.Forms.TextBox txt_Url;
        private System.Windows.Forms.ComboBox cb_Resolution;
        private System.Windows.Forms.Button cmd_DownloadAudio;
        private System.Windows.Forms.TextBox txt_location;
        private System.Windows.Forms.Label lbl_locationSave;
        private System.Windows.Forms.Label Lbl_youtubeUrl;
        private System.Windows.Forms.Label lbl_Resolution;
        private System.Windows.Forms.Button cmd_Exit;
        private System.Windows.Forms.Panel pnl_YouTubeDownloader;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserYoutube;
    }
}

