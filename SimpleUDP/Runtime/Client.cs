// This file is provided under The MIT License as part of SimpleUDP.
// Copyright (c) StrumDev
// For additional information please see the included LICENSE.md file or view it on GitHub:
// https://github.com/StrumDev/SimpleUDP/blob/main/LICENSE

using System;
using System.Net;
using SimpleUDP.Core;

namespace SimpleUDP
{
    public class Client : UdpNetwork
    {
        public uint Rtt => udpPeer.Rtt;
        public State State => udpPeer.State;
        
        public Action OnStart, OnStop;

        public Action OnConnected;
        public Action OnDisconnected;
        
        public Action<Packet> OnReceiveReliable;
        public Action<Packet> OnReceiveUnreliable;
        public Action<Packet, EndPoint> OnReceiveBroadcast;
        public Action<Packet, EndPoint> OnReceiveUnconnected;
        
        private UdpPeer udpPeer;

        public Client()
        {
            MaxConnections = 1;
            udpPeer = new UdpPeer();
        }

        public void Connect(string ipAddress, ushort port)
        {
            CreateConnect(NewEndPoint(ipAddress, port), out udpPeer);
        }

        public void Disconnect()
        {
            udpPeer?.Disconnect();
        }

        public void Disconnected()
        {
            udpPeer?.Disconnected();
        }

        protected override void OnStarted()
        {
            base.OnStarted();
            OnStart?.Invoke();
        }
        
        protected override void OnPeerConnected(UdpPeer udpPeer)
        {
            OnConnected?.Invoke();
        }

        protected override void OnPeerDisconnected(UdpPeer udpPeer)
        {
            OnDisconnected?.Invoke();
        }
        
        protected override void OnHandlerReliable(byte[] packet, UdpPeer udpPeer)
        {
            OnReceiveReliable?.Invoke(Packet.Read(packet, packet.Length, 0));
        }

        protected override void OnHandlerUnreliable(byte[] packet, UdpPeer udpPeer)
        {
            OnReceiveUnreliable?.Invoke(Packet.Read(packet, packet.Length, 0));
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
            base.OnStopped();
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
        
        public void SendReliable(Packet packet) => udpPeer?.SendReliabe(packet.Data, packet.Length);
        public void SendUnreliable(Packet packet) => udpPeer?.SendUnreliabe(packet.Data, packet.Length);
    }
}