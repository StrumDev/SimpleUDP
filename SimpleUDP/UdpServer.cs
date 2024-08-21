using System;
using System.Net;
using SimpleUDP.Core;
using System.Collections.Generic;

namespace SimpleUDP
{
    public class UdpServer : UdpListener
    {
        public Action<UdpPeer> OnConnected;
        public Action<UdpPeer> OnDisconnected;

        public Action<UdpPeer, byte[]> OnReceiveReliable;
        public Action<UdpPeer, byte[]> OnReceiveUnreliable;

        public uint TimeOut = 5000;
        public uint MaxConnections = 256;

        public uint ConnectionsCount => (uint)Connections.Count;

        public Dictionary<uint, UdpPeer> Connections;

        private object locker = new object();
        private Stack<UdpPeer> disconnectedPeers;
        private Dictionary<EndPoint, UdpPeer> peers;

        public UdpServer()
        {
            disconnectedPeers = new Stack<UdpPeer>();
            peers = new Dictionary<EndPoint, UdpPeer>();
            Connections = new Dictionary<uint, UdpPeer>();
        }

        public void Disconnect(uint peerId)
        {
            if (Connections.TryGetValue(peerId, out UdpPeer peer))
                peer.Disconnect();
        }

        public void SendReliable(uint peerId, byte[] packet)
        {
            SendReliable(peerId, packet, packet.Length);
        }

        public void SendUnreliable(uint peerId, byte[] packet)
        {
            SendUnreliable(peerId, packet, packet.Length);
        }

        public void SendReliable(uint peerId, byte[] packet, int length, int offset = 0)
        {
            lock (Connections)
            {
                if (Connections.TryGetValue(peerId, out UdpPeer peer))
                    peer.SendReliable(packet, length, offset);   
            }
        }

        public void SendUnreliable(uint peerId, byte[] packet, int length, int offset = 0)
        {
            lock (Connections)
            {
                if (Connections.TryGetValue(peerId, out UdpPeer peer))
                    peer.SendUnreliable(packet, length, offset);   
            }
        }

        public void SendAllReliable(byte[] packet, uint ignoreId = 0)
        {
            SendAllReliable(packet, packet.Length, 0, ignoreId);
        }

        public void SendAllUnreliable(byte[] packet, uint ignoreId = 0)
        {
            SendAllUnreliable(packet, packet.Length, 0, ignoreId);
        }

        public void SendAllReliable(byte[] packet, int length, uint ignoreId = 0)
        {
            SendAllReliable(packet, length, 0, ignoreId);
        }

        public void SendAllUnreliable(byte[] packet, int length, uint ignoreId = 0)
        {
            SendAllUnreliable(packet, length, 0, ignoreId);
        }

        public void SendAllReliable(byte[] packet, int length, int offset, uint ignoreId = 0)
        {
            lock (Connections)
            {
                foreach (UdpPeer peer in Connections.Values)
                {
                    if (peer.Id != ignoreId)
                        peer.SendReliable(packet, length, offset);
                }    
            }
        }

        public void SendAllUnreliable(byte[] packet, int length, int offset, uint ignoreId = 0)
        {
            lock (Connections)
            {
                foreach (UdpPeer peer in Connections.Values)
                {
                    if (peer.Id != ignoreId)
                        peer.SendUnreliable(packet, length, offset);
                }    
            }
        }

        protected sealed override void OnTickUpdate(uint deltaTime)
        {
            lock (locker)
            {
                if (IsRunning)
                {
                    UpdateTimer(deltaTime);
                    UpdateDisconnecting();
                }
            }
        }

