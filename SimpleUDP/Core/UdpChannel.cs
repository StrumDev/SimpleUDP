using System;
using SimpleUDP.Utils;
using System.Collections.Generic;

namespace SimpleUDP.Core
{
    public class UdpChannel
    {
        internal const byte IndexHeader = 0;
        internal const byte IndexAck = 1;
        internal const byte HeaderUnreliable = 1;
        internal const byte HeaderReliable = 2;
        
        internal const byte MaxPending = 128;

        internal uint Interval = 50;
        internal ushort MaxQueue = 64;
        internal ushort CapacityQueue = 128;
        
        internal Action<byte[]> RawSend;

        private byte[] sendAck;
        private byte[] receiveAck;
        private Stack<byte> indexes;
        private SerialBuffer serial;
        private UdpPending[] pending;
        private Dictionary<byte, UdpPending> inPending;
        
        private bool isInitialize;
        private object locker = new object();

        public UdpChannel(Action<byte[]> rawSend)
        {
            RawSend = rawSend;
            isInitialize = false;
        }

        internal void Initialize()
        {
            lock (locker)
            {
                sendAck = new byte[MaxPending];
                receiveAck = new byte[MaxPending];
                pending = new UdpPending[MaxPending];
                indexes = new Stack<byte>(MaxPending);
                inPending = new Dictionary<byte, UdpPending>();
                serial = new SerialBuffer(MaxQueue, CapacityQueue);

                for (int index = MaxPending - 1; index >= 0; index--)
                {
                    indexes.Push((byte)index);
                    pending[index] = new UdpPending(RawSend);
                }

                isInitialize = true;
            }  
        }

        internal void ClearAll()
        {
            lock (locker)
            {
                if (inPending != null)
                {
                    foreach (UdpPending pending in pending)
                        pending.ClearPacket();
                    
                    serial.Clear();
                    indexes.Clear(); 
                    inPending.Clear();    
                }
            }
        }
        
        internal void SendUnreliable(byte[] packet, int length, int offset)
        {
            lock (locker)
            {
                byte[] buffer = new byte[length + HeaderUnreliable];
                
                buffer[IndexHeader] = UdpHeader.Unreliable;
                Buffer.BlockCopy(packet, offset, buffer, HeaderUnreliable, length);

                RawSend(buffer);   
            }
        }

        internal void SendReliable(byte[] packet, int length, int offset)
        {
            lock (locker)
            {
                byte[] buffer = new byte[length + HeaderReliable];
                
                buffer[IndexHeader] = UdpHeader.Reliable;
                Buffer.BlockCopy(packet, offset, buffer, HeaderReliable, length);

                if (indexes.Count != 0)
                    SendPending(indexes.Pop(), buffer);
                else
                    serial.AddElement(buffer);    
            }
        }

        internal void UpdateTimer(uint deltaTime)
        {
            lock (locker)
            {
                if (isInitialize)
                {
                    foreach (UdpPending pending in inPending.Values)
                        pending.UpdateTimer(deltaTime, Interval);
                }
            }
        }

        internal bool IsNewAck(byte[] data)
        {
            lock (locker)
            {
                RawSend(new byte[]{UdpHeader.ReliableAck, data[IndexAck]});

                byte index = GetIndex(data[IndexAck]);
                
                if (receiveAck[index] != data[IndexAck])
                {
                    receiveAck[index] = data[IndexAck];
                    return true;
                }
                else return false;    
            }
        }

        internal void ClearAck(byte[] data)
        {
            lock (locker)
            {
                byte index = GetIndex(data[IndexAck]);

                if (inPending.ContainsKey(index))
                {
                    pending[index].ClearPacket();

                    if (serial.Count == 0)
                    {
                        indexes.Push(index);
                        inPending.Remove(index);
                    }
                    else SendPending(index, serial.GetElement());
                }       
            }
        }
        
        private void SendPending(byte index, byte[] buffer)
        {
            lock (locker)
            {
                NextAck(index, ref sendAck[index], out buffer[IndexAck]);
                pending[index].SendPacket(buffer);

                if (!inPending.ContainsKey(index))
                    inPending.Add(index, pending[index]);    
            }
        }

        private void NextAck(byte index, ref byte ack, out byte newAck)
        {
            newAck = (byte)((ack < 128 ? ack += 128 : ack -= 128) + index);
        }

        private byte GetIndex(byte header)
        {
            return (byte)(header < 128 ? header : header - 128);
        }
    }
}