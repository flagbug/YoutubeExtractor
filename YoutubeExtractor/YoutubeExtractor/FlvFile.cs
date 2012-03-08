using System;
using System.IO;

namespace YoutubeExtractor
{
    internal class FlvFile : IDisposable
    {
        private readonly string inputPath;
        private FileStream fileStream;
        private long fileOffset;
        private readonly long fileLength;
        IAudioExtractor audioWriter;
        private readonly string outputPath;

        public event EventHandler<ProgressEventArgs> ConversionProgressChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlvFile"/> class.
        /// </summary>
        /// <param name="inputPath">The path of the input.</param>
        /// <param name="outputPath">The path of the output without extension.</param>
        public FlvFile(string inputPath, string outputPath)
        {
            this.inputPath = inputPath;
            this.outputPath = outputPath;
            this.fileStream = new FileStream(this.inputPath, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024);
            this.fileOffset = 0;
            this.fileLength = fileStream.Length;
        }

        public bool ExtractedAudio { get; private set; }

        public void ExtractStreams()
        {
            this.Seek(0);

            if (this.ReadUInt32() != 0x464C5601)
            {
                // not a FLV file
                throw new InvalidOperationException("Invalid input file. Impossible to extract audio track.");
            }

            this.ReadUInt8();
            uint dataOffset = this.ReadUInt32();

            this.Seek(dataOffset);

            this.ReadUInt32();

            // Store the progress is a temporary variable, so that we can check if it changed
            int tempProgress = 0;

            while (fileOffset < fileLength)
            {
                if (!ReadTag())
                {
                    break;
                }

                if ((fileLength - fileOffset) < 4)
                {
                    break;
                }

                this.ReadUInt32();

                int progress = (int)((this.fileOffset * 1.0 / this.fileLength) * 100);

                if (this.ConversionProgressChanged != null && progress != tempProgress)
                {
                    tempProgress = progress;
                    this.ConversionProgressChanged(this, new ProgressEventArgs(progress));
                }
            }

            this.CloseOutput(false);
        }

        private void CloseOutput(bool disposing)
        {
            if (audioWriter != null)
            {
                audioWriter.Finish();

                if (disposing && (audioWriter.Path != null))
                {
                    try
                    {
                        File.Delete(audioWriter.Path);
                    }
                    catch { }
                }

                audioWriter = null;
            }
        }

        private bool ReadTag()
        {
            if ((fileLength - fileOffset) < 11)
                return false;

            // Read tag header
            uint tagType = ReadUInt8();
            uint dataSize = ReadUInt24();
            uint timeStamp = ReadUInt24();
            timeStamp |= ReadUInt8() << 24;
            ReadUInt24();

            // Read tag data
            if (dataSize == 0)
                return true;

            if ((fileLength - fileOffset) < dataSize)
                return false;

            uint mediaInfo = ReadUInt8();
            dataSize -= 1;
            byte[] data = ReadBytes((int)dataSize);

            if (tagType == 0x8)
            {
                // If we have no audio writer, create one
                if (audioWriter == null)
                {
                    audioWriter = GetAudioWriter(mediaInfo);
                    ExtractedAudio = audioWriter != null;
                }

                if (audioWriter == null)
                    throw new InvalidOperationException();

                audioWriter.WriteChunk(data, timeStamp);
            }

            return true;
        }

        private IAudioExtractor GetAudioWriter(uint mediaInfo)
        {
            uint format = mediaInfo >> 4;
            string path = this.outputPath;

            switch (format)
            {
                case 14:
                case 2:
                    path += ".mp3";
                    return new Mp3AudioExtractor(path);

                case 10:
                    path += ".aac";
                    return new AacAudioExtractor(path);
            }

            string typeStr;

            switch (format)
            {
                case 1:
                    typeStr = "ADPCM";
                    break;

                case 6:
                case 5:
                case 4:
                    typeStr = "Nellymoser";
                    break;

                default:
                    typeStr = "format=" + format;
                    break;
            }

            throw new InvalidOperationException("Unable to extract audio (" + typeStr + " is unsupported).");
        }

        private void Seek(long offset)
        {
            fileStream.Seek(offset, SeekOrigin.Begin);
            fileOffset = offset;
        }

        private uint ReadUInt8()
        {
            fileOffset += 1;
            return (uint)fileStream.ReadByte();
        }

        private uint ReadUInt24()
        {
            var x = new byte[4];
            fileStream.Read(x, 1, 3);
            fileOffset += 3;
            return BigEndianBitConverter.ToUInt32(x, 0);
        }

        private uint ReadUInt32()
        {
            var x = new byte[4];
            fileStream.Read(x, 0, 4);
            fileOffset += 4;
            return BigEndianBitConverter.ToUInt32(x, 0);
        }

        private byte[] ReadBytes(int length)
        {
            var buff = new byte[length];
            fileStream.Read(buff, 0, length);
            fileOffset += length;
            return buff;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.fileStream != null)
                {
                    this.fileStream.Close();
                    this.fileStream = null;
                }

                this.CloseOutput(true);
            }
        }
    }
}