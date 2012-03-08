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
                        return VideoFormat.Mobile;
                }

                return VideoFormat.Unkown;
            }
        }

        public bool CanExtractAudio
        {
            get { return this.VideoType == VideoType.Flash; }
        }

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

        public int FormatCode { get; private set; }

        public VideoInfo(string downloadUrl, int formatCode)
        {
            this.DownloadUrl = downloadUrl;
            this.FormatCode = formatCode;
        }
    }
}