namespace YoutubeExtractor
{
    public class DownloadInfo
    {
        public string DownloadUrl { get; private set; }

        public int FormatCode { get; private set; }

        public DownloadInfo(string downloadUrl, int formatCode)
        {
            this.DownloadUrl = downloadUrl;
            this.FormatCode = formatCode;
        }
    }
}