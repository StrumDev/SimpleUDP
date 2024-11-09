using System;
using SimpleUDP.Utils;

namespace SimpleUDP.Core
{
    public class UdpChannel
    {  
        internal Action<byte[]> RawSend;
        
        internal uint Interval = 50;
        internal byte MaxPending = 64;

        internal ushort MaxQueue = 4;
        internal ushort CapacityQueue = 16;
        
        internal const byte MaximumCount = 128;
        
        private byte[] sendAck;
        private byte[] receiveAck;

        private UdpBuffer<byte[]> udpBuffer;
        private UdpActive<UdpPending> pendings;

        private object locker = new object();
        private object lockerAck = new object();

        public UdpChannel(Action<byte[]> rawSend)
        {
            RawSend = rawSend;
        }

        internal void Initialize()
        {
            sendAck = new byte[MaxPending];
            receiveAck = new byte[MaximumCount];

            pendings = new UdpActive<UdpPending>(MaxPending);

            for (int index = 0; index < MaxPending; index++)
                pendings.AddElement(new UdpPending(RawSend));
            
            udpBuffer = new UdpBuffer<byte[]>(MaxQueue, CapacityQueue);
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

                if (pendings.TrySetActive(out byte index))
                    SendPending(index, buffer);
                else 
                    udpBuffer.AddElement(buffer);
            }
        }

        private void SendPending(byte index, byte[] buffer)
        {
            NextAck(index, ref sendAck[index], out buffer[UdpIndex.Ack]);
            pendings[index].SendPacket(buffer);
        }

        internal void UpdateTimer(uint deltaTime)
        {
            lock (locker)
            {
                for (int index = 0; index < pendings.ActiveCount; index++)
                    pendings.GetActive(index)?.UpdateTimer(deltaTime, Interval);
            }
        }

        internal bool IsNewAck(byte[] data)
        {
            lock (lockerAck)
            {
                byte index = GetIndex(data[UdpIndex.Ack]);
                RawSend(new byte[]{UdpHeader.ReliableAck, data[UdpIndex.Ack]});
                
                if (receiveAck[index] != data[UdpIndex.Ack])
                {
                    receiveAck[index] = data[UdpIndex.Ack];
                    return true;
                }
                else return false;  
            }
        }

        internal void ClearAck(byte[] data)
        {
            lock (locker)
            {
                byte index = GetIndex(data[UdpIndex.Ack]);

                if (pendings[index].buffer == null)
                    return;
                
                if (pendings[index].buffer[UdpIndex.Ack] == data[UdpIndex.Ack])
                {
                    pendings[index].ClearPacket();

                    if (udpBuffer.Count != 0)
                        SendPending(index, udpBuffer.GetElement());
                    else
                        pendings.RemoveActive(index);
                }   
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
        
        internal void ClearChannel()
        {
            if (pendings != null)
            {
                for (int i = 0; i < MaxPending; i++)
                    pendings[i].ClearPacket();

                udpBuffer.Clear();   
            }
        }
    }
}