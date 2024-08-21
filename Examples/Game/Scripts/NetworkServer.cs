using SimpleUDP;
using UnityEngine;
using System.Collections.Generic;

namespace SimpleUDP.Examples
{
    public class NetworkServer : MonoBehaviour
    {   
        public uint OnlinePlayers;
        
        private List<UdpPeer> players = new List<UdpPeer>();

        private UdpServer Server => NetworkManager.Server;

        private void Awake()
        {
            Server.OnStarted += OnStarted;
            Server.OnStopped += OnStopped;

            Server.OnConnected += OnConnected;
            Server.OnDisconnected += OnDisconnected;

            Server.OnReceiveReliable += ReceiveHandler;
            Server.OnReceiveUnreliable += ReceiveHandler;
        }

        private void FixedUpdate()
        {
            OnlinePlayers = Server.ConnectionsCount;
        }
        
        private void OnStarted()
        {
            players.Clear();
            Debug.Log($"[Server] Started: {Server.LocalPort}");
        }

        private void OnConnected(UdpPeer peer)
        {
            lock (players)
            {
                players.Add(peer);

                SendConnectedClients(peer);

                Server.SendAllReliable(Packet.Byte((byte)Header.ClientConnected).UInt(peer.Id), peer.Id);
                
                Debug.Log($"[Server] Client Connected: {peer.Id}");    
            }
        }

        private void OnDisconnected(UdpPeer peer)
        {
            lock (players)
            {
                players.Remove(peer);

                Server.SendAllReliable(Packet.Byte((byte)Header.ClientDisconnected).UInt(peer.Id), peer.Id);

                Debug.Log($"[Server] Client Disconnected: {peer.Id}");   
            }
        }
        
        private void SendConnectedClients(UdpPeer peer)
        {
            lock (players)
            {
                foreach (UdpPeer player in players)
                {
                    if (player.Id != peer.Id)
                        peer.SendReliable(Packet.Byte((byte)Header.ClientConnected).UInt(player.Id));
                }    
            }   
        }

        private void ReceiveHandler(UdpPeer peer, byte[] packet)
        {
            Packet read = Packet.Read(packet);

            switch ((Header)read.Byte())
            {
                case Header.Movement:
                    Movement(read, peer);
                return;
            }
        }

        private void Movement(Packet packet, UdpPeer peer)
        {
            Server.SendAllUnreliable(packet, peer.Id);
        }
        
        private void OnStopped()
        {
            players.Clear();
            Debug.Log($"[Server] Stopped!");
        }
    }    
}