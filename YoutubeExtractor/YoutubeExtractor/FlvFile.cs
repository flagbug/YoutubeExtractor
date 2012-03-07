using System;
using System.Collections.Generic;
using System.IO;

namespace YoutubeExtractor
{
    public delegate bool OverwriteDelegate(ref string destPath);

    public class FlvFile : IDisposable
    {
        private readonly string inputPath;
        private string outputPathBase;
        private FileStream fileStream;
        private long fileOffset;
        private readonly long fileLength;
        IAudioExtractor audioWriter;
        private readonly List<string> warnings;
        private readonly OverwriteDelegate setoutput;

        public FlvFile(string path, OverwriteDelegate setoutput)
        {
            this.inputPath = path;
            this.setoutput = setoutput;
            this.OutputDirectory = Path.GetDirectoryName(path);
            this.warnings = new List<string>();
            this.fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 65536);
            this.fileOffset = 0;
            this.fileLength = fileStream.Length;
        }

        public string OutputDirectory { get; set; }

        public IEnumerable<string> Warnings
        {
            get { return warnings; }
        }

        public bool ExtractedAudio { get; private set; }

        public void ExtractStreams()
        {
            this.outputPathBase = Path.Combine(OutputDirectory, Path.GetFileNameWithoutExtension(inputPath));
            //this.setoutput = outputDeleg;

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
                // Audio
                if (audioWriter == null)
                {
                    audioWriter = GetAudioWriter(mediaInfo);
                    ExtractedAudio = audioWriter != null;
                }

                if (audioWriter == null)
                    throw new InvalidOperationException();

                audioWriter.WriteChunk(data, timeStamp);
            }

            else if ((tagType == 0x9) && ((mediaInfo >> 4) != 5))
            {
                // Video
            }

            return true;
        }

        private IAudioExtractor GetAudioWriter(uint mediaInfo)
        {
            uint format = mediaInfo >> 4;
            string path;

            switch (format)
            {
                case 14:
                case 2:
                    path = outputPathBase + ".mp3";
                    if (!CanWriteTo(ref path))
                        return null;

                    return new Mp3AudioExtractor(path);

                case 10:
                    path = outputPathBase + ".aac";
                    if (!CanWriteTo(ref path))
                        return null;

                    return new AacAudioExtractor(path);

                default:
                    {
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

                        warnings.Add("Unable to extract audio (" + typeStr + " is unsupported).");

                        return null;
                    }
            }
        }

        private bool CanWriteTo(ref string path)
        {
            return setoutput(ref path);
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
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                    fileStream = null;
                }
                CloseOutput(true);
            }
        }
    }
}