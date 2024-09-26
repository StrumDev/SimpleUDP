using System;

namespace SimpleUDP.Utils
{ 
    public static class UdpConverter
    {
        public const byte SizeBool = 1;
        public const byte SizeChar = 2;

        public const byte SizeByte = 1;
        public const byte SizeSByte = 1;

        public const byte SizeShort = 2;
        public const byte SizeUShort = 2;

        public const byte SizeInt = 4;
        public const byte SizeUInt = 4;

        public const byte SizeLong = 8;
        public const byte SizeULong = 8;

        public const byte SizeFloat = 4;
        public const byte SizeDouble = 8;        

        public static bool IsLittleEndian { get; private set; }

        static UdpConverter()
        {
            IsLittleEndian = BitConverter.IsLittleEndian;
        }
    
    #region ToBytes
        
        public static byte SetBool(bool value, byte[] array, int startIndex)
        {
            array[startIndex] = (byte)(value ? 1 : 0);

            return SizeByte;
        }

        public static byte SetByte(byte value, byte[] array, int startIndex)
        {
            array[startIndex] = value;

            return SizeByte;
        }

        public static byte SetSByte(sbyte value, byte[] array, int startIndex)
        {
            array[startIndex] = (byte)value;

            return SizeByte;
        }

        public static byte SetShort(short value, byte[] array, int startIndex)
        {
            if (IsLittleEndian)
            {
                array[startIndex    ] = (byte)value;
                array[startIndex + 1] = (byte)(value >> 8); 
            }
            else
            {
                array[startIndex + 1] = (byte)value;
                array[startIndex    ] = (byte)(value >> 8); 
            }

            return SizeShort;
        }

        public static byte SetUShort(ushort value, byte[] array, int startIndex)
        {
            if (IsLittleEndian)
            {
                array[startIndex    ] = (byte)value;
                array[startIndex + 1] = (byte)(value >> 8); 
            }
            else
            {
                array[startIndex + 1] = (byte)value;
                array[startIndex    ] = (byte)(value >> 8); 
            }

            return SizeShort;
        }

        public static byte SetInt(int value, byte[] array, int startIndex)
        {
            if (IsLittleEndian)
            {
                array[startIndex    ] = (byte)value;
                array[startIndex + 1] = (byte)(value >> 8);
                array[startIndex + 2] = (byte)(value >> 16);
                array[startIndex + 3] = (byte)(value >> 24); 
            }
            else
            {
                array[startIndex + 3] = (byte)value;
                array[startIndex + 2] = (byte)(value >> 8);
                array[startIndex + 1] = (byte)(value >> 16);
                array[startIndex    ] = (byte)(value >> 24); 
            }

            return SizeInt;
        }

        public static byte SetUInt(uint value, byte[] array, int startIndex)
        {
            if (IsLittleEndian)
            {
                array[startIndex    ] = (byte)value;
                array[startIndex + 1] = (byte)(value >> 8);
                array[startIndex + 2] = (byte)(value >> 16);
                array[startIndex + 3] = (byte)(value >> 24); 
            }
            else
            {
                array[startIndex + 3] = (byte)value;
                array[startIndex + 2] = (byte)(value >> 8);
                array[startIndex + 1] = (byte)(value >> 16);
                array[startIndex    ] = (byte)(value >> 24); 
            }

            return SizeInt;
        }

        public static byte SetLong(long value, byte[] array, int startIndex)
        {
            if (IsLittleEndian)
            {
                array[startIndex    ] = (byte)value;
                array[startIndex + 1] = (byte)(value >> 8);
                array[startIndex + 2] = (byte)(value >> 16);
                array[startIndex + 3] = (byte)(value >> 24);
                array[startIndex + 4] = (byte)(value >> 32);
                array[startIndex + 5] = (byte)(value >> 40);
                array[startIndex + 6] = (byte)(value >> 48);
                array[startIndex + 7] = (byte)(value >> 56);
            }
            else
            {
                array[startIndex + 7] = (byte)value;
                array[startIndex + 6] = (byte)(value >> 8);
                array[startIndex + 5] = (byte)(value >> 16);
                array[startIndex + 4] = (byte)(value >> 24);
                array[startIndex + 3] = (byte)(value >> 32);
                array[startIndex + 2] = (byte)(value >> 40);
                array[startIndex + 1] = (byte)(value >> 48);
                array[startIndex    ] = (byte)(value >> 56);    
            }

            return SizeLong;
        }

