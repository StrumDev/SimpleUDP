// This file is provided under The MIT License as part of SimpleUDP.
// Copyright (c) StrumDev
// For additional information please see the included LICENSE.md file or view it on GitHub:
// https://github.com/StrumDev/SimpleUDP/blob/main/LICENSE

using System;

namespace SimpleUDP.Utils
{
    public static class Converter
    {
    #region ToBytes

        public static byte SetByte(byte value, byte[] array, int startIndex)
        {
            array[startIndex] = value;
            return sizeof(byte);
        }

        public static byte SetSByte(sbyte value, byte[] array, int startIndex)
        {
            array[startIndex] = (byte)value;
            return sizeof(sbyte);
        }

        public static byte SetShort(short value, byte[] array, int startIndex)
        {
            array[startIndex    ] = (byte)value;
            array[startIndex + 1] = (byte)(value >> 8);
            return sizeof(short);
        }

        public static byte SetUShort(ushort value, byte[] array, int startIndex)
        {
            array[startIndex    ] = (byte)value;
            array[startIndex + 1] = (byte)(value >> 8);

            return sizeof(ushort);
        }

        public static byte SetInt(int value, byte[] array, int startIndex)
        {
            array[startIndex    ] = (byte)value;
            array[startIndex + 1] = (byte)(value >> 8);
            array[startIndex + 2] = (byte)(value >> 16);
            array[startIndex + 3] = (byte)(value >> 24);

            return sizeof(int);
        }

        public static byte SetUInt(uint value, byte[] array, int startIndex)
        {
            array[startIndex    ] = (byte)value;
            array[startIndex + 1] = (byte)(value >> 8);
            array[startIndex + 2] = (byte)(value >> 16);
            array[startIndex + 3] = (byte)(value >> 24);

            return sizeof(uint);
        }

        public static byte SetLong(long value, byte[] array, int startIndex)
        {
            array[startIndex    ] = (byte)value;
            array[startIndex + 1] = (byte)(value >> 8);
            array[startIndex + 2] = (byte)(value >> 16);
            array[startIndex + 3] = (byte)(value >> 24);
            array[startIndex + 2] = (byte)(value >> 32);
            array[startIndex + 3] = (byte)(value >> 40);
            array[startIndex + 2] = (byte)(value >> 48);
            array[startIndex + 3] = (byte)(value >> 56);

            return sizeof(long);
        }

        public static byte SetULong(ulong value, byte[] array, int startIndex)
        {
            array[startIndex    ] = (byte)value;
            array[startIndex + 1] = (byte)(value >> 8);
            array[startIndex + 2] = (byte)(value >> 16);
            array[startIndex + 3] = (byte)(value >> 24);
            array[startIndex + 2] = (byte)(value >> 32);
            array[startIndex + 3] = (byte)(value >> 40);
            array[startIndex + 2] = (byte)(value >> 48);
            array[startIndex + 3] = (byte)(value >> 56);

            return sizeof(ulong);
        }

        public static byte SetFloat(float value, byte[] array, int startIndex)
        {
            byte[] buffer = BitConverter.GetBytes(value);

            for (int i = 0; i < sizeof(float); i++)
                array[startIndex + i] = buffer[i];

            return sizeof(float);
        }

        public static byte SetDouble(double value, byte[] array, int startIndex)
        {
            byte[] buffer = BitConverter.GetBytes(value);

            for (int i = 0; i < sizeof(double); i++)
                array[startIndex + i] = buffer[i];

            return sizeof(double);
        }

    #endregion

    #region GetValue

        public static byte GetByte(byte[] array, int startIndex)
        {
            byte value = array[startIndex];
            startIndex += sizeof(byte);

            return value;
        }

        public static sbyte GetSByte(byte[] array, int startIndex)
        {
            sbyte value = (sbyte)array[startIndex];
            startIndex += sizeof(sbyte);

            return value;
        }

        public static short GetShort(byte[] array, int startIndex)
        {
            short value = (short)(array[startIndex] | (array[startIndex + 1] << 8));

            startIndex += sizeof(short);
            return value;
        }

        public static ushort GetUShort(byte[] array, int startIndex)
        {
            ushort value = (ushort)(array[startIndex] | (array[startIndex + 1] << 8));

            startIndex += sizeof(ushort);
            return value;
        }

        public static int GetInt(byte[] array, int startIndex)
        {
            int value = (int)(array[startIndex] | (array[startIndex + 1] << 8) | 
            (array[startIndex + 2] << 16) | (array[startIndex + 3] << 24));

            startIndex += sizeof(int);
            return value;
        } 

        public static uint GetUInt(byte[] array, int startIndex)
        {
            uint value = (uint)(array[startIndex] | (array[startIndex + 1] << 8) | 
            (array[startIndex + 2] << 16) | (array[startIndex + 3] << 24));
            
            startIndex += sizeof(uint);
            return value;
        }

        public static long GetLong(byte[] array, int startIndex)
        {
            long value = (long)(array[startIndex] | (array[startIndex + 1] << 8) | 
            (array[startIndex + 2] << 16) | (array[startIndex + 3] << 24) | 
            (array[startIndex + 4] << 32) | (array[startIndex + 5] << 40) | 
            (array[startIndex + 6] << 48) | (array[startIndex + 7] << 56));
            
            startIndex += sizeof(long);
            return value;
        }

        public static ulong GetULong(byte[] array, int startIndex)
        {
            ulong value = (ulong)(array[startIndex] | (array[startIndex + 1] << 8) | 
            (array[startIndex + 2] << 16) | (array[startIndex + 3] << 24) | 
            (array[startIndex + 4] << 32) | (array[startIndex + 5] << 40) | 
            (array[startIndex + 6] << 48) | (array[startIndex + 7] << 56));
            
            startIndex += sizeof(ulong);
            return value;
        }

        public static float GetFloat(byte[] array, int startIndex)
        {
            float value = BitConverter.ToSingle(array, startIndex);

            startIndex += sizeof(float);
            return value;
        }

        public static double GetDouble(byte[] array, int startIndex)
        {
            double value = BitConverter.ToDouble(array, startIndex);

            startIndex += sizeof(double);
            return value;
        }

    #endregion
    }
}
