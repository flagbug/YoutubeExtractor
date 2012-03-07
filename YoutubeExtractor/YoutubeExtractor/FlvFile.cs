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
        private OverwriteDelegate setoutput;

        public FlvFile(string path)
        {
            this.inputPath = path;
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
            uint tagType, dataSize, timeStamp, streamID, mediaInfo;
            byte[] data;

            if ((fileLength - fileOffset) < 11)
                return false;

            // Read tag header
            tagType = ReadUInt8();
            dataSize = ReadUInt24();
            timeStamp = ReadUInt24();
            timeStamp |= ReadUInt8() << 24;
            streamID = ReadUInt24();

            // Read tag data
            if (dataSize == 0)
                return true;
            if ((fileLength - fileOffset) < dataSize)
                return false;

            mediaInfo = ReadUInt8();
            dataSize -= 1;
            data = ReadBytes((int)dataSize);

            if (tagType == 0x8)
            {  // Audio
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
            { /* Video*/ }

            return true;
        }

        private IAudioExtractor GetAudioWriter(uint mediaInfo)
        {
            uint format = mediaInfo >> 4;
            uint rate = (mediaInfo >> 2) & 0x3;
            uint bits = (mediaInfo >> 1) & 0x1;
            uint chans = mediaInfo & 0x1;
            string path;

            if ((format == 2) || (format == 14))
            { // MP3
                path = outputPathBase + ".mp3";
                if (!CanWriteTo(ref path))
                    return null;

                return new Mp3AudioExtractor(path);
            }
            /*		else if ((format == 0) || (format == 3))
            { // PCM (WAVE format)
                int sampleRate = 0;
                switch (rate)
                {
                    case 0: sampleRate = 5512; break;
                    case 1: sampleRate = 11025; break;
                    case 2: sampleRate = 22050; break;
                    case 3: sampleRate = 44100; break;
                }
                path = _outputPathBase + ".wav";
                if (!CanWriteTo(ref path)) return new DummyAudioWriter();
                if (format == 0)
                {
                    _warnings.Add("PCM byte order unspecified, assuming little endian.");
                }
                return new WAVWriter(path, (bits == 1) ? 16 : 8,
                    (chans == 1) ? 2 : 1, sampleRate);
            }
    */		else if (format == 10)
            { // AAC
                path = outputPathBase + ".aac";
                if (!CanWriteTo(ref path)) return null;
                return new AacAudioExtractor(path);
            }
            /*		else if (format == 11)
        { // Speex
            path = _outputPathBase + ".spx";
            if (!CanWriteTo(ref path)) return new DummyAudioWriter();
            return new SpeexWriter(path, (int)(_fileLength & 0xFFFFFFFF));
        }
*/		else
            {
                string typeStr;

                if (format == 1)
                    typeStr = "ADPCM";
                else if ((format == 4) || (format == 5) || (format == 6))
                    typeStr = "Nellymoser";
                else
                    typeStr = "format=" + format.ToString();

                warnings.Add("Unable to extract audio (" + typeStr + " is unsupported).");

                return null;
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
            byte[] x = new byte[4];
            fileStream.Read(x, 1, 3);
            fileOffset += 3;
            return BigEndianBitConverter.ToUInt32(x, 0);
        }

        private uint ReadUInt32()
        {
            byte[] x = new byte[4];
            fileStream.Read(x, 0, 4);
            fileOffset += 4;
            return BigEndianBitConverter.ToUInt32(x, 0);
        }

        private byte[] ReadBytes(int length)
        {
            byte[] buff = new byte[length];
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