namespace YoutubeExtractor
{
    public class VideoInfo
    {
        private int formatCode;

        public string DownloadUrl { get; private set; }

        public YouTubeVideoType VideoType { get; private set; }

        public int FormatCode
        {
            get { return this.formatCode; }
            set
            {
                this.formatCode = value;

                switch (value)
                {
                    case 34:
                    case 35:
                    case 5:
                    case 6:
                        this.VideoType = YouTubeVideoType.Flash;
                        break;

                    case 18:
                    case 22:
                    case 37:
                    case 38:
                    case 82:
                    case 84:
                        this.VideoType = YouTubeVideoType.Mp4;
                        break;

                    case 13:
                    case 17:
                        this.VideoType = YouTubeVideoType.Mobile;
                        break;

                    case 43:
                    case 45:
                        this.VideoType = YouTubeVideoType.WebM;
                        break;

                    default:
                        this.VideoType = YouTubeVideoType.Unknown;
                        break;
                }
            }
        }

        public VideoInfo(string downloadUrl, int formatCode)
        {
            this.DownloadUrl = downloadUrl;
            this.FormatCode = formatCode;
        }
    }
}