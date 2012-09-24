using System;

namespace YoutubeExtractor
{
    internal interface IAudioExtractor : IDisposable
    {
        string VideoPath { get; }

        /// <exception cref="AudioExtractionException">An error occured while writing the chunk.</exception>
        void WriteChunk(byte[] chunk, uint timeStamp);
    }
}