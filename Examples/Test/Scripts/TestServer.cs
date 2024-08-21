using SimpleUDP;
using System.Net;
using UnityEngine;

namespace SimpleUDP.Examples
{
    public class TestServer : MonoBehaviour
    {
        public ushort Port = 12700;
        public ushort MaxConnections = 256;

        public uint TimeOut = 5000;
        public bool Broadcast = true;

        public uint Online;

        public UdpServer server;

        private void Start()
        {
            server = new UdpServer();
            
            server.OnStarted = OnStarted;
            server.OnStopped = OnStopped;

            server.OnConnected = OnConnected;
            server.OnDisconnected = OnDisconnected;

            server.OnReceiveReliable = OnReceiveReliable;
            server.OnReceiveUnreliable = OnReceiveUnreliable;

            server.OnReceiveBroadcast = OnReceiveBroadcast;
            server.OnReceiveUnconnected = OnReceiveUnconnected;

            server.TimeOut = TimeOut;
            server.MaxConnections = MaxConnections;
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 40, 250, 999));

            if (server.IsRunning)
            {
                if (GUILayout.Button("Stop Server"))
                    server.Stop();
                
                if (GUILayout.Button("Send All Reliable"))
                    SendAllReliable();

                if (GUILayout.Button("Send All Unreliable"))
                    SendAllUnreliable();
            }
            else
            {
                if (GUILayout.Button("Start Server"))
                {
                    if (server.IsAvailablePort(Port))
                        server.Start(Port, Broadcast);
                    else
                        server.Start(0, Broadcast);
                }
            }

            GUILayout.EndArea();
        }

        public void SendAllReliable()
        {
            Packet packet = Packet.Write();
            packet.String("Hello, reliable from server!");

            server.SendAllReliable(packet);
        }

        public void SendAllUnreliable()
        {
            Packet packet = Packet.Write();
            packet.String("Hello, unreliable from server!");

            server.SendAllUnreliable(packet);
        }

        private void OnStarted()
        {
            Debug.Log($"[Server] OnStarted on port: {server.LocalPort}");
        }

        private void OnStopped()
        {
            Debug.Log($"[Server] OnStopped");
        }

        private void OnConnected(UdpPeer peer)
        {
            Debug.Log($"[Server] OnConnected: {peer.EndPoint}");
        }

        private void OnDisconnected(UdpPeer peer)
        {
            Debug.Log($"[Server] OnDisconnected: {peer.EndPoint}");
        }

        private void OnReceiveReliable(UdpPeer peer, byte[] packet)
        {
            Packet read = Packet.Read(packet);

            Debug.Log($"[Server] OnReceiveReliable: {read.String()}");
        }

        private void OnReceiveUnreliable(UdpPeer peer, byte[] packet)
        {
            Packet read = Packet.Read(packet);

            Debug.Log($"[Server] OnReceiveUnreliable: {read.String()}");
        }

        private void OnReceiveBroadcast(EndPoint endPoint, byte[] packet)
        {
            Packet read = Packet.Read(packet);

            Debug.Log($"[Server] OnReceiveBroadcast: {read.String()}");
        }

        private void OnReceiveUnconnected(EndPoint endPoint, byte[] packet)
        {
            Packet read = Packet.Read(packet);

            Debug.Log($"[Server] OnReceiveUnconnected: {read.String()}");
        }

        private void FixedUpdate()
        {
            server.Receive();
            server.TickUpdate();

            Online = server.ConnectionsCount;
        }

        private void OnApplicationQuit()
        {
            server.Stop();
        }
    }
}