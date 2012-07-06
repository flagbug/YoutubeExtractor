namespace YoutubeExtractor
{
    public class VideoInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VideoInfo"/> class.
        /// </summary>
        /// <param name="downloadUrl">The download URL.</param>
        /// <param name="title">The video title.</param>
        /// <param name="formatCode">The format code.</param>
        internal VideoInfo(string downloadUrl, string title, int formatCode)
        {
            this.DownloadUrl = downloadUrl;
            this.Title = title;
            this.FormatCode = formatCode;
        }

        /// <summary>
        /// Gets the audio extension.
        /// </summary>
        public string AudioExtension
        {
            get
            {
                if (this.VideoFormat == VideoFormat.FlashAacHighQuality || this.VideoFormat == VideoFormat.FlashAacLowQuality)
                {
                    return ".aac";
                }

                if (this.VideoFormat == VideoFormat.FlashMp3HighQuality || this.VideoFormat == VideoFormat.FlashMp3LowQuality)
                {
                    return ".mp3";
                }

                return null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the audio of this video can be extracted by YoutubeExtractor.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the audio of this video can be extracted by YoutubeExtractor; otherwise, <c>false</c>.
        /// </value>
        public bool CanExtractAudio
        {
            get { return this.VideoType == VideoType.Flash; }
        }

        /// <summary>
        /// Gets the download URL.
        /// </summary>
        public string DownloadUrl { get; private set; }

        /// <summary>
        /// Gets the format code.
        /// </summary>
        public int FormatCode { get; private set; }

        /// <summary>
        /// Gets the video title.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Gets the video extension.
        /// </summary>
        public string VideoExtension
        {
            get
            {
                switch (this.VideoType)
                {
                    case VideoType.Mp4:
                        return ".mp4";

                    case VideoType.Mobile:
                        return ".3gp";

                    case VideoType.Flash:
                        return ".flv";

                    case VideoType.WebM:
                        return ".webm";
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the video format.
        /// </summary>
        public VideoFormat VideoFormat
        {
            get
            {
                switch (this.FormatCode)
                {
                    case 43:
                        return VideoFormat.WebM360;

                    case 44:
                        return VideoFormat.WebM480;

                    case 45:
                        return VideoFormat.WebM720;

                    case 46:
                        return VideoFormat.WebM1080;

                    case 38:
                        return VideoFormat.HighDefinition4K;

                    case 37:
                        return VideoFormat.HighDefinition1080;

                    case 22:
                        return VideoFormat.HighDefinition720;

                    case 82:
                        return VideoFormat.Standard360_3D;

                    case 84:
                        return VideoFormat.HighDefinition720_3D;

                    case 35:
                        return VideoFormat.FlashAacHighQuality;

                    case 34:
                        return VideoFormat.FlashAacLowQuality;

                    case 18:
                        return VideoFormat.Standard360;

                    case 6:
                        return VideoFormat.FlashMp3HighQuality;

                    case 5:
                        return VideoFormat.FlashMp3LowQuality;

                    case 13:
                    case 17:
                    case 36:
                        return VideoFormat.Mobile;
                }

                return VideoFormat.Unkown;
            }
        }

        /// <summary>
        /// Gets the type of the video.
        /// </summary>
        /// <value>
        /// The type of the video.
        /// </value>
        public VideoType VideoType
        {
            get
            {
                switch (this.FormatCode)
                {
                    case 34:
                    case 35:
                    case 5:
                    case 6:
                        return VideoType.Flash;

                    case 18:
                    case 22:
                    case 37:
                    case 38:
                    case 82:
                    case 84:
                        return VideoType.Mp4;

                    case 13:
                    case 17:
                    case 36:
                        return VideoType.Mobile;

                    case 43:
                    case 45:
                    case 46:
                        return VideoType.WebM;
                }

                return VideoType.Unknown;
            }
        }
    }
}