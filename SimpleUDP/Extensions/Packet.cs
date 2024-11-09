using System;
using System.Text;
using SimpleUDP.Utils;

namespace SimpleUDP
{
    public class Packet
    {
        public int Offset => read;
        public int Length => write;
        
        public byte[] Data { get; private set; }
        
        public const ushort MaxSizeData = 1432;
        
        private int write, read;
        
        public static Packet Write(ushort maxSizeData = MaxSizeData) =>
            new Packet(new byte[maxSizeData]);

        public static Packet Read(byte[] packet) =>
            new Packet(packet, packet.Length, 0);
        
        public static Packet Read(byte[] packet, int length, int offset) =>
            new Packet(packet, length, offset);

        internal Packet(byte[] data, int length = 0, int offset = 0)
        {
            Data = data;
            read = offset;
            write = length;
        }
        
        public Packet Reset()
        {
            read = 0;
            write = 0;

            return this;
        }

        public byte[] GetArray(int offset = 0)
        {
            byte[] buffer = new byte[Length];
            Buffer.BlockCopy(Data, offset, buffer, 0, Length);
            
            return buffer;
        }

        private bool CanWrite(int size)
        {
            return write + size <= Data.Length;
        }

        private bool CanRead(int size)
        {
            return read + size <= Length;
        }

    #region Bool

        public static Packet Bool(bool value, ushort maxSizeData = 256) =>
            new Packet(new byte[maxSizeData]).Bool(value);

        public Packet Bool(bool value)
        {
            if (!CanWrite(UdpConverter.SizeBool))
                throw new IndexOutOfRangeException($"The maximum length should not exceed {Data.Length} bytes");

            write += UdpConverter.SetBool(value, Data, write);
            return this;
        }

        public bool Bool()
        {
            if (!CanRead(UdpConverter.SizeBool))
                return false;
            
            bool value = UdpConverter.GetBool(Data, read);
            read += sizeof(bool);

            return value;
        }

    #endregion

    #region Byte

        public static Packet Byte(byte value, ushort maxSizeData = 256) =>
            new Packet(new byte[maxSizeData]).Byte(value);

        public Packet Byte(byte value)
        {
            if (!CanWrite(UdpConverter.SizeByte))
                throw new IndexOutOfRangeException($"The maximum length should not exceed {Data.Length} bytes");
            
            write += UdpConverter.SetByte(value, Data, write);
            return this;
        }

        public byte Byte()
        {
            if (!CanRead(UdpConverter.SizeByte))
                return 0;

            byte value = UdpConverter.GetByte(Data, read);
            read += sizeof(byte);

            return value;
        }

    #endregion

    #region SByte

        public static Packet SByte(sbyte value, ushort maxSizeData = 256) =>
            new Packet(new byte[maxSizeData]).SByte(value);

        public Packet SByte(sbyte value)
        {
            if (!CanWrite(UdpConverter.SizeSByte))
                throw new IndexOutOfRangeException($"The maximum length should not exceed {Data.Length} bytes");
            
            write += UdpConverter.SetSByte(value, Data, write);
            return this;
        }

        public sbyte SByte()
        {
            if (!CanRead(UdpConverter.SizeSByte))
                return 0;
            
            sbyte value = UdpConverter.GetSByte(Data, read);
            read += sizeof(sbyte);

            return value;
        }

    #endregion

    #region Short

        public static Packet Short(short value, ushort maxSizeData = 256) =>
            new Packet(new byte[maxSizeData]).Short(value);

        public Packet Short(short value)
        {
            if (!CanWrite(UdpConverter.SizeShort))
                throw new IndexOutOfRangeException($"The maximum length should not exceed {Data.Length} bytes");
            
            write += UdpConverter.SetShort(value, Data, write);
            return this;
        }

        public short Short()
        {
            if (!CanRead(UdpConverter.SizeShort))
                return 0;
            
            short value = UdpConverter.GetShort(Data, read);
            read += sizeof(short);

            return value;
        }

    #endregion

    #region UShort

        public static Packet UShort(ushort value, ushort maxSizeData = 256) =>
            new Packet(new byte[maxSizeData]).UShort(value);

        public Packet UShort(ushort value)
        {
            if (!CanWrite(UdpConverter.SizeUShort))
                throw new IndexOutOfRangeException($"The maximum length should not exceed {Data.Length} bytes");
            
            write += UdpConverter.SetUShort(value, Data, write);
            return this;
        }

        public ushort UShort()
        {
            if (!CanRead(UdpConverter.SizeUShort))
                return 0;
            
            ushort value = UdpConverter.GetUShort(Data, read);
            read += sizeof(ushort);

            return value;
        }

    #endregion

    #region Int

        public static Packet Int(int value, ushort maxSizeData = 256) =>
            new Packet(new byte[maxSizeData]).Int(value);

        public Packet Int(int value)
        {
            if (!CanWrite(UdpConverter.SizeInt))
                throw new IndexOutOfRangeException($"The maximum length should not exceed {Data.Length} bytes");
            
            write += UdpConverter.SetInt(value, Data, write);
            return this;
        }

