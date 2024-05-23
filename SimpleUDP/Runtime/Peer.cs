// This file is provided under The MIT License as part of SimpleUDP.
// Copyright (c) StrumDev
// For additional information please see the included LICENSE.md file or view it on GitHub:
// https://github.com/StrumDev/SimpleUDP/blob/main/LICENSE

using System.Net;
using SimpleUDP.Core;

namespace SimpleUDP
{
    public class Peer
    {
        public uint Rtt => udpPeer.Rtt;
        public State State => udpPeer.State;
        public EndPoint EndPoint => udpPeer.EndPoint;
        
        private UdpPeer udpPeer;

        internal Peer(UdpPeer peer)
        {
            udpPeer = peer;
        }
        
        public void Disconnect() => udpPeer.Disconnect();
        public void Disconnected() => udpPeer.Disconnected();

        public void UpdateTimer(uint deltaTime) => udpPeer.UpdateTimer(deltaTime);
        public void SendReliable(Packet packet) => udpPeer.SendReliabe(packet.Data, packet.Length);
        public void SendUnreliable(Packet packet) => udpPeer.SendUnreliabe(packet.Data, packet.Length);
    }
}