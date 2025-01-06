using System;
using System.Net;
using SimpleUDP.Core;

namespace SimpleUDP
{
    public static class Extensions
    {

#region UdpServer

        public static void SendReliable(this UdpServer server, uint peerId, Packet packet)
        {
            server.SendReliable(peerId, packet.Data, packet.Length, 0);
        }

        public static void SendUnreliable(this UdpServer server, uint peerId, Packet packet)
        {
            server.SendUnreliable(peerId, packet.Data, packet.Length, 0);
        }

        public static void SendAllReliable(this UdpServer server, Packet packet, uint ignoreId = 0)
        {
            server.SendAllReliable(packet.Data, packet.Length, 0, ignoreId);
        }

        public static void SendAllUnreliable(this UdpServer server, Packet packet, uint ignoreId = 0)
        {
            server.SendAllUnreliable(packet.Data, packet.Length, 0, ignoreId);
        }

        public static void SendBroadcast(this UdpServer server, ushort port, Packet packet)
        {
            server.SendBroadcast(port, packet.Data, packet.Length);
        }

        public static void SendUnconnected(this UdpServer server, EndPoint endPoint, Packet packet)
        {
            server.SendUnconnected(endPoint, packet.Data, packet.Length);
        }

#endregion

#region UdpClient

        public static void SendReliable(this UdpClient client, Packet packet)
        {
            client.SendReliable(packet.Data, packet.Length, 0);
        }

        public static void SendUnreliable(this UdpClient client, Packet packet)
        {
            client.SendUnreliable(packet.Data, packet.Length, 0);
        }

        public static void SendBroadcast(this UdpClient client, ushort port, Packet packet)
        {
            client.SendBroadcast(port, packet.Data, packet.Length);
        }

        public static void SendUnconnected(this UdpClient client, EndPoint endPoint, Packet packet)
        {
            client.SendUnconnected(endPoint, packet.Data, packet.Length);
        }

#endregion

#region UdpPeer

        public static void SendReliable(this UdpPeer peer, Packet packet)
        {
            peer.SendReliable(packet.Data, packet.Length, 0);
        }

        public static void SendUnreliable(this UdpPeer peer, Packet packet)
        {
            peer.SendUnreliable(packet.Data, packet.Length, 0);
        }

#endregion

    }
}