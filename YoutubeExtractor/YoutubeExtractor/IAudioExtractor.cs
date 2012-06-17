using System;

namespace YoutubeExtractor
{
    internal interface IAudioExtractor : IDisposable
    {
        string VideoPath { get; }

        void WriteChunk(byte[] chunk, uint timeStamp);
    }
}