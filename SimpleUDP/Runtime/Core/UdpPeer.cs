// This file is provided under The MIT License as part of SimpleUDP.
// Copyright (c) StrumDev
// For additional information please see the included LICENSE.md file or view it on GitHub:
// https://github.com/StrumDev/SimpleUDP/blob/main/LICENSE

using System;
using System.Net;
using SimpleUDP.Core.Net;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SimpleUDP.Core
{    
    public class UdpPeer
    {
        public uint Rtt { get; private set; }
        public EndPoint EndPoint { get; private set; }
        public uint ElapsedMilliseconds => (uint)watch.ElapsedMilliseconds;

        internal uint TimeOut = 5000;
        internal uint Interval = 100;
        internal int DelayResendPing = 500;

        private const float RetryTime = 1.4f;

        internal Action<UdpPeer> OnConnected;
        internal Action<UdpPeer> OnDisconnected;
        internal State State = State.NoConnect;
        
        private SendTo sendTo;
        private Stopwatch watch;
        private UdpPending pending;
        private UdpChannel channel;

        internal UdpPeer() { }

        internal UdpPeer(SendTo sendTo)
        {
            this.sendTo = sendTo;

            watch = new Stopwatch();
            pending = new UdpPending(RawSend);
            channel = new UdpChannel(RawSend);
        }
        
        internal void RawSend(params byte[] packet)
        {
            sendTo(packet, packet.Length, EndPoint);
        }

        public void SendReliabe(byte[] packet, int length)
        {
            if (State == State.Connected)
                channel.SendReliabe(packet, length);
        }

        public void SendUnreliabe(byte[] packet, int length)
        {
            if (State == State.Connected)
                channel.SendUnreliabe(packet, length);
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

        internal void Connect(EndPoint endPoint)
        {
            if (State == State.NoConnect)
            {
                State = State.Connecting;

                EndPoint = endPoint;
                SendPending(Header.Connect);
            }
        }

        internal void Disconnect()
        {
            if (State == State.Connected)
            {
                State = State.Disconnecting;
                SendPending(Header.Disconnect);
            }
        }

        internal void Connected()
        {
            if (State == State.Connecting)
            {
                ClearPending();
                channel.Initialize();
                
                SendPing();
                State = State.Connected;
                OnConnected?.Invoke(this);
            }
        }

        internal void Disconnected()
        {
            if (State != State.NoConnect)
            {
                channel.ClearAll();
                ClearPending();
                
                State = State.NoConnect;
                OnDisconnected?.Invoke(this);
            }
        }

        internal void HandlerPong()
        {
            ClearPending();
            SendPing();
        }

        internal void UpdateTimer(uint deltaTime)
        {
            if (State == State.Connected)
                channel.UpdateTimer(deltaTime);

            pending.UpdateTimer(deltaTime, Interval);

            if (ElapsedMilliseconds >= TimeOut)
                Disconnected();
        }
        
        private async void SendPing()
        {
            await Task.Delay(DelayResendPing);
            SendPending(Header.Ping);
        }
        
        private void SendPending(params byte[] packet)
        {
            watch.Restart();
            pending.SendPacket(packet);
        }

        private void SetChannelInterval(uint rtt)
        {
            channel.Interval = (uint)Math.Max(10, rtt * RetryTime);
        }

        private void ClearPending()
        {
            watch.Stop();
            pending.ClearPacket();

            Rtt = (uint)watch.ElapsedMilliseconds;
            SetChannelInterval(Rtt);
        }
    }
}