        public static byte SetULong(ulong value, byte[] array, int startIndex)
        {
            if (IsLittleEndian)
            {
                array[startIndex    ] = (byte)value;
                array[startIndex + 1] = (byte)(value >> 8);
                array[startIndex + 2] = (byte)(value >> 16);
                array[startIndex + 3] = (byte)(value >> 24);
                array[startIndex + 4] = (byte)(value >> 32);
                array[startIndex + 5] = (byte)(value >> 40);
                array[startIndex + 6] = (byte)(value >> 48);
                array[startIndex + 7] = (byte)(value >> 56);
            }
            else
            {
                array[startIndex + 7] = (byte)value;
                array[startIndex + 6] = (byte)(value >> 8);
                array[startIndex + 5] = (byte)(value >> 16);
                array[startIndex + 4] = (byte)(value >> 24);
                array[startIndex + 3] = (byte)(value >> 32);
                array[startIndex + 2] = (byte)(value >> 40);
                array[startIndex + 1] = (byte)(value >> 48);
                array[startIndex    ] = (byte)(value >> 56);    
            }

            return SizeLong;
        }

        public static byte SetFloat(float value, byte[] array, int startIndex)
        {
            int intValue = BitConverter.SingleToInt32Bits(value);

            if (IsLittleEndian)
            {
                array[startIndex    ] = (byte)(intValue & 0xFF);
                array[startIndex + 1] = (byte)((intValue >> 8) & 0xFF);
                array[startIndex + 2] = (byte)((intValue >> 16) & 0xFF);
                array[startIndex + 3] = (byte)((intValue >> 24) & 0xFF);
            }
            else
            {
                array[startIndex + 3] = (byte)(intValue & 0xFF);
                array[startIndex + 2] = (byte)((intValue >> 8) & 0xFF);
                array[startIndex + 1] = (byte)((intValue >> 16) & 0xFF);
                array[startIndex    ] = (byte)((intValue >> 24) & 0xFF);
            }

            return SizeFloat;
        }

        public static byte SetDouble(double value, byte[] array, int startIndex)
        {
            long longValue = BitConverter.DoubleToInt64Bits(value);

            if (IsLittleEndian)
            {
                array[startIndex    ] = (byte)(longValue & 0xFF);
                array[startIndex + 1] = (byte)((longValue >> 8) & 0xFF);
                array[startIndex + 2] = (byte)((longValue >> 16) & 0xFF);
                array[startIndex + 3] = (byte)((longValue >> 24) & 0xFF);
                array[startIndex + 4] = (byte)((longValue >> 32) & 0xFF);
                array[startIndex + 5] = (byte)((longValue >> 40) & 0xFF);
                array[startIndex + 6] = (byte)((longValue >> 48) & 0xFF);
                array[startIndex + 7] = (byte)((longValue >> 56) & 0xFF);
            }
            else
            {
                array[startIndex + 7] = (byte)(longValue & 0xFF);
                array[startIndex + 6] = (byte)((longValue >> 8) & 0xFF);
                array[startIndex + 5] = (byte)((longValue >> 16) & 0xFF);
                array[startIndex + 4] = (byte)((longValue >> 24) & 0xFF);
                array[startIndex + 3] = (byte)((longValue >> 32) & 0xFF);
                array[startIndex + 2] = (byte)((longValue >> 40) & 0xFF);
                array[startIndex + 1] = (byte)((longValue >> 48) & 0xFF);
                array[startIndex    ] = (byte)((longValue >> 56) & 0xFF);
            }

            return SizeDouble;
        }

    #endregion

    #region GetValue

        public static bool GetBool(byte[] array, int startIndex)
        {
            return array[startIndex] == 1;
        }

        public static byte GetByte(byte[] array, int startIndex)
        {
            return array[startIndex];
        }

        public static sbyte GetSByte(byte[] array, int startIndex)
        {
            return (sbyte)array[startIndex];
        }

        public static short GetShort(byte[] array, int startIndex)
        {
            if (IsLittleEndian)
            {
                return (short)(array[startIndex] | 
                ((short)array[startIndex + 1] << 8));   
            }
            else
            {
                return (short)(array[startIndex + 1] | 
                ((short)array[startIndex] << 8)); 
            }
        }

        public static ushort GetUShort(byte[] array, int startIndex)
        {
            if (IsLittleEndian)
            {
                return (ushort)(array[startIndex] | 
                ((ushort)array[startIndex + 1] << 8));   
            }
            else
            {
                return (ushort)(array[startIndex + 1] | 
                ((ushort)array[startIndex] << 8)); 
            }
        }

