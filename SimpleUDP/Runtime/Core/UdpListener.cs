// This file is provided under The MIT License as part of SimpleUDP.
// Copyright (c) StrumDev
// For additional information please see the included LICENSE.md file or view it on GitHub:
// https://github.com/StrumDev/SimpleUDP/blob/main/LICENSE

using System;
using System.Net;
using System.Net.Sockets;

namespace SimpleUDP.Core.Net
{
    public class UdpListener
    {
        public bool IsRunning { get; private set; }
        public ushort LocalPort { get; private set; }
        public bool EnableBroadcast { get; private set; }
        public IPEndPoint LocalEndPoint { get; private set; }

        public int AvailablePackages => IsRunning ? socket.Available : 0;
        public bool SocketPoll => IsRunning ? socket.Poll(500000, SelectMode.SelectRead) : false; //500000 => 0.5s

        public ushort ReceiveBufferSize = 2048;

        private Socket socket;
        private EndPoint sender;

        private byte[] buffer;
        private object locker = new object();

        public void Start(ushort port = 0, bool enableBroadcast = false)
        {
            lock (locker)
            {
                if (!IsRunning)
                {
                    sender = new IPEndPoint(IPAddress.Any, 0);
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    
                    EnableBroadcast = enableBroadcast;
                    socket.EnableBroadcast = EnableBroadcast;           
                    socket.Bind(new IPEndPoint(IPAddress.Any, port));
                    
                    LocalEndPoint = (IPEndPoint)socket.LocalEndPoint;
                    LocalPort = (ushort)LocalEndPoint.Port;

                    buffer = new byte[ReceiveBufferSize];
                    IsRunning = true;
 
                    OnStarted();
                }
            }
        }
        
        public void Stop()
        {
            lock (locker)
            {
                if (IsRunning)
                {
                    socket.Close();
                    IsRunning = false;

                    OnStopped();
                }
            }
        }

        public void Receive()
        {
            lock (locker)
            {
                if (IsRunning && socket.Available != 0)
                {
                    try
                    {
                        for (int i = 0; i < socket.Available; i++)
                        {
                            int size = socket.ReceiveFrom(buffer, ref sender);
                            OnRawHandler(buffer, size, sender);
                        }
                    }
                    catch (Exception) { return; }        
                }   
            }
        }
        
        private protected void SendTo(EndPoint endPoint, params byte[] data)
        {
            SendTo(data, data.Length, endPoint);
        }

        private protected void SendTo(byte[] data, int size, EndPoint endPoint)
        {
            if (IsRunning)
                socket?.SendTo(data, size, SocketFlags.None, endPoint);
        }

        protected EndPoint NewEndPoint(string ipAddress, ushort port)
        {
            return new IPEndPoint(IPAddress.Parse(ipAddress), port);
        }

        public bool IsAvailablePort(ushort port)
        {
            lock (locker)
            {
                try
                {
                    using (UdpClient udpPort = new UdpClient(port))
                        udpPort.Close();
                    
                    return true;
                }
                catch (SocketException) { return false; }  
            }     
        }

        protected virtual void OnStarted() { }
        protected virtual void OnStopped() { }

        private protected virtual void OnRawHandler(byte[] data, int length, EndPoint endPoint) { }
    }

    public delegate void SendTo(byte[] data, int size, EndPoint endPoint);
}