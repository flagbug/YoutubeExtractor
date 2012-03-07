namespace YoutubeExtractor
{
    public enum VideoFormat
    {
        /// <summary>
        /// A WebM video with 720p resolution (.webm).
        /// </summary>
        WebM720,

        /// <summary>
        /// A WebM video with 480p resolution (.webm).
        /// </summary>
        WebM480,

        /// <summary>
        /// A WebM video with 360p resolution (.webm).
        /// </summary>
        WebM360,

        /// <summary>
        /// A high definition video with 4K resolution (.mp4).
        /// </summary>
        HighDefinition4K,

        /// <summary>
        /// A high definition video with 1080p resolution (.mp4).
        /// </summary>
        HighDefinition1080,

        /// <summary>
        /// A high definition video with 720p resolution (.mp4).
        /// </summary>
        HighDefinition720,

        /// <summary>
        /// A high definition video with 720p resolution and 3D (.mp4).
        /// </summary>
        HighDefinition720_3D,

        /// <summary>
        /// A standard video with 360p solution (.mp4).
        /// </summary>
        Standard360,

        /// <summary>
        /// A standard video with 360p solution and 3D (.mp4).
        /// </summary>
        Standard360_3D,

        /// <summary>
        /// A flash video with 480p resolution and 44KHz stereo AAC audio (.flv).
        /// </summary>
        FlashAacHighQuality,

        /// <summary>
        /// A flash video with 240p resolution and 22KHz stereo AAC audio (.flv).
        /// </summary>
        FlashAacLowQuality,

        /// <summary>
        /// A flash video with 360p resolution and 44KHz mono Mp3 audio (.flv).
        /// </summary>
        FlashMp3HighQuality,

        /// <summary>
        /// A flash video with 240p resolution and 44KHz mono Mp3 audio (.flv).
        /// </summary>
        FlashMp3LowQuality,

        /// <summary>
        /// A 3GP video for mobile devices (.3gp).
        /// </summary>
        Mobile,

        /// <summary>
        /// Unkown format
        /// </summary>
        Unkown
    }
}