using SimpleUDP;
using UnityEngine;
using System.Collections.Generic;

namespace SimpleUDP.Examples
{
    public class NetworkClient : MonoBehaviour
    {
        public GameObject PlayerPrefab;

        private NetworkPlayer localPlayer;
        private Dictionary<uint, NetworkPlayer> players = new Dictionary<uint, NetworkPlayer>();

        private UdpClient Client => NetworkManager.Client;


        private void Awake()
        {
            Client.OnStarted += OnStarted;
            Client.OnStopped += OnStopped;

            Client.OnConnected += OnConnected;
            Client.OnDisconnected += OnDisconnected;

            Client.OnReceiveReliable += ReceiveHandler;
            Client.OnReceiveUnreliable += ReceiveHandler;
        }

        private void OnStarted()
        {
            players.Clear();
            Debug.Log($"[Client] Started: {Client.LocalPort}");
        }
        
        private void OnConnected()
        {
            localPlayer = Instantiate(PlayerPrefab).GetComponent<NetworkPlayer>();
            
            localPlayer.IsLocal = true;
            localPlayer.ClientId = Client.Id;
            localPlayer.SendAsyncPosition();

            Debug.Log($"[Client] Connected to: {Client.EndPoint}");
        }

        private void OnDisconnected()
        {
            lock (players)
            {
                localPlayer?.Destroy();
                
                foreach (NetworkPlayer player in players.Values)
                    player.Destroy();
                
                players.Clear();

                Debug.Log($"[Client] Disconnected from: {Client.EndPoint}");  
            }
        }
        
        private void ReceiveHandler(byte[] packet)
        {
            Packet read = Packet.Read(packet);

            switch ((Header)read.Byte())
            {
                case Header.Movement:
                    Movement(read);
                    return;
                case Header.ClientConnected:
                    ClientConnected(read);
                    return;
                case Header.ClientDisconnected:
                    ClientDisconnected(read);
                    return;
            }
        }

        private void Movement(Packet packet)
        {
            if (players.TryGetValue(packet.UInt(), out NetworkPlayer player))   
                player.SetTransform(packet);
        }

        private void ClientConnected(Packet packet)
        {
            lock (players)
            {
                NetworkPlayer player = Instantiate(PlayerPrefab).GetComponent<NetworkPlayer>();
                
                player.IsLocal = false;
                player.ClientId = packet.UInt();

                players.Add(player.ClientId, player);   
            }
        }

        private void ClientDisconnected(Packet packet)
        {
            lock (players)
            {
                NetworkPlayer player = players[packet.UInt()];
                
                player.Destroy();
                players.Remove(player.ClientId);   
            }  
        }
        
        private void OnStopped()
        {
            Debug.Log($"[Client] Stopped!");
        }
    }    
}