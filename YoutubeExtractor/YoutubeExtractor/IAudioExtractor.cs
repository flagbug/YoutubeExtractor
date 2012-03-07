using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YoutubeExtractor
{
    public interface IAudioExtractor
    {
        void WriteChunk(byte[] chunk, uint timeStamp);

        void Finish();

        string Path { get; }
    }
}