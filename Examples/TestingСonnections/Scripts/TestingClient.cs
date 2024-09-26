using System;
using SimpleUDP;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SimpleUDP.Examples
{
    public class TestingClient : MonoBehaviour
    {
        public string IpAddress = "127.0.0.1";
        public ushort Port = 12700;

        public ushort MaxСonnectionСreation = 1024;
        public ushort DelayСreationСonnection = 100;

        [Header("List Clients")]
        public List<ThreadClients> Clients = new List<ThreadClients>();

        private int currentList;

        private async void Start()
        {
            currentList = 0;
            Clients.Add(new ThreadClients());

            for (int i = 0; i < MaxСonnectionСreation; i++)
            {
                lock (Clients)
                {
                    Client client = new Client(IpAddress, Port);
                    AddClientToList(client);
                }
                await Task.Delay(DelayСreationСonnection);
            }
        }

        private void AddClientToList(Client client)
        {   
            if (!Clients[currentList].TryAddClient(client))
            {
                currentList++;

                Clients.Add(new ThreadClients());
                Clients[currentList].TryAddClient(client);
            }
        }

        private void OnApplicationQuit()
        {
            lock (Clients)
            {
                foreach (ThreadClients client in Clients)
                    client.Stop();
            }
        }

        [Serializable] public class Client
        {
            public uint Rtt;
            public bool IsConnected;
            private UdpClient udpClient;

            public Client(string address, ushort port)
            {
                IsConnected = false;
                udpClient = new UdpClient();

                udpClient.OnConnected = () => IsConnected = true;
                udpClient.OnDisconnected = () => IsConnected = false;

                udpClient.Start();
                udpClient.Connect(address, port);
            }

            public void TickUpdate()
            {
                udpClient.Receive();
                udpClient.TickUpdate();

                Rtt = udpClient.Rtt;
            }

            public void Stop()
            {
                udpClient.Stop();
            }
        }
        
        [Serializable] public class ThreadClients
        {
            public List<Client> Clients;
            
            private bool isActive;
            private ushort maxClients = 128;

            public ThreadClients()
            {
                Clients = new List<Client>();
                isActive = true;

                new Thread(TickUpdateThread).Start();
            }

            public bool TryAddClient(Client client)
            {
                lock (Clients)
                {
                    if (Clients.Count < maxClients)
                    {
                        Clients.Add(client);
                        return true;
                    }
                }

                return false;
            }

            private void TickUpdateThread()
            {
                while (isActive)
                {
                    lock (Clients)
                    {
                        foreach (Client client in Clients)
                            client?.TickUpdate();
                    }
                    
                    Thread.Sleep(10);
                }
            }

            public void Stop()
            {
                isActive = false;

                lock (Clients)
                {
                    foreach (Client client in Clients)
                        client.Stop();
                }
            }
        }
    }
}