        public static int GetInt(byte[] array, int startIndex)
        {
            if (IsLittleEndian)
            {
                return (int)array[startIndex] |
                ((int)array[startIndex + 1] << 8 ) |
                ((int)array[startIndex + 2] << 16) |
                ((int)array[startIndex + 3] << 24);
            }
            else
            {
                return (int)array[startIndex + 3] |
                ((int)array[startIndex + 2] << 8 ) |
                ((int)array[startIndex + 1] << 16) |
                ((int)array[startIndex    ] << 24);
            }
        } 

        public static uint GetUInt(byte[] array, int startIndex)
        {
            if (IsLittleEndian)
            {
                return (uint)array[startIndex] |
                ((uint)array[startIndex + 1] << 8 ) |
                ((uint)array[startIndex + 2] << 16) |
                ((uint)array[startIndex + 3] << 24);
            }
            else
            {
                return (uint)array[startIndex + 3] |
                ((uint)array[startIndex + 2] << 8 ) |
                ((uint)array[startIndex + 1] << 16) |
                ((uint)array[startIndex    ] << 24);
            }
        }

        public static long GetLong(byte[] array, int startIndex)
        {
            if (IsLittleEndian)
            {
                return (long)array[startIndex] |
                ((long)array[startIndex + 1] << 8 ) |
                ((long)array[startIndex + 2] << 16) |
                ((long)array[startIndex + 3] << 24) |
                ((long)array[startIndex + 4] << 32) |
                ((long)array[startIndex + 5] << 40) |
                ((long)array[startIndex + 6] << 48) |
                ((long)array[startIndex + 7] << 56);
            }
            else
            {
                return (long)array[startIndex + 7]  |
                ((long)array[startIndex + 6] << 8 ) |
                ((long)array[startIndex + 5] << 16) |
                ((long)array[startIndex + 4] << 24) |
                ((long)array[startIndex + 3] << 32) |
                ((long)array[startIndex + 2] << 40) |
                ((long)array[startIndex + 1] << 48) |
                ((long)array[startIndex    ] << 56);
            }
        }

        public static ulong GetULong(byte[] array, int startIndex)
        {
            if (IsLittleEndian)
            {
                return (ulong)array[startIndex] |
                ((ulong)array[startIndex + 1] << 8 ) |
                ((ulong)array[startIndex + 2] << 16) |
                ((ulong)array[startIndex + 3] << 24) |
                ((ulong)array[startIndex + 4] << 32) |
                ((ulong)array[startIndex + 5] << 40) |
                ((ulong)array[startIndex + 6] << 48) |
                ((ulong)array[startIndex + 7] << 56);
            }
            else
            {
                return (ulong)array[startIndex + 7]  |
                ((ulong)array[startIndex + 6] << 8 ) |
                ((ulong)array[startIndex + 5] << 16) |
                ((ulong)array[startIndex + 4] << 24) |
                ((ulong)array[startIndex + 3] << 32) |
                ((ulong)array[startIndex + 2] << 40) |
                ((ulong)array[startIndex + 1] << 48) |
                ((ulong)array[startIndex    ] << 56);
            }
        }

        public static float GetFloat(byte[] array, int startIndex)
        {
            int intValue;

            if (IsLittleEndian)
            {
                intValue = (int)array[startIndex] |
                ((int)array[startIndex + 1] << 8 ) |
                ((int)array[startIndex + 2] << 16) |
                ((int)array[startIndex + 3] << 24);
            }
            else
            {
                intValue = (int)array[startIndex + 3] |
                ((int)array[startIndex + 2] << 8 ) |
                ((int)array[startIndex + 1] << 16) |
                ((int)array[startIndex    ] << 24);
            }
            
            return BitConverter.Int32BitsToSingle(intValue);
        }

        public static double GetDouble(byte[] array, int startIndex)
        {
            long longValue;
            
            if (IsLittleEndian)
            {
                longValue = (long)array[startIndex] |
                ((long)array[startIndex + 1] << 8 ) |
                ((long)array[startIndex + 2] << 16) |
                ((long)array[startIndex + 3] << 24) |
                ((long)array[startIndex + 4] << 32) |
                ((long)array[startIndex + 5] << 40) |
                ((long)array[startIndex + 6] << 48) |
                ((long)array[startIndex + 7] << 56);
            }
            else
            {
                longValue = (long)array[startIndex + 7]  |
                ((long)array[startIndex + 6] << 8 ) |
                ((long)array[startIndex + 5] << 16) |
                ((long)array[startIndex + 4] << 24) |
                ((long)array[startIndex + 3] << 32) |
                ((long)array[startIndex + 2] << 40) |
                ((long)array[startIndex + 1] << 48) |
                ((long)array[startIndex    ] << 56);
            }

            return BitConverter.Int64BitsToDouble(longValue);
        }

    #endregion
    }
}