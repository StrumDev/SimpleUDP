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

    #region Bool

        public static Packet Bool(bool value, ushort maxSizeData = 256) =>
            new Packet(new byte[maxSizeData]).Bool(value);

        public Packet Bool(bool value)
        {
            write += Converter.SetBool(value, Data, write);
            return this;
        }

        public bool Bool()
        {
            bool value = Converter.GetBool(Data, read);
            read += sizeof(bool);

            return value;
        }

    #endregion

    #region Byte

        public static Packet Byte(byte value, ushort maxSizeData = 256) =>
            new Packet(new byte[maxSizeData]).Byte(value);

        public Packet Byte(byte value)
        {
            write += Converter.SetByte(value, Data, write);
            return this;
        }

        public byte Byte()
        {
            byte value = Converter.GetByte(Data, read);
            read += sizeof(byte);

            return value;
        }

    #endregion

    #region SByte

        public static Packet SByte(sbyte value, ushort maxSizeData = 256) =>
            new Packet(new byte[maxSizeData]).SByte(value);

        public Packet SByte(sbyte value)
        {
            write += Converter.SetSByte(value, Data, write);
            return this;
        }

        public sbyte SByte()
        {
            sbyte value = Converter.GetSByte(Data, read);
            read += sizeof(sbyte);

            return value;
        }

    #endregion

    #region Short

        public static Packet Short(short value, ushort maxSizeData = 256) =>
            new Packet(new byte[maxSizeData]).Short(value);

        public Packet Short(short value)
        {
            write += Converter.SetShort(value, Data, write);
            return this;
        }

        public short Short()
        {
            short value = Converter.GetShort(Data, read);
            read += sizeof(short);

            return value;
        }

    #endregion

    #region UShort

        public static Packet UShort(ushort value, ushort maxSizeData = 256) =>
            new Packet(new byte[maxSizeData]).UShort(value);

        public Packet UShort(ushort value)
        {
            write += Converter.SetUShort(value, Data, write);
            return this;
        }

        public ushort UShort()
        {
            ushort value = Converter.GetUShort(Data, read);
            read += sizeof(ushort);

            return value;
        }

    #endregion

    #region Int

        public static Packet Int(int value, ushort maxSizeData = 256) =>
            new Packet(new byte[maxSizeData]).Int(value);

        public Packet Int(int value)
        {
            write += Converter.SetInt(value, Data, write);
            return this;
        }

        public int Int()
        {
            int value = Converter.GetInt(Data, read);
            read += sizeof(int);

            return value;
        }

    #endregion

    #region UInt

        public static Packet UInt(uint value, ushort maxSizeData = 256) =>
            new Packet(new byte[maxSizeData]).UInt(value);

        public Packet UInt(uint value)
        {
            write += Converter.SetUInt(value, Data, write);
            return this;
        }

        public uint UInt()
        {
            uint value = Converter.GetUInt(Data, read);
            read += sizeof(uint);

            return value;
        }

    #endregion

    #region Long

        public static Packet Long(long value, ushort maxSizeData = 256) =>
            new Packet(new byte[maxSizeData]).Long(value);

        public Packet Long(long value)
        {
            write += Converter.SetLong(value, Data, write);
            return this;
        }

        public long Long()
        {
            long value = Converter.GetLong(Data, read);
            read += sizeof(long);

            return value;
        }

    #endregion

    #region ULong

        public static Packet ULong(ulong value, ushort maxSizeData = 256) =>
            new Packet(new byte[maxSizeData]).ULong(value);

        public Packet ULong(ulong value)
        {
            write += Converter.SetULong(value, Data, write);
            return this;
        }

        public ulong ULong()
        {
            ulong value = Converter.GetULong(Data, read);
            read += sizeof(ulong);

            return value;
        }

    #endregion

    #region Float

        public static Packet Float(float value, ushort maxSizeData = 256) =>
            new Packet(new byte[maxSizeData]).Float(value);

        public Packet Float(float value)
        {
            write += Converter.SetFloat(value, Data, write);
            return this;
        }

        public float Float()
        {
            float value = Converter.GetFloat(Data, read);
            read += sizeof(float);

            return value;
        }

    #endregion

    #region Double

        public static Packet Double(double value, ushort maxSizeData = 256) =>
            new Packet(new byte[maxSizeData]).Double(value);

        public Packet Double(double value)
        {
            write += Converter.SetDouble(value, Data, write);
            return this;
        }

        public double Double()
        {
            double value = Converter.GetDouble(Data, read);
            read += sizeof(double);

            return value;
        }

    #endregion

    #region String

        public static Packet String(string value, ushort maxSizeData = MaxSizeData) =>
            new Packet(new byte[maxSizeData], 5).String(value);

        public Packet String(string value)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(value);
            UShort((ushort)buffer.Length);

            Buffer.BlockCopy(buffer, 0 , Data, write, buffer.Length);
            write += buffer.Length;
            
            return this;
        }

        public string String()
        {
            if (Length > 2)
            {
                ushort length = UShort();
                string value = Encoding.UTF8.GetString(Data, read, length);
                
                read += length;
                return value;
            }
            
            return "";
        }

    #endregion
    }
}