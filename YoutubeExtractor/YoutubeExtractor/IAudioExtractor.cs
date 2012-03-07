namespace YoutubeExtractor
{
    public interface IAudioExtractor
    {
        void WriteChunk(byte[] chunk, uint timeStamp);

        void Finish();

        string Path { get; }
    }
}