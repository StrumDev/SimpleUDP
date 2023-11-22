// This file is provided under The MIT License as part of SimpleUDP.
// Copyright (c) StrumDev
// For additional information please see the included LICENSE.md file or view it on GitHub:
// https://github.com/StrumDev/SimpleUDP/blob/main/LICENSE

using System;
using System.Net;
using System.Collections.Generic;

namespace SimpleUDP.Core
{
    public class Channel
    {
        public const uint MAX_PENDINGS = 256; // = 1 byte

        public SendTo SendTo;
        public EndPoint EndPoint;
        
        public uint Interval = 50;
        public uint MaxPendings = 16;
        public uint MaxOverflow = 512;

        public const byte LENGTH_HEADER_UNRELIABLE = 1;
        public const byte LENGTH_HEADER_RELIABLE = 3;

        public const byte HEADER = 0;
        public const byte INDEX = 1;
        public const byte ACK = 2;

        private Dictionary<ushort, Pending> pendings = new Dictionary<ushort, Pending>();
        private Queue<byte[]> segments = new Queue<byte[]>();
        
        private Stack<byte> indexes = new Stack<byte>();
        private byte[] sendAck = new byte[MAX_PENDINGS];
        private byte[] receiveAck = new byte[MAX_PENDINGS];

        public Channel(SendTo sendTo, EndPoint endPoint)
        {
            SendTo = sendTo;
            EndPoint = endPoint;

            for (int i = 0; i < MAX_PENDINGS; i++)
                indexes.Push((byte)i);
        }

        public void SendUnreliable(byte[] data, int length)
        {
            RawSend(AddHeader(data, length, Header.Unreliable));
        }

        public void SendReliable(byte[] data, int length)
        {
            lock (pendings)
            {
                if (segments.Count > MaxOverflow)
                    return;

                byte[] buffer = AddHeader(data, length, Header.Reliable, 0, 0);
                MaxPendings = Clamp(MaxPendings, 1, MAX_PENDINGS);

                if (segments.Count == 0 && pendings.Count < MaxPendings)
                    NewPending(buffer, indexes.Pop(), null);
                else
                    segments.Enqueue(buffer);
            }
        }

        public void UpdateTimer(uint deltaTime)
        {
            lock (pendings)
            {
                foreach (Pending pending in pendings.Values)
                    pending.UpdateTimer(deltaTime);
            }
        }

        public bool IsNewAck(byte[] data)
        {   
            if (data.Length >= LENGTH_HEADER_RELIABLE)
            {
                RawSend(Header.Ack, data[INDEX], data[ACK]);

                if (receiveAck[data[INDEX]] != data[ACK])
                {
                    receiveAck[data[INDEX]] = data[ACK];
                    return true;
                }
            }

            return false;
        }

        public void ClearAck(byte[] data)
        {
            lock (pendings)
            {
                if (data.Length < 3)
                    return;

                ushort key = GetUShort(data, INDEX);

                if (pendings.ContainsKey(key))
                {
                    pendings[key].ClearAck();
                    
                    if (segments.Count != 0)
                        NewPending(segments.Dequeue(), data[INDEX], pendings[key]);   
                    else 
                        indexes.Push(data[INDEX]);
                        
                    pendings.Remove(key);                
                }
            }
        }
        
        public void Close()
        {
            lock (pendings)
            {
                segments.Clear();

                foreach (Pending pending in pendings.Values)
                    pending.ClearAck();
            }
        }

        private void NewPending(byte[] data, byte index, Pending pending)
        {
            lock (pendings)
            {
                pending ??= new Pending(this);

                data[INDEX] = index;
                data[ACK] = ++sendAck[index];
                
                pending.SetPacket(data);
                pendings.Add(GetUShort(data, INDEX), pending);
            }
        }
        
        public byte[] RemoveHandler(byte[] data, int length, out bool channel)
        {
            channel = (data[HEADER] == Header.Reliable ? true : false);

            if (data[HEADER] == Header.Unreliable)
                return CopyData(data, length, LENGTH_HEADER_UNRELIABLE);
            
            if (data[HEADER] == Header.Reliable)
                return CopyData(data, length, LENGTH_HEADER_RELIABLE);

            return default;
        }
        
        private void RawSend(params byte[] data)
        {
            SendTo(data, EndPoint);
        }

        private ushort GetUShort(byte[] data, int index)
        {
            return (ushort)(data[index] | (data[index + 1] << 8));
        }

        private uint Clamp(uint value, uint min, uint max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

        private byte[] AddHeader(byte[] data, int length, params byte[] header)
        {
            byte[] buffer = new byte[length + header.Length];

            Array.Copy(header, 0, buffer, 0, header.Length);
            Array.Copy(data, 0, buffer, header.Length, length);

            return buffer;
        }

        private byte[] CopyData(byte[] data, int length, int offset)
        {
            byte[] buffer = new byte[length - offset];

            Array.Copy(data, offset, buffer, 0, buffer.Length);

            return buffer;
        }
        
        private class Pending
        {
            private Channel channel;

            private byte[] packet;
            private uint timer;
            private bool isSend;

            public Pending(Channel channel)
            {
                this.channel = channel;
            }

            public bool SetPacket(byte[] data)
            {
                if (packet != null)
                    return false;
                
                packet = data;
                TrySend();
                return true;
            }
            
            private void TrySend()
            {
                channel.RawSend(packet);
            }

            public void UpdateTimer(uint deltaTime)
            {
                if (packet != null)
                {
                    if (isSend)
                    {
                        timer += deltaTime;

                        if (timer >= channel.Interval)
                        {
                            timer = 0;
                            TrySend();
                        }
                    }
                    else isSend = true;
                }
            }

            public void ClearAck()
            {
                timer = 0;
                packet = null;
                isSend = false;
            }
        }
    }   
}