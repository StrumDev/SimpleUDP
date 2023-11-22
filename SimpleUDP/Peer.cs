// This file is provided under The MIT License as part of SimpleUDP.
// Copyright (c) StrumDev
// For additional information please see the included LICENSE.md file or view it on GitHub:
// https://github.com/StrumDev/SimpleUDP/blob/main/LICENSE

using System;
using System.Net;
using SimpleUDP.Core;

namespace SimpleUDP
{
    public enum State { NoConnect, Connecting, Connected, Disconnecting }
    public class Peer
    {
        public uint RTT { get; private set; } //Round trip time
        public State State { get; private set; }
        public EndPoint EndPoint { get; private set; }
        
        internal Channel Channel;

        internal uint Interval = 250;
        internal uint MaxAttempts = 16;
        internal uint MaxPingLoss = 4;
        
        private SendTo sendTo;
        private DateTime lastRTT;
        private Action<EndPoint> disconnected;
        
        private const float RetryTime = 1.4f;

        private uint attempt;
        private byte[] packetBuffer;
        private uint timer;
        private bool isSend;

        internal Peer(SendTo sendTo, EndPoint endPoint, Action<EndPoint> disconnected)
        {
            this.sendTo = sendTo;
            this.EndPoint = endPoint;
            this.disconnected = disconnected;

            Channel = new Channel(sendTo, endPoint);
        }

        internal void Connect()
        {  
            if (State == State.NoConnect)
            {
                State = State.Connecting;
                TrySend(Header.Connect);
            }
        }

        internal void Connected()
        {
            if (State == State.Connecting)
            {
                State = State.Connected;
                ClearSend();
            }
        }

        internal void SendReliable(byte[] data, int length)
        {
            if (State == State.Connected)
                Channel.SendReliable(data, length);
        }

        internal void SendUnreliable(byte[] data, int length)
        {
            if (State == State.Connected && attempt < MaxPingLoss)
                Channel.SendUnreliable(data, length); 
        }

        internal void UpdatePeer(uint deltaTime)
        {
            if (State == State.Connected && attempt < MaxPingLoss)
                Channel.UpdateTimer(deltaTime);
            
            UpdateTimer(deltaTime);
        }

        internal void Disconnect()
        {  
            if (State == State.Connected)
            {
                State = State.Disconnecting;
                ClearSend();

                Interval = 50;
                TrySend(Header.Disconnect);
            }
        }

        internal void Disconnected()
        {
            ClearSend();
            disconnected?.Invoke(EndPoint);
        }

        internal void Ping()
        {
            if (packetBuffer != null && State != State.Connected)
                return;

            lastRTT = DateTime.UtcNow;
            TrySend(Header.Ping);
        }

        internal void Pong()
        {
            RTT = (uint)(DateTime.UtcNow - lastRTT).Milliseconds;
            Channel.Interval = (uint)Math.Max(10, RTT * RetryTime);
            
            ClearSend();
        }
        
        internal void RawSend(params byte[] packet)
        {
            sendTo(packet, EndPoint);
        }

        private void TrySend(params byte[] packet)
        {
            if (attempt < MaxAttempts)
            {
                RawSend(packet);

                attempt++;
                packetBuffer = packet;
                return;
            }
            
            Disconnected();
        }

        private void UpdateTimer(uint deltaTime)
        {
            if (packetBuffer != null)
            {
                if (isSend)
                {
                    timer += deltaTime;

                    if (timer >= Interval)
                    {
                        timer = 0;
                        TrySend(packetBuffer);
                    }
                }
                else isSend = true;
            }
        }

        private void ClearSend()
        {
            timer = 0;
            attempt = 0;
            isSend = false;
            packetBuffer = null;
        }
        
        internal void Close()
        {
            ClearSend();
            Channel.Close();
            State = State.NoConnect;
        }
    }
}