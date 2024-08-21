using System;
using System.Net;
using SimpleUDP.Core;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SimpleUDP
{
    public enum State
    { 
        NoConnect, 
        Connecting, 
        Connected, 
        Disconnecting 
    }
    public class UdpPeer
    {
        public uint Id { get; private set; }
        public uint Rtt { get; private set; }

        public State State { get; private set; }
        public EndPoint EndPoint { get; private set; }
        
        public uint ElapsedMilliseconds => (uint)watch.ElapsedMilliseconds;

        internal uint TimeOut = 5000;
        internal uint Interval = 100;
        internal int DelayResendPing = 1000;

        internal Action<UdpPeer> OnLostConnection;
        
        private SendTo sendTo;
        private Stopwatch watch;
        private UdpPending pending;
        private UdpChannel channel;

        private bool isSendingPing;
        private const float RetryTime = 1.4f;

        private const byte HeaderSize = 5;

        internal UdpPeer(SendTo sendTo)
        {
            this.sendTo = sendTo;
            
            watch = new Stopwatch();
            pending = new UdpPending(RawSend);
            channel = new UdpChannel(RawSend);

            State = State.NoConnect;
        }

        public void SendReliable(byte[] packet)
        {
            if (State == State.Connected)
                channel.SendReliable(packet, packet.Length, 0);
        }

        public void SendUnreliable(byte[] packet)
        {
            if (State == State.Connected)
                channel.SendUnreliable(packet, packet.Length, 0);
        }
        
        public void SendReliable(byte[] packet, int length, int offset = 0)
        {
            if (State == State.Connected)
                channel.SendReliable(packet, length, offset);
        }

        public void SendUnreliable(byte[] packet, int length, int offset = 0)
        {
            if (State == State.Connected)
                channel.SendUnreliable(packet, length, offset);
        }

        public void Disconnect()
        {
            SendDisconnect();
        }
        
        public void QuietDisconnect()
        {
            if (State != State.NoConnect)
            {
                SetDisconnected();
                OnLostConnection?.Invoke(this);
            }
        }
        
        internal void UpdateTimer(uint deltaTime)
        {
            if (State != State.NoConnect)
            {
                if (State == State.Connected)
                    channel.UpdateTimer(deltaTime);
            
                pending.UpdateTimer(deltaTime, Interval);

                if (ElapsedMilliseconds >= TimeOut)
                    QuietDisconnect();
            }
        }

        internal void SendConnect(EndPoint endPoint)
        {
            if (State == State.NoConnect)
            {
                State = State.Connecting;
                
                EndPoint = endPoint;
                
                Id = (uint)endPoint.GetHashCode();
                SendPending(CreatePacket(UdpHeader.Connect, Id));
            }
        }

        internal void SendDisconnect()
        {
            if (State != State.Disconnecting)
            {
                if (State == State.NoConnect)
                    return;
                
                ClearPending();

                State = State.Disconnecting;
                SendPending(UdpHeader.Disconnect);
            }
        }
        
        internal void SetConnected(uint peerId)
        {
            if (State == State.Connecting)
            {
                ClearPending();

                Id = peerId;
                State = State.Connected;
                
                SendPing();
                channel.Initialize(); 
            }
        }

        internal void SetDisconnected()
        {
            if (State != State.NoConnect)
            {
                ClearPending();

                channel.ClearAll();
                State = State.NoConnect;
            }
        }

        internal bool IsNewAck(byte[] data)
        {
            if (State == State.Connected)
                return channel.IsNewAck(data);
            
            return false;
        }

        internal void ClearAck(byte[] data)
        {
            if (State == State.Connected)
                channel.ClearAck(data);
        }
        
        private async void SendPing()
        {
            await Task.Delay(DelayResendPing);
            
            if (State == State.Connected)
            {
                isSendingPing = true;
                SendPending(UdpHeader.Ping);
            }
        }

        internal void HandlerPong()
        {
            if (State == State.Connected && isSendingPing)
            {
                isSendingPing = false;
                ClearPending();
                SendPing();   
            }
        }

        internal void RawSend(params byte[] packet)
        {
            sendTo(packet, packet.Length, EndPoint);
        }

        private void SendPending(params byte[] packet)
        {
            watch.Restart();
            pending.SendPacket(packet);
        }

        private void ClearPending()
        {
            watch.Stop();
            pending.ClearPacket();

            Rtt = ElapsedMilliseconds;
            SetChannelInterval(Rtt);   
        }
        
        private void SetChannelInterval(uint rtt)
        {
            channel.Interval = (uint)Math.Max(10, rtt * RetryTime);
        }        
        
        private byte[] CreatePacket(byte header, uint peerId)
        {
            byte[] packet = new byte[HeaderSize];
            
            packet[0] = header;
            packet[1] = (byte)peerId;
            packet[2] = (byte)(peerId >> 8);
            packet[3] = (byte)(peerId >> 16);
            packet[4] = (byte)(peerId >> 24);

            return packet;
        }
    }
}