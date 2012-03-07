namespace YoutubeExtractor
{
    internal static class LitleEndianBitConverter
    {
        public static byte[] GetBytes(ulong value)
        {
            var buffer = new byte[8];

            buffer[0] = (byte)(value);
            buffer[1] = (byte)(value >> 8);
            buffer[2] = (byte)(value >> 16);
            buffer[3] = (byte)(value >> 24);
            buffer[4] = (byte)(value >> 32);
            buffer[5] = (byte)(value >> 40);
            buffer[6] = (byte)(value >> 48);
            buffer[7] = (byte)(value >> 56);

            return buffer;
        }

        public static byte[] GetBytes(uint value)
        {
            var buffer = new byte[4];

            buffer[0] = (byte)(value);
            buffer[1] = (byte)(value >> 8);
            buffer[2] = (byte)(value >> 16);
            buffer[3] = (byte)(value >> 24);

            return buffer;
        }

        public static byte[] GetBytes(ushort value)
        {
            var buffer = new byte[2];

            buffer[0] = (byte)(value);
            buffer[1] = (byte)(value >> 8);

            return buffer;
        }
    }
}