using System;

namespace YoutubeExtractor
{
    /// <summary>
    /// The exception that is thrown when an error occurs durin audio extraction.
    /// </summary>
    public class AudioExtractionException : Exception
    {
        public AudioExtractionException(string message)
            : base(message)
        { }
    }
}