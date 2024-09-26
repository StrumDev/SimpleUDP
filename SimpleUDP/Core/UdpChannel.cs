using System;
using SimpleUDP.Utils;
using System.Collections.Generic;

namespace SimpleUDP.Core
{
    public class UdpChannel
    {   
        internal uint Interval = 50;
        internal ushort MaxQueue = 64;
        internal ushort CapacityQueue = 128;
        
        internal Action<byte[]> RawSend;
        
        internal const byte MaxPending = 128;

        private byte[] sendAck;
        private byte[] receiveAck;
        private Stack<byte> indexes;
        private UdpBuffer<byte[]> udpBuffer;
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
                udpBuffer = new UdpBuffer<byte[]>(MaxQueue, CapacityQueue);

                for (int index = MaxPending - 1; index >= 0; index--)
                {
                    indexes.Push((byte)index);
                    pending[index] = new UdpPending(RawSend);
                }

                isInitialize = true;
            }  
        }

        internal void ClearChannel()
        {
            lock (locker)
            {
                if (inPending != null)
                {
                    isInitialize = false;

                    foreach (UdpPending pending in pending)
                        pending.ClearPacket();
                    
                    indexes.Clear(); 
                    udpBuffer.Clear();
                    inPending.Clear();
                }
            }
        }
        
        internal void SendUnreliable(byte[] packet, int length, int offset)
        {
            byte[] buffer = new byte[length + UdpIndex.Unreliable];
            
            buffer[UdpIndex.Header] = UdpHeader.Unreliable;
            Buffer.BlockCopy(packet, offset, buffer, UdpIndex.Unreliable, length);

            RawSend(buffer);   
        }

        internal void SendReliable(byte[] packet, int length, int offset)
        {
            lock (locker)
            {
                byte[] buffer = new byte[length + UdpIndex.Reliable];
                
                buffer[UdpIndex.Header] = UdpHeader.Reliable;
                Buffer.BlockCopy(packet, offset, buffer, UdpIndex.Reliable, length);

                if (indexes.Count != 0)
                    SendPending(indexes.Pop(), buffer);
                else
                    udpBuffer.AddElement(buffer);    
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
            RawSend(new byte[]{UdpHeader.ReliableAck, data[UdpIndex.Ack]});

            byte index = GetIndex(data[UdpIndex.Ack]);
            
            if (receiveAck[index] != data[UdpIndex.Ack])
            {
                receiveAck[index] = data[UdpIndex.Ack];
                return true;
            }
            else return false;
        }

        internal void ClearAck(byte[] data)
        {
            lock (locker)
            {
                byte index = GetIndex(data[UdpIndex.Ack]);

                if (inPending.ContainsKey(index))
                {
                    pending[index].ClearPacket();

                    if (udpBuffer.Count == 0)
                    {
                        indexes.Push(index);
                        inPending.Remove(index);
                    }
                    else SendPending(index, udpBuffer.GetElement());
                }       
            }
        }
        
        private void SendPending(byte index, byte[] buffer)
        {
            NextAck(index, ref sendAck[index], out buffer[UdpIndex.Ack]);
            pending[index].SendPacket(buffer);

            if (!inPending.ContainsKey(index))
                inPending.Add(index, pending[index]);    
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