        public int Int()
        {
            if (!CanRead(UdpConverter.SizeInt))
                return 0;

            int value = UdpConverter.GetInt(Data, read);
            read += sizeof(int);

            return value;
        }

    #endregion

    #region UInt

        public static Packet UInt(uint value, ushort maxSizeData = 256) =>
            new Packet(new byte[maxSizeData]).UInt(value);

        public Packet UInt(uint value)
        {
            if (!CanWrite(UdpConverter.SizeUInt))
                throw new IndexOutOfRangeException($"The maximum length should not exceed {Data.Length} bytes");
            
            write += UdpConverter.SetUInt(value, Data, write);
            return this;
        }

        public uint UInt()
        {
            if (!CanRead(UdpConverter.SizeUInt))
                return 0;
            
            uint value = UdpConverter.GetUInt(Data, read);
            read += sizeof(uint);

            return value;
        }

    #endregion

    #region Long

        public static Packet Long(long value, ushort maxSizeData = 256) =>
            new Packet(new byte[maxSizeData]).Long(value);

        public Packet Long(long value)
        {
            if (!CanWrite(UdpConverter.SizeLong))
                throw new IndexOutOfRangeException($"The maximum length should not exceed {Data.Length} bytes");
            
            write += UdpConverter.SetLong(value, Data, write);
            return this;
        }

        public long Long()
        {
            if (!CanRead(UdpConverter.SizeLong))
                return 0;

            long value = UdpConverter.GetLong(Data, read);
            read += sizeof(long);

            return value;
        }

    #endregion

    #region ULong

        public static Packet ULong(ulong value, ushort maxSizeData = 256) =>
            new Packet(new byte[maxSizeData]).ULong(value);

        public Packet ULong(ulong value)
        {
            if (!CanWrite(UdpConverter.SizeULong))
                throw new IndexOutOfRangeException($"The maximum length should not exceed {Data.Length} bytes");
            
            write += UdpConverter.SetULong(value, Data, write);
            return this;
        }

        public ulong ULong()
        {
            if (!CanRead(UdpConverter.SizeULong))
                return 0;
            
            ulong value = UdpConverter.GetULong(Data, read);
            read += sizeof(ulong);

            return value;
        }

    #endregion

    #region Float

        public static Packet Float(float value, ushort maxSizeData = 256) =>
            new Packet(new byte[maxSizeData]).Float(value);

        public Packet Float(float value)
        {
            if (!CanWrite(UdpConverter.SizeFloat))
                throw new IndexOutOfRangeException($"The maximum length should not exceed {Data.Length} bytes");
            
            write += UdpConverter.SetFloat(value, Data, write);
            return this;
        }

        public float Float()
        {
            if (!CanRead(UdpConverter.SizeFloat))
                return 0;
            
            float value = UdpConverter.GetFloat(Data, read);
            read += sizeof(float);

            return value;
        }

    #endregion

    #region Double

        public static Packet Double(double value, ushort maxSizeData = 256) =>
            new Packet(new byte[maxSizeData]).Double(value);

        public Packet Double(double value)
        {
            if (!CanWrite(UdpConverter.SizeDouble))
                throw new IndexOutOfRangeException($"The maximum length should not exceed {Data.Length} bytes");
            
            write += UdpConverter.SetDouble(value, Data, write);
            return this;
        }

        public double Double()
        {
            if (!CanRead(UdpConverter.SizeDouble))
                return 0;
            
            double value = UdpConverter.GetDouble(Data, read);
            read += sizeof(double);

            return value;
        }

    #endregion

    #region Char
        
        public static Packet Char(char value, ushort maxSizeData = 512) =>
            new Packet(new byte[maxSizeData]).Char(value);

        public Packet Char(char value)
        {
            if (!CanWrite(UdpConverter.SizeChar))
                throw new IndexOutOfRangeException($"The maximum length should not exceed {Data.Length} bytes");
            
            byte[] buffer = BitConverter.GetBytes(value);
           
            Data[write] = buffer[0];
            Data[write + 1] = buffer[1];

            write += 2;
            return this;
        }

        public char Char()
        {
            if (!CanRead(UdpConverter.SizeChar))
                return ' ';
            
            char value = BitConverter.ToChar(Data, read);
            
            read += 2;
            return value;
        }

    #endregion

    #region String

        public static Packet String(string value, ushort maxSizeData = MaxSizeData) =>
            new Packet(new byte[maxSizeData]).String(value);

        public Packet String(string value)
        {
            if (!CanWrite(value.Length * UdpConverter.SizeChar))
                throw new IndexOutOfRangeException($"The maximum length should not exceed {Data.Length} bytes");
            
            byte[] buffer = Encoding.UTF8.GetBytes(value);

            UShort((ushort)buffer.Length);
            Buffer.BlockCopy(buffer, 0 , Data, write, buffer.Length);
            
            write += buffer.Length;
            return this;
        }

        public string String()
        {
            ushort count = UShort();

            if (count == 0 || !CanRead(count))
                return "";
            
            string value = Encoding.UTF8.GetString(Data, read, count);
            
            read += count;
            return value;
        }

    #endregion
    }
}