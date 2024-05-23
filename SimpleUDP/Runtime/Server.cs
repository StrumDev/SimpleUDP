// This file is provided under The MIT License as part of SimpleUDP.
// Copyright (c) StrumDev
// For additional information please see the included LICENSE.md file or view it on GitHub:
// https://github.com/StrumDev/SimpleUDP/blob/main/LICENSE

using System;
using System.Net;
using SimpleUDP.Core;
using System.Collections.Generic;

namespace SimpleUDP
{
    public class Server : UdpNetwork
    {   
        public Action OnStart, OnStop;

        public Action<Peer> OnConnected;
        public Action<Peer> OnDisconnected;
        
        public Action<Packet, Peer> OnReceiveReliable;
        public Action<Packet, Peer> OnReceiveUnreliable;
        public Action<Packet, EndPoint> OnReceiveBroadcast;
        public Action<Packet, EndPoint> OnReceiveUnconnected;

        
        public Dictionary<UdpPeer, Peer> Connections { get; private set; } 

        protected override void OnStarted()
        {
            Connections = new Dictionary<UdpPeer, Peer>();
            
            OnStart?.Invoke();
        }

        protected override void OnPeerConnected(UdpPeer udpPeer)
        {
            lock (Connections)
            {
                Peer newPeer = new Peer(udpPeer);

                Connections.Add(udpPeer, newPeer);
                OnConnected?.Invoke(newPeer);
            }
        }

        protected override void OnPeerDisconnected(UdpPeer udpPeer)
        {
            lock (Connections)
            {   
                Peer peer = Connections[udpPeer];
                Connections.Remove(udpPeer);
                OnDisconnected?.Invoke(peer);
            }
        }
        
        protected override void OnHandlerReliable(byte[] packet, UdpPeer udpPeer)
        {
            OnReceiveReliable?.Invoke(Packet.Read(packet, packet.Length, 0), Connections[udpPeer]);
        }

        protected override void OnHandlerUnreliable(byte[] packet, UdpPeer udpPeer)
        {
            OnReceiveUnreliable?.Invoke(Packet.Read(packet, packet.Length, 0), Connections[udpPeer]);
        }

        protected override void OnHandlerBroadcast(byte[] packet, EndPoint endPoint)
        {
            OnReceiveBroadcast?.Invoke(Packet.Read(packet, packet.Length, 0), endPoint);
        }

        protected override void OnHandlerUnconnected(byte[] packet, EndPoint endPoint)
        {
            OnReceiveUnconnected?.Invoke(Packet.Read(packet, packet.Length, 0), endPoint);
        }
        
        protected override void OnStopped()
        {
            Connections.Clear();
            OnStop?.Invoke();
        }

        public void SendBroadcast(Packet packet, ushort port)
        {
            SendBroadcast(packet.Data, packet.Length, port);
        }

        public void SendUnconnected(Packet packet, EndPoint endPoint)
        {
            SendUnconnected(packet.Data, packet.Length, endPoint);
        }

        public void SendAllReliable(Packet packet, Peer ignore = null)
        {
            lock (Connections)
            {
                foreach (Peer peer in Connections.Values)
                {
                    if (!peer.Equals(ignore))
                        peer.SendReliable(packet);
                }    
            }
        }

        public void SendAllUnreliable(Packet packet, Peer ignore = null)
        {
            lock (Connections)
            {
                foreach (Peer peer in Connections.Values)
                {
                    if (!peer.Equals(ignore))
                        peer.SendUnreliable(packet);
                }    
            }
        }
    }
}