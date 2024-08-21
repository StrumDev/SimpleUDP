using System;
using System.Net;
using SimpleUDP.Core;

namespace SimpleUDP
{
    public static class Extensions
    {

#region UdpListener

        public static void SendBroadcast(this UdpListener listener, ushort port, Packet packet)
        {
            listener.SendBroadcast(port, packet.Data, packet.Length);
        }

        public static void SendUnconnected(this UdpListener listener, EndPoint endPoint, Packet packet)
        {
            listener.SendUnconnected(endPoint, packet.Data, packet.Length);
        }

#endregion

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