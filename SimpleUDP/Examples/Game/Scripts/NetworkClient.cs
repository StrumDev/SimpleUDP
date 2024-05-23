using SimpleUDP;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SimpleUDP.Examples
{
    public class NetworkClient : MonoBehaviour
    {
        public GameObject PlayerPrefab;

        private NetworkPlayer localPlayer;
        private Dictionary<uint, NetworkPlayer> players = new Dictionary<uint, NetworkPlayer>();

        private void Start()
        {
            NetworkManager.Client.OnStart += OnStartClient;

            NetworkManager.Client.OnReceiveReliable += Handler;
            NetworkManager.Client.OnReceiveUnreliable += Handler;
            
            NetworkManager.Client.OnDisconnected += OnDisconnected;
        }

        private void OnStartClient()
        {
            UpdateReceive();
            UpdateTimer();
        }

        private void Handler(Packet packet)
        {
            switch ((Header)packet.Byte())
            {
                case Header.Movement:
                    Movement(packet);
                    return;
                case Header.ClientConnected:
                    ClientConnected(packet);
                    return;
                case Header.ClientDisconnected:
                    ClientDisconnected(packet);
                    return;
                case Header.Connected:
                    Connected(packet);
                return;
            }
        }

        private void Movement(Packet packet)
        {
            if (players.TryGetValue(packet.UInt(), out NetworkPlayer player))   
                player.SetTransform(packet);
        }

        private void Connected(Packet packet)
        {
            localPlayer = Instantiate(PlayerPrefab).GetComponent<NetworkPlayer>();
            
            localPlayer.IsLocal = true;
            localPlayer.ClientId = packet.UInt();
        }

        private void ClientConnected(Packet packet)
        {
            NetworkPlayer player = Instantiate(PlayerPrefab).GetComponent<NetworkPlayer>();
            
            player.IsLocal = false;
            player.ClientId = packet.UInt();

            players.Add(player.ClientId, player);
        }

        private void ClientDisconnected(Packet packet)
        {
            NetworkPlayer player = players[packet.UInt()];
            player.IsDestroy = true;

            players.Remove(player.ClientId);
        }

        private void OnDisconnected()
        {
            if (localPlayer != null)
            {
                localPlayer.IsDestroy = true;
                localPlayer = null;

                foreach (NetworkPlayer player in players.Values)
                    player.IsDestroy = true;
                
                players.Clear();    
            }
        }

        private async void UpdateReceive()
        {
            while (NetworkManager.Client.IsRunning)
            {
                NetworkManager.Client.Receive();
                await Task.Delay(10);
            }
        }

        private async void UpdateTimer()
        {
            while (NetworkManager.Client.IsRunning)
            {
                NetworkManager.Client.UpdatePeer(10);
                await Task.Delay(10);
            }
        }
    }    
}