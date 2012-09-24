using System.IO;

namespace YoutubeExtractor
{
    internal class AacAudioExtractor : IAudioExtractor
    {
        private readonly FileStream fileStream;
        private int aacProfile;
        private int channelConfig;
        private int sampleRateIndex;

        public AacAudioExtractor(string path)
        {
            this.VideoPath = path;
            fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 64 * 1024);
        }

        public string VideoPath { get; private set; }

        public void Dispose()
        {
            this.fileStream.Dispose();
        }

        public void WriteChunk(byte[] chunk, uint timeStamp)
        {
            if (chunk.Length < 1)
            {
                return;
            }

            if (chunk[0] == 0)
            {
                // Header
                if (chunk.Length < 3)
                {
                    return;
                }

                ulong bits = (ulong)BigEndianBitConverter.ToUInt16(chunk, 1) << 48;

                aacProfile = BitHelper.Read(ref bits, 5) - 1;
                sampleRateIndex = BitHelper.Read(ref bits, 4);
                channelConfig = BitHelper.Read(ref bits, 4);

                if (aacProfile < 0 || aacProfile > 3)
                    throw new AudioExtractionException("Unsupported AAC profile.");
                if (sampleRateIndex > 12)
                    throw new AudioExtractionException("Invalid AAC sample rate index.");
                if (channelConfig > 6)
                    throw new AudioExtractionException("Invalid AAC channel configuration.");
            }

            else
            {
                // Audio data
                int dataSize = chunk.Length - 1;
                ulong bits = 0;

                // Reference: WriteADTSHeader from FAAC's bitstream.c

                BitHelper.Write(ref bits, 12, 0xFFF);
                BitHelper.Write(ref bits, 1, 0);
                BitHelper.Write(ref bits, 2, 0);
                BitHelper.Write(ref bits, 1, 1);
                BitHelper.Write(ref bits, 2, aacProfile);
                BitHelper.Write(ref bits, 4, sampleRateIndex);
                BitHelper.Write(ref bits, 1, 0);
                BitHelper.Write(ref bits, 3, channelConfig);
                BitHelper.Write(ref bits, 1, 0);
                BitHelper.Write(ref bits, 1, 0);
                BitHelper.Write(ref bits, 1, 0);
                BitHelper.Write(ref bits, 1, 0);
                BitHelper.Write(ref bits, 13, 7 + dataSize);
                BitHelper.Write(ref bits, 11, 0x7FF);
                BitHelper.Write(ref bits, 2, 0);

                fileStream.Write(BigEndianBitConverter.GetBytes(bits), 1, 7);
                fileStream.Write(chunk, 1, dataSize);
            }
        }
    }
}