        public void UpdateTimer(uint deltaTime)
        {
            lock (locker)
            {
                foreach (UdpPeer peer in peers.Values)
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

        private void AcceptPing(EndPoint endPoint)
        {
            lock (locker)
            {
                if (peers.TryGetValue(endPoint, out UdpPeer peer))
                    SendTo(endPoint, UdpHeader.Pong);
                else
                    SendTo(endPoint, UdpHeader.Disconnected);
            }
        }

        private void AcceptConnect(EndPoint endPoint)
        {
            lock (locker)
            {
                if (!peers.ContainsKey(endPoint))
                {
                    if (peers.Count >= MaxConnections)
                    {
                        SendTo(endPoint, UdpHeader.Disconnected);
                        return;
                    }

                    UdpPeer udpPeer = new UdpPeer(SendTo)
                    {
                        TimeOut = TimeOut,
                        OnLostConnection = disconnectedPeers.Push,
                    };
                    
                    udpPeer.SendConnect(endPoint);
                    peers.Add(endPoint, udpPeer);
                }
            }
        }

        private void AcceptDisconnect(EndPoint endPoint)
        {
            lock (locker)
            {
                HandlerDiconnected(endPoint);
                SendTo(endPoint, UdpHeader.Disconnected);
            }
        }
        
        protected sealed override void OnRawHandler(byte[] data, int length, EndPoint endPoint)
        {
            switch (data[IndexHeader])
            {
                case UdpHeader.Unreliable:
                    HandlerUnreliable(data, length, endPoint);
                    return;
                case UdpHeader.Reliable:
                    HandlerReliable(data, length, endPoint);
                    return;
                case UdpHeader.ReliableAck:
                    HandlerReliableAck(data, length, endPoint);
                    return;
                case UdpHeader.Ping:
                    AcceptPing(endPoint);
                    return;
                case UdpHeader.Pong:
                    HandlerPong(endPoint);
                    return;
                case UdpHeader.Connect:
                    AcceptConnect(endPoint);
                    return;
                case UdpHeader.Connected:
                    HandlerConnected(endPoint);
                    return;
                case UdpHeader.Disconnect:
                    AcceptDisconnect(endPoint);
                    return;
                case UdpHeader.Disconnected:
                    HandlerDiconnected(endPoint);
                    return;
                case UdpHeader.Broadcast:
                    HandlerBroadcast(data, length, endPoint);
                    return;
                case UdpHeader.Unconnected:
                    HandlerUnconnected(data, length, endPoint);
                return;
            }
        }

        private void HandlerPong(EndPoint endPoint)
        {
            lock (locker)
            {
                if (peers.TryGetValue(endPoint, out UdpPeer peer))
                    peer.HandlerPong();
            }
        }

        private void HandlerConnected(EndPoint endPoint)
        {
            if (peers.TryGetValue(endPoint, out UdpPeer peer))
            {
                peer.SetConnected(peer.Id);
                peer.OnLostConnection = LostConnection;

                Connections.Add(peer.Id, peer);
                OnPeerConnected(peer);
            }
        }

        private void HandlerDiconnected(EndPoint endPoint)
        {
            lock (locker)
            {
                if (peers.TryGetValue(endPoint, out UdpPeer peer))
                {
                    if (peer.State != State.NoConnect)
                    {
                        peer.SetDisconnected();
                        LostConnection(peer);
                    }
                }
            }
        }

        private void HandlerReliable(byte[] data, int length, EndPoint endPoint)
        {
            lock (locker)
            {
                if (peers.TryGetValue(endPoint, out UdpPeer peer))
                {
                    if (peer.IsNewAck(data))
                    {
                        byte[] buffer = CopyPacket(data, length, HeaderReliable);
                        OnPeerReceiveReliable(peer, buffer);
                    }  
                }                
            }
        }

        private void HandlerUnreliable(byte[] data, int length, EndPoint endPoint)
        {
            lock (locker)
            {
                if (peers.TryGetValue(endPoint, out UdpPeer peer))
                {
                    byte[] buffer = CopyPacket(data, length, HeaderUnreliable);
                    OnPeerReceiveUnreliable(peer, buffer);
                }
            }
        }

        private void HandlerReliableAck(byte[] data, int length, EndPoint endPoint)
        {
            lock (locker)
            {
                if (peers.TryGetValue(endPoint, out UdpPeer peer))
                    peer.ClearAck(data);
            }
        }

        private void LostConnection(UdpPeer udpPeer)
        {
            disconnectedPeers.Push(udpPeer);
            
            Connections.Remove(udpPeer.Id);
            OnPeerDisconnected(udpPeer);
        }
        
        private void RemovePeer(UdpPeer udpPeer)
        {
            lock (locker)
            {
                if (peers.ContainsKey(udpPeer.EndPoint))
                    peers.Remove(udpPeer.EndPoint);
            }
        }

        protected override void OnListenerStopped()
        {
            lock (locker)
            {
                foreach (UdpPeer peer in peers.Values)
                    peer.SetDisconnected();
                    
                peers.Clear();
                Connections.Clear();
                disconnectedPeers.Clear();
                
                base.OnListenerStopped();
            }
        }

        protected virtual void OnPeerConnected(UdpPeer peer)
        {
            OnConnected?.Invoke(peer);
        }

        protected virtual void OnPeerDisconnected(UdpPeer peer)
        {
            OnDisconnected?.Invoke(peer);
        }

        protected virtual void OnPeerReceiveReliable(UdpPeer peer, byte[] packet)
        {
            OnReceiveReliable?.Invoke(peer, packet);
        }

        protected virtual void OnPeerReceiveUnreliable(UdpPeer peer, byte[] packet)
        {
            OnReceiveUnreliable?.Invoke(peer, packet);
        }
    }    
}