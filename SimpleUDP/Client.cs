// This file is provided under The MIT License as part of SimpleUDP.
// Copyright (c) StrumDev
// For additional information please see the included LICENSE.md file or view it on GitHub:
// https://github.com/StrumDev/SimpleUDP/blob/main/LICENSE

using System;
using System.Net;
using SimpleUDP.Core;
using Timer = System.Timers.Timer;

namespace SimpleUDP
{
    public class Client : Listener
    {
        public Peer Peer { get; private set; }
        public State State => Peer != null ? Peer.State : State.NoConnect;

        public Action OnConnected;
        public Action OnDisconnected;
        public Action<bool, byte[]> OnHandler;

        private const uint UPDATE_TIME_OUT = 1000;
        
        private Timer timer;
        private EndPoint serverEP;

        public Client()
        {
            timer = new Timer(UPDATE_TIME_OUT);
            timer.AutoReset = true;
            timer.Elapsed += (o, e) => UpdateTimeOut();
        }

        public void Connect(string ip, ushort port)
        {
            if (IsRunning)
                return;
            
            serverEP = new IPEndPoint(IPAddress.Parse(ip), port);
            Peer = new Peer(SendTo, serverEP, Disconnected);

            StartListener();

            Log.Info($"[Client] Connect to: {serverEP}");

            Peer.Connect();
        }

        public void Send(bool channel, byte[] data, int length)
        {
            if (IsRunning)
            {
                if (channel) 
                    Peer.SendReliable(data, length);
                else 
                    Peer.SendUnreliable(data, length);
            }       
        }

        public void Disconnect()
        {
            if (State == State.Connected)
                Peer.Disconnect();
        }

        public void UpdatePeer(uint deltaTime)
        {  
            if (IsRunning)
                Peer.UpdatePeer(deltaTime);
        }

        private void UpdateTimeOut()
        {
            if (State == State.Connected)
                Peer.Ping();
        }

        private bool IsConnected(byte header, EndPoint endPoint)
        {
            if (!serverEP.Equals(endPoint))
                return false;

            if (State == State.Connected)
                return true;

            switch (header)
            {
                case Header.Connect:
                    Peer.RawSend(Header.Connected);
                    break;
                case Header.Connected:
                    Connected();
                    break;
                case Header.Disconnected:
                    Peer.Disconnected();
                    break;
            }

            return false;
        }

        protected override void RawHandler(byte[] data, int length, EndPoint endPoint)
        {
            if (!IsConnected(data[Channel.HEADER], endPoint))
                return;

            switch (data[Channel.HEADER])
            {
                case Header.Unreliable:
                    HandlerUnrealible(data, length);
                    break;
                case Header.Reliable:
                    HandlerRealible(data, length);
                    break;
                case Header.Ack:
                    HandlerAck(data, length);
                    break;
                case Header.Ping:
                    Peer.RawSend(Header.Pong);
                    break;
                case Header.Pong:
                    Peer.Pong();
                    break;
                case Header.Disconnect:
                    Peer.Disconnect();
                break;
            }
        }

        private void HandlerRealible(byte[] data, int length)
        {
            if (length >= Channel.LENGTH_HEADER_RELIABLE)
            {
                if (!Peer.Channel.IsNewAck(data))
                    return;
                
                byte[] buffer = Peer.Channel.RemoveHandler(data, length, out bool channel);
                OnHandler?.Invoke(channel, buffer);
            }
        }

        private void HandlerUnrealible(byte[] data, int length)
        {
            if (length >= Channel.LENGTH_HEADER_UNRELIABLE)
            {
                byte[] buffer = Peer.Channel.RemoveHandler(data, length, out bool channel);
                OnHandler?.Invoke(channel, buffer);
            }
        }

        private void HandlerAck(byte[] data, int length)
        {
            if (length >= Channel.LENGTH_HEADER_RELIABLE)
            {
                Peer.Channel.ClearAck(data);
            }
        }

        private void Connected()
        {
            if (State == State.Connecting)
            {
                Peer.Connected();
                timer.Start();
                OnConnected?.Invoke();

                Log.Info($"[Client] Connected to: {serverEP}");
            }
        }

        private void Disconnected(EndPoint endPoint)
        {
            Stop();
            
            Log.Info($"[Client] Disconnected from: {endPoint}");
        }

        public void Stop()
        {
            if (!IsRunning)
                return;

            Peer?.Close();
            timer?.Stop();

            StopListener();
            OnDisconnected?.Invoke();
        }
    }    
}