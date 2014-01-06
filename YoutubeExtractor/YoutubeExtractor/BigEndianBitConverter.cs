// ****************************************************************************
//
// FLV Extract
// Copyright (C) 2006-2012  J.D. Purcell (moitah@yahoo.com)
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// ****************************************************************************

namespace YoutubeExtractor
{
    internal static class BigEndianBitConverter
    {
        public static byte[] GetBytes(ulong value)
        {
            var buff = new byte[8];

            buff[0] = (byte)(value >> 56);
            buff[1] = (byte)(value >> 48);
            buff[2] = (byte)(value >> 40);
            buff[3] = (byte)(value >> 32);
            buff[4] = (byte)(value >> 24);
            buff[5] = (byte)(value >> 16);
            buff[6] = (byte)(value >> 8);
            buff[7] = (byte)(value);

            return buff;
        }

        public static byte[] GetBytes(uint value)
        {
            var buff = new byte[4];

            buff[0] = (byte)(value >> 24);
            buff[1] = (byte)(value >> 16);
            buff[2] = (byte)(value >> 8);
            buff[3] = (byte)(value);

            return buff;
        }

        public static byte[] GetBytes(ushort value)
        {
            var buff = new byte[2];

            buff[0] = (byte)(value >> 8);
            buff[1] = (byte)(value);

            return buff;
        }

        public static ushort ToUInt16(byte[] value, int startIndex)
        {
            return (ushort)(value[startIndex] << 8 | value[startIndex + 1]);
        }

        public static uint ToUInt32(byte[] value, int startIndex)
        {
            return
                (uint)value[startIndex] << 24 |
                (uint)value[startIndex + 1] << 16 |
                (uint)value[startIndex + 2] << 8 |
                value[startIndex + 3];
        }

        public static ulong ToUInt64(byte[] value, int startIndex)
        {
            return
                (ulong)value[startIndex] << 56 |
                (ulong)value[startIndex + 1] << 48 |
                (ulong)value[startIndex + 2] << 40 |
                (ulong)value[startIndex + 3] << 32 |
                (ulong)value[startIndex + 4] << 24 |
                (ulong)value[startIndex + 5] << 16 |
                (ulong)value[startIndex + 6] << 8 |
                value[startIndex + 7];
        }
    }
}