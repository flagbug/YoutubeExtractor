using System.Collections.Generic;
using System.IO;

namespace YoutubeExtractor
{
    internal class Mp3AudioExtractor : IAudioExtractor
    {
        private readonly List<byte[]> chunkBuffer;
        private readonly FileStream fileStream;
        private readonly List<uint> frameOffsets;
        private readonly List<string> warnings;
        private int channelMode;
        private bool delayWrite;
        private int firstBitRate;
        private uint firstFrameHeader;
        private bool hasVbrHeader;
        private bool isVbr;
        private int mpegVersion;
        private int sampleRate;
        private uint totalFrameLength;
        private bool writeVbrHeader;

        public Mp3AudioExtractor(string path)
        {
            this.VideoPath = path;
            this.fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 64 * 1024);
            this.warnings = new List<string>();
            this.chunkBuffer = new List<byte[]>();
            this.frameOffsets = new List<uint>();
            this.delayWrite = true;
        }

        public string VideoPath { get; private set; }

        public IEnumerable<string> Warnings
        {
            get { return this.warnings; }
        }

        public void Dispose()
        {
            this.Flush();

            if (this.writeVbrHeader)
            {
                this.fileStream.Seek(0, SeekOrigin.Begin);
                this.WriteVbrHeader(false);
            }

            this.fileStream.Dispose();
        }

        public void WriteChunk(byte[] chunk, uint timeStamp)
        {
            this.chunkBuffer.Add(chunk);
            this.ParseMp3Frames(chunk);

            if (this.delayWrite && this.totalFrameLength >= 65536)
            {
                this.delayWrite = false;
            }

            if (!this.delayWrite)
            {
                this.Flush();
            }
        }

        private static int GetFrameDataOffset(int mpegVersion, int channelMode)
        {
            return 4 + (mpegVersion == 3 ?
                (channelMode == 3 ? 17 : 32) :
                (channelMode == 3 ? 9 : 17));
        }

        private static int GetFrameLength(int mpegVersion, int bitRate, int sampleRate, int padding)
        {
            return (mpegVersion == 3 ? 144 : 72) * bitRate / sampleRate + padding;
        }

        private void Flush()
        {
            foreach (byte[] chunk in chunkBuffer)
            {
                this.fileStream.Write(chunk, 0, chunk.Length);
            }

            this.chunkBuffer.Clear();
        }

        private void ParseMp3Frames(byte[] buffer)
        {
            var mpeg1BitRate = new[] { 0, 32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320, 0 };
            var mpeg2XBitRate = new[] { 0, 8, 16, 24, 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160, 0 };
            var mpeg1SampleRate = new[] { 44100, 48000, 32000, 0 };
            var mpeg20SampleRate = new[] { 22050, 24000, 16000, 0 };
            var mpeg25SampleRate = new[] { 11025, 12000, 8000, 0 };

            int offset = 0;
            int length = buffer.Length;

            while (length >= 4)
            {
                int mpegVersion, sampleRate, channelMode;

                ulong header = (ulong)BigEndianBitConverter.ToUInt32(buffer, offset) << 32;

                if (BitHelper.Read(ref header, 11) != 0x7FF)
                {
                    break;
                }

                mpegVersion = BitHelper.Read(ref header, 2);
                int layer = BitHelper.Read(ref header, 2);
                BitHelper.Read(ref header, 1);
                int bitRate = BitHelper.Read(ref header, 4);
                sampleRate = BitHelper.Read(ref header, 2);
                int padding = BitHelper.Read(ref header, 1);
                BitHelper.Read(ref header, 1);
                channelMode = BitHelper.Read(ref header, 2);

                if (mpegVersion == 1 || layer != 1 || bitRate == 0 || bitRate == 15 || sampleRate == 3)
                {
                    break;
                }

                bitRate = (mpegVersion == 3 ? mpeg1BitRate[bitRate] : mpeg2XBitRate[bitRate]) * 1000;

                switch (mpegVersion)
                {
                    case 2:
                        sampleRate = mpeg20SampleRate[sampleRate];
                        break;

                    case 3:
                        sampleRate = mpeg1SampleRate[sampleRate];
                        break;

                    default:
                        sampleRate = mpeg25SampleRate[sampleRate];
                        break;
                }

                int frameLenght = GetFrameLength(mpegVersion, bitRate, sampleRate, padding);

                if (frameLenght > length)
                {
                    break;
                }

                bool isVbrHeaderFrame = false;

                if (frameOffsets.Count == 0)
                {
                    // Check for an existing VBR header just to be safe (I haven't seen any in FLVs)
                    int o = offset + GetFrameDataOffset(mpegVersion, channelMode);

                    if (BigEndianBitConverter.ToUInt32(buffer, o) == 0x58696E67)
                    {
                        // "Xing"
                        isVbrHeaderFrame = true;
                        this.delayWrite = false;
                        this.hasVbrHeader = true;
                    }
                }

                if (!isVbrHeaderFrame)
                {
                    if (this.firstBitRate == 0)
                    {
                        this.firstBitRate = bitRate;
                        this.mpegVersion = mpegVersion;
                        this.sampleRate = sampleRate;
                        this.channelMode = channelMode;
                        this.firstFrameHeader = BigEndianBitConverter.ToUInt32(buffer, offset);
                    }

                    else if (!this.isVbr && bitRate != this.firstBitRate)
                    {
                        this.isVbr = true;

                        if (!this.hasVbrHeader)
                        {
                            if (this.delayWrite)
                            {
                                this.WriteVbrHeader(true);
                                this.writeVbrHeader = true;
                                this.delayWrite = false;
                            }

                            else
                            {
                                this.warnings.Add("Detected VBR too late, cannot add VBR header.");
                            }
                        }
                    }
                }

                this.frameOffsets.Add(this.totalFrameLength + (uint)offset);

                offset += frameLenght;
                length -= frameLenght;
            }

            this.totalFrameLength += (uint)buffer.Length;
        }

        private void WriteVbrHeader(bool isPlaceholder)
        {
            var buffer = new byte[GetFrameLength(this.mpegVersion, 64000, this.sampleRate, 0)];

            if (!isPlaceholder)
            {
                uint header = this.firstFrameHeader;
                int dataOffset = GetFrameDataOffset(this.mpegVersion, this.channelMode);
                header &= 0xFFFE0DFF; // Clear CRC, bitrate, and padding fields
                header |= (uint)(mpegVersion == 3 ? 5 : 8) << 12; // 64 kbit/sec
                BitHelper.CopyBytes(buffer, 0, BigEndianBitConverter.GetBytes(header));
                BitHelper.CopyBytes(buffer, dataOffset, BigEndianBitConverter.GetBytes(0x58696E67)); // "Xing"
                BitHelper.CopyBytes(buffer, dataOffset + 4, BigEndianBitConverter.GetBytes((uint)0x7)); // Flags
                BitHelper.CopyBytes(buffer, dataOffset + 8, BigEndianBitConverter.GetBytes((uint)frameOffsets.Count)); // Frame count
                BitHelper.CopyBytes(buffer, dataOffset + 12, BigEndianBitConverter.GetBytes(totalFrameLength)); // File length

                for (int i = 0; i < 100; i++)
                {
                    int frameIndex = (int)((i / 100.0) * this.frameOffsets.Count);

                    buffer[dataOffset + 16 + i] = (byte)(this.frameOffsets[frameIndex] / (double)this.totalFrameLength * 256.0);
                }
            }

            this.fileStream.Write(buffer, 0, buffer.Length);
        }
    }
}