using System;

namespace YoutubeExtractor
{
    /// <summary>
    /// The exception that is thrown when the video is not available for viewing.
    /// This can happen when the uploader restricts the video to specific countries.
    /// </summary>
    public class VideoNotAvailableException : Exception
    { }
}