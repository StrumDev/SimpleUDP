// This file is provided under The MIT License as part of SimpleUDP.
// Copyright (c) StrumDev
// For additional information please see the included LICENSE.md file or view it on GitHub:
// https://github.com/StrumDev/SimpleUDP/blob/main/LICENSE

using System;
using System.Net;
using System.Net.Sockets;

namespace SimpleUDP.Core
{
    public static class Header
    {
        public const byte Connect = 1;
        public const byte Connected = 2;
        public const byte Disconnect = 3;
        public const byte Disconnected = 4;
        public const byte Unreliable = 5;
        public const byte Reliable = 6;
        public const byte Ack = 7;
        public const byte Ping = 8;
        public const byte Pong = 9;
    }
    public delegate void SendTo(byte[] data, EndPoint endPoint);
    public abstract class Listener
    {
        public bool IsRunning = false;

        private Socket socket;
        private EndPoint sender;

        private byte[] buffer;
        private object locker = new object();
        
        public bool Poll => IsRunning ? socket.Poll(500000, SelectMode.SelectRead) : false; //500000 => 0.5s
        public int Available => IsRunning ? socket.Available : 0;

        protected void StartListener(ushort port = 0)
        {
            lock (locker)
            {
                if (!IsRunning)
                {
                    sender = new IPEndPoint(IPAddress.Any, 0);
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    socket.Bind(new IPEndPoint(IPAddress.Any, port));

                    IsRunning = true;
                    buffer = new byte[2048];     
                }
            }
        }

        protected void StopListener()
        {
            lock (locker)
            {
                if (IsRunning)
                {
                    IsRunning = false;
                    socket.Close();
                }
            }
        }
        
        public void Receive()
        {
            try
            {
                if (IsRunning)
                {
                    int size = socket.ReceiveFrom(buffer, ref sender);
                    RawHandler(buffer, size, sender);
                }
            } 
            catch (Exception) { return; }
        }

        public void ReceiveAll()
        {        
            try
            {
                if (IsRunning)
                {
                    for (int i = 0; i < socket.Available; i++)
                    {
                        int size = socket.ReceiveFrom(buffer, ref sender);
                        RawHandler(buffer, size, sender);
                    }
                }
            } 
            catch (Exception) { return; }
        }

        protected void SendTo(EndPoint endPoint, params byte[] data)
        {
            SendTo(data, data.Length, endPoint);
        }
        
        protected void SendTo(byte[] data, EndPoint endPoint)
        {
            SendTo(data, data.Length, endPoint);
        }

        protected void SendTo(byte[] data, int size, EndPoint endPoint)
        {
            if (IsRunning)
                socket?.SendTo(data, size, SocketFlags.None, endPoint);
        }
        
        protected abstract void RawHandler(byte[] data, int length, EndPoint endPoint);
    }
}