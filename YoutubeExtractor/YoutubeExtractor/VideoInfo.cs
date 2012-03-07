namespace YoutubeExtractor
{
    public class VideoInfo
    {
        public string DownloadUrl { get; private set; }

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
                        return VideoType.Mobile;

                    case 43:
                    case 45:
                        return VideoType.WebM;
                }

                return VideoType.Unknown;
            }
        }

        public AudioType AudioType
        {
            get
            {
                switch (this.FormatCode)
                {
                    case 35:
                        return AudioType.AacHighQuality;

                    case 34:
                        return AudioType.AacLowQuality;

                    case 6:
                        return AudioType.Mp3HighQuality;

                    case 5:
                        return AudioType.Mp3LowQuality;
                }

                return AudioType.Unkown;
            }
        }

        public bool CanExtractAudio
        {
            get { return this.VideoType == VideoType.Flash; }
        }

        public int FormatCode { get; private set; }

        public VideoInfo(string downloadUrl, int formatCode)
        {
            this.DownloadUrl = downloadUrl;
            this.FormatCode = formatCode;
        }
    }
}