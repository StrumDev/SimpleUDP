using SimpleUDP;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace SimpleUDP.Examples
{
    public class NetworkServer : MonoBehaviour
    {
        private Dictionary<Peer, uint> clientIds = new Dictionary<Peer, uint>();

        private void Start()
        {
            NetworkManager.Server.OnStart += OnStartServer;
            NetworkManager.Server.OnStart += OnStopServer;
            
            NetworkManager.Server.OnReceiveReliable += Handler;
            NetworkManager.Server.OnReceiveUnreliable += Handler;

            NetworkManager.Server.OnConnected += ClientConnected;
            NetworkManager.Server.OnDisconnected += ClientDisconnected;        
        }

        

        private void OnStartServer()
        {
            UpdateReceive();
            UpdateTimer();
        }
        
        private void OnStopServer()
        {
            clientIds.Clear();
        }

        private void Handler(Packet packet, Peer peer)
        {
            switch ((Header)packet.Byte())
            {
                case Header.Movement:
                    Movement(packet, peer);
                return;
            }
        }

        private void Movement(Packet packet, Peer peer)
        {
            NetworkManager.Server.SendAllReliable(packet, peer);
        }

        private void ClientConnected(Peer peer)
        {
            uint newId = (uint)peer.GetHashCode();
            clientIds.Add(peer, newId);

            SendConnectedClient(peer);

            NetworkManager.Server.SendAllReliable(Packet.Byte((byte)Header.ClientConnected).UInt(newId), peer);
        }

        private void SendConnectedClient(Peer peer)
        {
            peer.SendReliable(Packet.Byte((byte)Header.Connected).UInt(clientIds[peer]));

            foreach (uint clientId in clientIds.Values)
            {
                if (clientId != clientIds[peer])
                    peer.SendReliable(Packet.Byte((byte)Header.ClientConnected).UInt(clientId));
            }       
        }

        private void ClientDisconnected(Peer peer)
        {
            uint clientId = clientIds[peer];
            clientIds.Remove(peer);

            NetworkManager.Server.SendAllReliable(Packet.Byte((byte)Header.ClientDisconnected).UInt(clientId), peer);
        }

        private async void UpdateReceive()
        {
            while (NetworkManager.Server.IsRunning)
            {
                NetworkManager.Server.Receive();
                await Task.Delay(10);
            }
        }

        private async void UpdateTimer()
        {
            while (NetworkManager.Server.IsRunning)
            {
                NetworkManager.Server.UpdatePeer(10);
                await Task.Delay(10);
            }
        }
    }    
}