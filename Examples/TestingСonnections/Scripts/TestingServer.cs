using System;
using SimpleUDP;
using UnityEngine;
using System.Threading;

namespace SimpleUDP.Examples
{
    public class TestingServer : MonoBehaviour
    {
        public ushort Port = 12700;
        public uint MaxConnections = 1024;

        [Header("Statistic")]
        public uint Connections;

        private UdpServer udpServer;

        private void Start()
        {
            udpServer = new UdpServer();
            udpServer.MaxConnections = MaxConnections;

            udpServer.Start(Port);

            new Thread(ReceiveThread).Start();
            new Thread(TickUpdateThread).Start();
        }
        
        private void ReceiveThread()
        {
            while (udpServer.IsRunning)
            {
                if (udpServer.SocketPoll)
                    udpServer.Receive();
                
                Thread.Sleep(10);
            }
        }
        
        private void TickUpdateThread()
        {
            while (udpServer.IsRunning)
            {
                udpServer.TickUpdate();
                Thread.Sleep(10);
            }
        }
        
        private void FixedUpdate()
        {
            Connections = udpServer.ConnectionsCount;
        }

        private void OnApplicationQuit()
        {
            udpServer.Stop();
        }
    }
}