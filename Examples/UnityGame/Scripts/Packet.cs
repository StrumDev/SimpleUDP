using System;
using System.Text;

public class Packet
{
    public byte[] Data;
    public int Length => writePos;
    public int Offset => readPos;

    public Header Header;

    private const ushort MaxSizeData = 1280;
    private int writePos;
    private int readPos;

    public Packet()
    {
        Data = new byte[MaxSizeData];
    }

    public Packet(Header header)
    {
        Data = new byte[MaxSizeData];

        AddByte((byte)header);
    }

    public Packet(byte[] data, int length, bool readHeader = false)
    {
        Data = data;
        writePos = length;

        if (readHeader) Header = (Header)GetByte();
    }

    public byte[] ToData()
    {
        byte[] copyData = new byte[writePos];
        Array.Copy(Data, copyData, writePos);
        return copyData;
    }

#region Writes

    public Packet AddByte(byte value)
    {
        Data[writePos] = value;

        writePos += sizeof(byte);
        return this;
    }

    public Packet AddSByte(sbyte value)
    {
        Data[writePos] = (byte)value;

        writePos += sizeof(sbyte);
        return this;
    }

    public Packet AddShort(short value)
    {
        Data[writePos    ] = (byte)value;
        Data[writePos + 1] = (byte)(value >> 8);

        writePos += sizeof(short);
        return this;
    }

    public Packet AddUShort(ushort value)
    {
        Data[writePos    ] = (byte)value;
        Data[writePos + 1] = (byte)(value >> 8);

        writePos += sizeof(ushort);
        return this;
    }

    public Packet AddInt(int value)
    {
        Data[writePos    ] = (byte)value;
        Data[writePos + 1] = (byte)(value >> 8);
        Data[writePos + 2] = (byte)(value >> 16);
        Data[writePos + 3] = (byte)(value >> 24);

        writePos += sizeof(int);
        return this;
    }

    public Packet AddUInt(uint value)
    {
        Data[writePos    ] = (byte)value;
        Data[writePos + 1] = (byte)(value >> 8);
        Data[writePos + 2] = (byte)(value >> 16);
        Data[writePos + 3] = (byte)(value >> 24);

        writePos += sizeof(uint);
        return this;
    }

    public Packet AddLong(long value)
    {
        Data[writePos    ] = (byte)value;
        Data[writePos + 1] = (byte)(value >> 8);
        Data[writePos + 2] = (byte)(value >> 16);
        Data[writePos + 3] = (byte)(value >> 24);
        Data[writePos + 2] = (byte)(value >> 32);
        Data[writePos + 3] = (byte)(value >> 40);
        Data[writePos + 2] = (byte)(value >> 48);
        Data[writePos + 3] = (byte)(value >> 56);

        writePos += sizeof(long);
        return this;
    }

    public Packet AddULong(ulong value)
    {
        Data[writePos    ] = (byte)value;
        Data[writePos + 1] = (byte)(value >> 8);
        Data[writePos + 2] = (byte)(value >> 16);
        Data[writePos + 3] = (byte)(value >> 24);
        Data[writePos + 2] = (byte)(value >> 32);
        Data[writePos + 3] = (byte)(value >> 40);
        Data[writePos + 2] = (byte)(value >> 48);
        Data[writePos + 3] = (byte)(value >> 56);

        writePos += sizeof(ulong);
        return this;
    }

    public Packet AddFloat(float value)
    {
        byte[] array = BitConverter.GetBytes(value);

        Data[writePos    ] = array[0];
        Data[writePos + 1] = array[1];
        Data[writePos + 2] = array[2];
        Data[writePos + 3] = array[3];

        writePos += sizeof(float);
        return this;
    }

    public Packet AddDouble(double value)
    {
        byte[] array = BitConverter.GetBytes(value);

        Data[writePos    ] = array[0];
        Data[writePos + 1] = array[1];
        Data[writePos + 2] = array[2];
        Data[writePos + 3] = array[3];
        Data[writePos + 4] = array[4];
        Data[writePos + 5] = array[5];
        Data[writePos + 6] = array[6];
        Data[writePos + 7] = array[7];

        writePos += sizeof(double);
        return this;
    }

    public Packet AddString(string value)
    {
        byte[] array = Encoding.UTF8.GetBytes(value);
            AddUShort((ushort)array.Length);

        Array.Copy(array, 0 , Data, writePos, array.Length);
        writePos += array.Length;
        return this;
    }

#endregion

#region Reads

    public byte GetByte()
    {
        byte value = Data[readPos];
        readPos += sizeof(byte);

        return value;
    }

    public sbyte GetSByte()
    {
        sbyte value = (sbyte)Data[readPos];
        readPos += sizeof(sbyte);

        return value;
    }

    public short GetShort()
    {
        short value = (short)(Data[readPos] | (Data[readPos + 1] << 8));

        readPos += sizeof(short);
        return value;
    }

    public ushort GetUShort()
    {
        ushort value = (ushort)(Data[readPos] | (Data[readPos + 1] << 8));

        readPos += sizeof(ushort);
        return value;
    }

    public int GetInt()
    {
        int value = (int)(Data[readPos] | (Data[readPos + 1] << 8) | 
        (Data[readPos + 2] << 16) | (Data[readPos + 3] << 24));

        readPos += sizeof(int);
        return value;
    } 

    public uint GetUInt()
    {
        uint value = (uint)(Data[readPos] | (Data[readPos + 1] << 8) | 
        (Data[readPos + 2] << 16) | (Data[readPos + 3] << 24));
        
        readPos += sizeof(uint);
        return value;
    }

    public long GetLong()
    {
        long value = (long)(Data[readPos] | (Data[readPos + 1] << 8) | 
        (Data[readPos + 2] << 16) | (Data[readPos + 3] << 24) | 
        (Data[readPos + 4] << 32) | (Data[readPos + 5] << 40) | 
        (Data[readPos + 6] << 48) | (Data[readPos + 7] << 56));
        
        readPos += sizeof(long);
        return value;
    }

    public ulong GetULong()
    {
        ulong value = (ulong)(Data[readPos] | (Data[readPos + 1] << 8) | 
        (Data[readPos + 2] << 16) | (Data[readPos + 3] << 24) | 
        (Data[readPos + 4] << 32) | (Data[readPos + 5] << 40) | 
        (Data[readPos + 6] << 48) | (Data[readPos + 7] << 56));
        
        readPos += sizeof(ulong);
        return value;
    }

    public float GetFloat()
    {
        float value = BitConverter.ToSingle(Data, readPos);

        readPos += sizeof(float);
        return value;
    }

    public double GetDouble()
    {
        double value = BitConverter.ToDouble(Data, readPos);

        readPos += sizeof(double);
        return value;
    }

    public string GetString()
    {
        ushort length = GetUShort();
        string value = Encoding.UTF8.GetString(Data, readPos, length);
        
        readPos += length;
        return value;
    }

#endregion
}