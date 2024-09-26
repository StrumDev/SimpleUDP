using System;
using System.Net;
using System.Text;
using SimpleUDP.Core;
using SimpleUDP.Utils;
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
    public enum Reason : byte
    {
        Nothing,
        Kicked,
        TimeOut,
        ServerIsFull,
        IncorrectKey,
        Disconnected,
        QuietDisconnected, 
    }
    public class UdpPeer
    {
        public uint Id { get; private set; }
        public uint Rtt { get; private set; }

        public State State { get; private set; }
        public EndPoint EndPoint { get; private set; }
        
        public Reason ReasonDisconnection { get; internal set; }
        
        public uint ElapsedMilliseconds => (uint)watch.ElapsedMilliseconds;

        internal uint TimeOut = 5000;
        internal uint Interval = 100;
        internal int DelayResendPing = 1000;
        
        internal Action<UdpPeer> OnLostConnection;

        internal const byte HeaderSize = 5;

        private SendTo sendTo;
        private Stopwatch watch;
        private UdpPending pending;
        private UdpChannel channel;

        private bool isSendingPing;
        private const float RetryTime = 1.4f;

        internal UdpPeer(SendTo sendTo)
        {
            this.sendTo = sendTo;
            
            watch = new Stopwatch();
            pending = new UdpPending(RawSend);
            channel = new UdpChannel(RawSend);

            State = State.NoConnect;
            ReasonDisconnection = Reason.Nothing;
        }

        public void Disconnect()
        {
            ReasonDisconnection = Reason.Disconnected;
            SendDisconnect(Reason.Disconnected);
        }
        
        public void QuietDisconnect()
        {
            if (State != State.NoConnect)
            {
                ReasonDisconnection = Reason.QuietDisconnected;
                
                SetDisconnected();
                OnLostConnection?.Invoke(this);
            }
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
        
        internal void UpdateTimer(uint deltaTime)
        {
            if (State != State.NoConnect)
            {
                if (State == State.Connected)
                    channel.UpdateTimer(deltaTime);
            
                pending.UpdateTimer(deltaTime, Interval);

                if (ElapsedMilliseconds >= TimeOut)
                {
                    if (State != State.NoConnect)
                    {
                        ReasonDisconnection = Reason.TimeOut;
                        
                        SetDisconnected();
                        OnLostConnection?.Invoke(this);
                    }
                }
            }
        }

        internal void SendConnect(EndPoint endPoint, uint newId, string key)
        {
            if (State == State.NoConnect)
            {
                State = State.Connecting;
                
                Id = newId;
                EndPoint = endPoint;

                SendConnectPacket(UdpHeader.Connect, newId, key);
            }
        }

        internal void SendDisconnect(Reason reason)
        {
            if (State != State.Disconnecting)
            {
                if (State == State.NoConnect)
                    return;
                
                ClearPending();

                State = State.Disconnecting;
                SendPending(UdpHeader.Disconnect, (byte)reason);
            }
        }
        
        internal void SetConnected(uint peerId)
        {
            if (State == State.Connecting)
            {
                ClearPending();

                Id = peerId;
                State = State.Connected;
                
                channel.Initialize();
                SendPing();
            }
        }

        internal void SetDisconnected()
        {
            if (State != State.NoConnect)
            {
                ClearPending();

                channel.ClearChannel();
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
        
        internal static bool KeyMatching(string key, byte[] packet, int length, int offset)
        {
            if (key.Length != length - offset) 
                return false;

            for (int i = 0; i < length - offset; i++)
            {
                if (key[i] != packet[offset + i])
                    return false;
            }

            return true;
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

        private void SendConnectPacket(byte header, uint peerId, string key)
        {
            byte[] bytesKey = Encoding.ASCII.GetBytes(key);
            byte[] packet = new byte[HeaderSize + bytesKey.Length];
            
            packet[UdpIndex.Header] = header;
            UdpConverter.SetUInt(peerId, packet, UdpIndex.Unreliable);
            Buffer.BlockCopy(bytesKey, 0, packet, HeaderSize, bytesKey.Length);

            SendPending(packet);
        }
        
        private void SetChannelInterval(uint rtt)
        {
            channel.Interval = (uint)Math.Max(10, rtt * RetryTime);
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
    }
}