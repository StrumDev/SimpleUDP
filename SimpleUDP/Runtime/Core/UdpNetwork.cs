// This file is provided under The MIT License as part of SimpleUDP.
// Copyright (c) StrumDev
// For additional information please see the included LICENSE.md file or view it on GitHub:
// https://github.com/StrumDev/SimpleUDP/blob/main/LICENSE

using System;
using System.Net;
using SimpleUDP.Core.Net;
using System.Collections.Generic;

namespace SimpleUDP.Core
{
    internal static class Header
    {
        public const byte Ping = 1;
        public const byte Pong = 2;
        public const byte Connect = 3;
        public const byte Connected = 4;
        public const byte Disconnect = 5;
        public const byte Disconnected = 6;
        public const byte Reliable = 7;
        public const byte Unreliable = 8;
        public const byte ReliableAck = 9;
        public const byte Broadcast = 10;
        public const byte Unconnected = 11;
    }
    public class UdpNetwork : UdpListener
    {
        internal const byte IndexHeader = 0;
        internal const byte IndexAck = 1;
        internal const byte HeaderUnreliable = 1;
        internal const byte HeaderReliable = 2;

        public uint PeerTimeOut = 5000;
        public uint MaxConnections = 256;

        private object locker = new object();
        private Stack<UdpPeer> disconnectedPeers = new Stack<UdpPeer>();
        protected Dictionary<EndPoint, UdpPeer> Peers = new Dictionary<EndPoint, UdpPeer>();
        
        protected void CreateConnect(EndPoint endPoint, out UdpPeer udpPeer)
        {
            lock (locker)
            {
                if (!Peers.ContainsKey(endPoint))
                {   
                    udpPeer = new UdpPeer(SendTo)
                    {
                        TimeOut = PeerTimeOut,
                        OnConnected = OnPeerConnected,
                        OnDisconnected = disconnectedPeers.Push,
                    };

                    udpPeer.Connect(endPoint);
                    Peers.Add(endPoint, udpPeer);
                }
                else udpPeer = Peers[endPoint];
            }
        }

        protected void CreateDisconnect(EndPoint endPoint)
        {
            lock (locker)
            {
                if (Peers.TryGetValue(endPoint, out UdpPeer peer))
                    peer.Disconnect();    
            }
        }

        private void AcceptConnect(EndPoint endPoint)
        {
            lock (locker)
            {
                if (!Peers.ContainsKey(endPoint))
                {
                    if (Peers.Count >= MaxConnections)
                    {
                        SendTo(endPoint, Header.Disconnected);
                        return;
                    }
                }

                CreateConnect(endPoint, out UdpPeer peer);
                SendTo(endPoint, Header.Connected);    
            }
        }

        private void AcceptDiconnect(EndPoint endPoint)
        {
            lock (locker)
            {
                CreateDisconnect(endPoint);
                SendTo(endPoint, Header.Disconnected);   
            }
        }

        public void UpdatePeer(uint deltaTime)
        {
            lock (locker)
            {
                UpdateTimer(deltaTime);
                UpdateDisconnecting();   
            }
        }

        public void UpdateTimer(uint deltaTime)
        {
            lock (locker)
            {
                foreach (UdpPeer peer in Peers.Values)
                    peer.UpdateTimer(deltaTime);   
            }
        }

        public void UpdateDisconnecting()
        {
            lock (locker)
            {
                while (disconnectedPeers.Count != 0)
                    RemovePeer(disconnectedPeers.Pop());    
            }
        }

        protected void SendBroadcast(byte[] packet, int length, ushort port)
        {
            lock (locker)
            {
                if (EnableBroadcast && port != 0)
                {
                    byte[] buffer = new byte[length + HeaderUnreliable];
                    
                    buffer[IndexHeader] = Header.Broadcast;
                    Buffer.BlockCopy(packet, 0, buffer, HeaderUnreliable, length);

                    SendTo(new IPEndPoint(IPAddress.Broadcast, port), buffer);
                }
                else throw new Exception("It is not possible to send a Broadcast message if EnableBroadcast = false or Port = 0.");
            }
        }

        protected void SendUnconnected(byte[] packet, int length, EndPoint endPoint)
        {
            lock (locker)
            {
                byte[] buffer = new byte[length + HeaderUnreliable];
                
                buffer[IndexHeader] = Header.Unconnected;
                Buffer.BlockCopy(packet, 0, buffer, HeaderUnreliable, length);

                SendTo(endPoint, buffer);  
            }
        }
        
