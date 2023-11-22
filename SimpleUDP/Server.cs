// This file is provided under The MIT License as part of SimpleUDP.
// Copyright (c) StrumDev
// For additional information please see the included LICENSE.md file or view it on GitHub:
// https://github.com/StrumDev/SimpleUDP/blob/main/LICENSE

using System;
using System.Net;
using System.Collections.Generic;
using SimpleUDP.Core;
using Timer = System.Timers.Timer;

namespace SimpleUDP
{
    public class Server : Listener
    {
        public Action OnStop;
        public Action<Peer> OnClientConnected;
        public Action<Peer> OnClientDisconnected;
        public Action<bool, byte[], Peer> OnHandler;

        public Dictionary<EndPoint, Peer> Peers { get; private set; }

        private const uint UPDATE_TIME_OUT = 1000;

        private Timer timer;
        
        public Server()
        {
            Peers = new Dictionary<EndPoint, Peer>();

            timer = new Timer(UPDATE_TIME_OUT);
            timer.AutoReset = true;
            timer.Elapsed += (o, e) => UpdateTimeOut();
        }

        public void Start(ushort port)
        {
            if (IsRunning)
                return;
            
            StartListener(port);
            
            timer.Start();
            Log.Info($"[Server] Start: {port}");
        }

        public void Send(bool channel, byte[] data, int length, Peer peer)
        {
            lock (Peers)
            {
                if (peer.State == State.Connected && IsRunning)
                {
                    if (channel) 
                        peer.Channel.SendReliable(data, length);
                    else 
                        peer.Channel.SendUnreliable(data, length);
                }    
            }   
        }

        public void SendAll(bool channel, byte[] data, int length)
        {
            lock (Peers)
            {
                if (IsRunning)
                {
                    if (channel)
                    {
                        foreach (Peer peer in Peers.Values)
                            peer.SendReliable(data, length);
                    } 
                    else
                    {
                        foreach (Peer peer in Peers.Values)
                            peer.SendUnreliable(data, length);
                    }
                }
            }
        }

        public void SendAll(bool channel, byte[] data, int length, Peer skip)
        {
            lock (Peers)
            {
                if (IsRunning)
                {
                    if (channel)
                    {
                        foreach (Peer peer in Peers.Values)
                            if (!peer.Equals(skip))
                                peer.SendReliable(data, length);
                    } 
                    else
                    {
                        foreach (Peer peer in Peers.Values)
                            if (!peer.Equals(skip))
                                peer.SendUnreliable(data, length);
                    }
                } 
            } 
        }

        public void Disconnect(Peer peer)
        {
            lock (Peers)
            {
                peer.Disconnect();
            }
        }

        public void UpdatePeer(uint deltaTime)
        {
            lock (Peers)
            {
                foreach (Peer peer in Peers.Values)
                    peer.UpdatePeer(deltaTime);
            }
        }

        private void UpdateTimeOut()
        {
            lock (Peers)
            {
                foreach (Peer peer in Peers.Values)
                    peer.Ping();
            }
        }

        private bool TryGetClient(byte header, EndPoint endPoint)
        {
            if (Peers.ContainsKey(endPoint))
                return true;
            
            switch (header)
            {
                case Header.Connect:
                    NewConnect(endPoint);
                    break;
                case Header.Disconnect:
                    ClientDisconnected(endPoint);
                    break;
            }

            return false;
        }

        protected override void RawHandler(byte[] data, int length, EndPoint endPoint)
        {
            if (!TryGetClient(data[Channel.HEADER], endPoint))
                return;

            switch (data[Channel.HEADER])
            {
                case Header.Unreliable:
                    HandlerUnrealible(Peers[endPoint], data, length);
                    break;
                case Header.Reliable:
                    HandlerRealible(Peers[endPoint], data, length);
                    break;
                case Header.Ack:
                    HandlerAck(Peers[endPoint], data, length);
                    break;
                case Header.Ping:
                    Peers[endPoint].RawSend(Header.Pong);
                    break;
                case Header.Pong:
                    Peers[endPoint].Pong();
                    break;
                case Header.Connected:
                    ClientConnected(endPoint);
                    break;
                case Header.Disconnect:
                    Peers[endPoint].Disconnected();
                break;
            }
        }

        private void HandlerRealible(Peer peer, byte[] data, int length)
        {
            if (length >= Channel.LENGTH_HEADER_RELIABLE)
            {
                if (!peer.Channel.IsNewAck(data))
                    return;
                
                byte[] buffer = peer.Channel.RemoveHandler(data, length, out bool channel);
                OnHandler?.Invoke(channel, buffer, peer);
            }
        }

        private void HandlerUnrealible(Peer peer, byte[] data, int length)
        {
            if (length >= Channel.LENGTH_HEADER_UNRELIABLE)
            {
                byte[] buffer = peer.Channel.RemoveHandler(data, length, out bool channel);
                OnHandler?.Invoke(channel, buffer, peer);
            }
        }

        private void HandlerAck(Peer peer, byte[] data, int length)
        {
            if (length >= Channel.LENGTH_HEADER_RELIABLE)
            {
                peer.Channel.ClearAck(data);
            }
        }

        private void NewConnect(EndPoint endPoint)
        {
            lock (Peers)
            {
                if (!Peers.ContainsKey(endPoint))
                {
                    Peer peer = new Peer(SendTo, endPoint, ClientDisconnected);
                    Peers.Add(endPoint, peer);
                    
                    peer.Connect();
                }

                Peers[endPoint].RawSend(Header.Connected);
            }
        }

        private void ClientConnected(EndPoint endPoint)
        {
            lock (Peers)
            {
                if (Peers.TryGetValue(endPoint, out Peer peer))
                {
                    if (peer.State == State.Connecting)
                    {
                        peer.Connected();
                        OnClientConnected?.Invoke(peer);

                        Log.Info($"[Server] Client Connected: {endPoint}, Online: {Peers.Count}"); 
                    }
                }
            }
        }
        
        private void ClientDisconnected(EndPoint endPoint)
        {
            lock (Peers)
            {
                if (Peers.TryGetValue(endPoint, out Peer peer))
                {
                    peer.Close();
                    Peers.Remove(endPoint);
                    OnClientDisconnected?.Invoke(peer);

                    Log.Info($"[Server] Client Disconnected: {endPoint}, Online: {Peers.Count}");
                }
                
                SendTo(endPoint, Header.Disconnected);
            }
        }

        public void Stop()
        {
            if (!IsRunning)
                return;

            foreach (Peer peer in Peers.Values)
                peer.Close();
            
            Peers.Clear();
            timer?.Stop();

            StopListener();

            OnStop?.Invoke();
            Log.Info($"[Server] Stopped");
        }
    }
}