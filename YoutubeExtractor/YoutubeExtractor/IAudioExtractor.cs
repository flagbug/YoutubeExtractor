using System;

namespace YoutubeExtractor
{
    internal interface IAudioExtractor : IDisposable
    {
        void WriteChunk(byte[] chunk, uint timeStamp);

        string Path { get; }
    }
}