        private protected override void OnRawHandler(byte[] data, int length, EndPoint endPoint)
        {
            switch (data[IndexHeader])
            {
                case Header.Unreliable:
                    HandlerUnreliable(data, length, endPoint);
                    return;
                case Header.Reliable:
                    HandlerReliable(data, length, endPoint);
                    return;
                case Header.ReliableAck:
                    HandlerReliableAck(data, length, endPoint);
                    return;
                case Header.Ping:
                    SendTo(endPoint, Header.Pong);
                    return;
                case Header.Pong:
                    HandlerPong(endPoint);
                    return;
                case Header.Connect:
                    AcceptConnect(endPoint);
                    return;
                case Header.Connected:
                    HandlerConnected(endPoint);
                    return;
                case Header.Disconnect:
                    AcceptDiconnect(endPoint);
                    return;
                case Header.Disconnected:
                    HandlerDiconnected(endPoint);
                    return;
                case Header.Broadcast:
                    HandlerBroadcast(data, length, endPoint);
                    return;
                case Header.Unconnected:
                    HandlerUnconnected(data, length, endPoint);
                return;
            }
        }
        
        private void HandlerReliable(byte[] data, int length, EndPoint endPoint)
        {
            lock (locker)
            {
                if (Peers.TryGetValue(endPoint, out UdpPeer peer))
                {
                    if (peer.IsNewAck(data))
                    {
                        byte[] buffer = new byte[length - HeaderReliable];
                        Buffer.BlockCopy(data, HeaderReliable, buffer, 0, length - HeaderReliable);
                        OnHandlerReliable(buffer, peer);
                    }
                }                
            }
        }

        private void HandlerUnreliable(byte[] data, int length, EndPoint endPoint)
        {
            lock (locker)
            {
                if (Peers.TryGetValue(endPoint, out UdpPeer peer))
                {
                    byte[] buffer = new byte[length - HeaderUnreliable];
                    Buffer.BlockCopy(data, HeaderUnreliable, buffer, 0, length - HeaderUnreliable);
                    OnHandlerUnreliable(buffer, peer);
                }
            }
        }

        private void HandlerReliableAck(byte[] data, int length, EndPoint endPoint)
        {
            lock (locker)
            {
                if (Peers.TryGetValue(endPoint, out UdpPeer peer))
                    peer.ClearAck(data);
            }
        }

        private void HandlerBroadcast(byte[] data, int length, EndPoint endPoint)
        {
            lock (locker)
            {
                byte[] buffer = new byte[length - HeaderUnreliable];
                Buffer.BlockCopy(data, HeaderUnreliable, buffer, 0, length - HeaderUnreliable);
                OnHandlerBroadcast(buffer, endPoint);
            }
        }

        private void HandlerUnconnected(byte[] data, int length, EndPoint endPoint)
        {
            lock (locker)
            {
                byte[] buffer = new byte[length - HeaderUnreliable];
                Buffer.BlockCopy(data, HeaderUnreliable, buffer, 0, length - HeaderUnreliable);
                OnHandlerUnconnected(buffer, endPoint);
            }
        }

        private void HandlerPong(EndPoint endPoint)
        {
            lock (locker)
            {
                if (Peers.TryGetValue(endPoint, out UdpPeer peer))
                    peer.HandlerPong();    
            }
        }

        private void HandlerConnected(EndPoint endPoint)
        {
            lock (locker)
            {
                if (Peers.TryGetValue(endPoint, out UdpPeer peer))
                    peer.Connected();    
            }
        }

        private void HandlerDiconnected(EndPoint endPoint)
        {
            lock (locker)
            {
                if (Peers.TryGetValue(endPoint, out UdpPeer peer))
                    disconnectedPeers.Push(peer);    
            }
        }
        
        private void RemovePeer(UdpPeer udpPeer)
        {
            lock (locker)
            {
                if (Peers.TryGetValue(udpPeer.EndPoint, out UdpPeer peer))
                {
                    peer.Disconnected();
                    Peers.Remove(udpPeer.EndPoint);
                    OnPeerDisconnected(peer);
                }   
            }
        }

        protected override void OnStopped()
        {
            Peers.Clear();
            disconnectedPeers.Clear();
            
            base.OnStopped();
        }

        protected virtual void OnPeerConnected(UdpPeer peer) { }
        protected virtual void OnPeerDisconnected(UdpPeer peer) { }
        protected virtual void OnHandlerReliable(byte[] packet, UdpPeer peer) { }
        protected virtual void OnHandlerUnreliable(byte[] packet, UdpPeer peer) { }
        protected virtual void OnHandlerBroadcast(byte[] packet, EndPoint endPoint) { }
        protected virtual void OnHandlerUnconnected(byte[] packet, EndPoint endPoint) { }
    }
}