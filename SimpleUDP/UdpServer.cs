using System;
using System.Net;
using SimpleUDP.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SimpleUDP
{
    public class UdpServer : UdpListener
    {
        public Action<UdpPeer> OnConnected;
        public Action<UdpPeer> OnDisconnected;

        public Action<UdpPeer, byte[]> OnReceiveReliable;
        public Action<UdpPeer, byte[]> OnReceiveUnreliable;

        public Action<EndPoint, byte[]> OnReceiveBroadcast;
        public Action<EndPoint, byte[]> OnReceiveUnconnected;

        public uint TimeOut = 5000;
        public uint MaxConnections = 256;
        public string KeyConnection = "";

        public ReadOnlyDictionary<uint, UdpPeer> Connections;
        public uint ConnectionsCount => (uint)connections.Count;
        
        private uint sequenceNumberId;
        
        private Stack<UdpPeer> disconnectedPeers;
        private Dictionary<EndPoint, UdpPeer> peers;
        private Dictionary<uint, UdpPeer> connections;

        public UdpServer()
        {    
            disconnectedPeers = new Stack<UdpPeer>();

            peers = new Dictionary<EndPoint, UdpPeer>();
            connections = new Dictionary<uint, UdpPeer>();

            Connections = new ReadOnlyDictionary<uint, UdpPeer>(connections);
        }

        protected override void OnListenerStarted()
        {
            AdjustBufferSizes(ServerKilobytes);
            base.OnListenerStarted();
        }

        public void Disconnect(uint peerId)
        {
            if (Connections.TryGetValue(peerId, out UdpPeer peer))
                peer.SendDisconnect(Reason.Kicked);
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
            if (Connections.TryGetValue(peerId, out UdpPeer peer))
                peer.SendReliable(packet, length, offset);   
        }

        public void SendUnreliable(uint peerId, byte[] packet, int length, int offset = 0)
        {
            if (Connections.TryGetValue(peerId, out UdpPeer peer))
                peer.SendUnreliable(packet, length, offset);   
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
            lock (locker)
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
            lock (locker)
            {
                byte[] buffer = new byte[length + UdpIndex.Unreliable];
            
                buffer[UdpIndex.Header] = UdpHeader.Unreliable;
                Buffer.BlockCopy(packet, offset, buffer, UdpIndex.Unreliable, length);
            
                foreach (UdpPeer peer in Connections.Values)
                {
                    if (peer.Id != ignoreId)
                        peer.RawSend(buffer);
                }
            } 
        }

        protected sealed override void OnTickUpdate(uint deltaTime)
        {
            if (IsRunning)
            {
                UpdateTimer(deltaTime);
                UpdateDisconnecting();
            }
        }

        public void UpdateTimer(uint deltaTime)
        {
            lock (locker)
            {
                if (IsRunning)
                {
                    foreach (UdpPeer peer in peers.Values)
                        peer.UpdateTimer(deltaTime);    
                }
            }
        }

        public void UpdateDisconnecting()
        {
            lock (locker)
            {
                if (IsRunning)
                {
                    while (disconnectedPeers.Count != 0)
                        RemovePeer(disconnectedPeers.Pop());     
                }
            }
        }

        private void AcceptPing(EndPoint endPoint)
        {
            if (peers.TryGetValue(endPoint, out UdpPeer peer))
                SendTo(endPoint, UdpHeader.Pong);
            else
                SendTo(endPoint, UdpHeader.Disconnected, 0);
        }

        private void AcceptConnect(byte[] data, int length, EndPoint endPoint)
        {
            lock (locker)
            {
                if (!UdpPeer.KeyMatching(KeyConnection, data, length, UdpPeer.HeaderSize))
                {
                    SendTo(endPoint, UdpHeader.Disconnected, (byte)Reason.IncorrectKey);
                    return;
                }

                if (!peers.ContainsKey(endPoint))
                {
                    if (peers.Count >= MaxConnections)
                    {
                        SendTo(endPoint, UdpHeader.Disconnected, (byte)Reason.ServerIsFull);
                        return;
                    }

                    UdpPeer udpPeer = new UdpPeer(SendTo)
                    {
                        TimeOut = TimeOut,
                        OnLostConnection = disconnectedPeers.Push,
                    };

                    peers.Add(endPoint, udpPeer);
                    udpPeer.SendConnect(endPoint, GetNewId(), KeyConnection);
                }
            }
        }

        private void AcceptDisconnect(byte[] data, int length, EndPoint endPoint)
        {
            byte reason = data[UdpIndex.Reason];

            HandlerDiconnected(data, length, endPoint);
            SendTo(endPoint, UdpHeader.Disconnected, reason);
        }

        public void SendBroadcast(ushort port, byte[] packet, int length)
        {
            if (IsRunning && EnableBroadcast && port != 0)
            {
                byte[] buffer = new byte[length + UdpIndex.Unreliable];
                
                buffer[UdpIndex.Header] = UdpHeader.Broadcast;
                Buffer.BlockCopy(packet, 0, buffer, UdpIndex.Unreliable, length);

                SendTo(new IPEndPoint(IPAddress.Broadcast, port), buffer);
            }
            else throw new Exception("It is not possible to send a Broadcast message if EnableBroadcast = false or Port = 0.");
        }

        public void SendUnconnected(EndPoint endPoint, byte[] packet, int length)
        {
            if (IsRunning)
            {
                byte[] buffer = new byte[length + UdpIndex.Unreliable];
                
                buffer[UdpIndex.Header] = UdpHeader.Unconnected;
                Buffer.BlockCopy(packet, 0, buffer, UdpIndex.Unreliable, length);

                SendTo(endPoint, buffer);                      
            }
        }
        
        protected sealed override void OnRawHandler(byte[] data, int length, EndPoint endPoint)
        {
            switch (data[UdpIndex.Header])
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
                    AcceptConnect(data, length, endPoint);
                    return;
                case UdpHeader.Connected:
                    HandlerConnected(endPoint);
                    return;
                case UdpHeader.Disconnect:
                    AcceptDisconnect(data, length, endPoint);
                    return;
                case UdpHeader.Disconnected:
                    HandlerDiconnected(data, length, endPoint);
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
            if (peers.TryGetValue(endPoint, out UdpPeer peer))
                peer.HandlerPong();
        }

        private void HandlerConnected(EndPoint endPoint)
        {
            lock (locker)
            {
                if (peers.TryGetValue(endPoint, out UdpPeer peer))
                {
                    if (Connections.ContainsKey(peer.Id))
                        return;

                    peer.SetConnected(peer.Id);
                    peer.OnLostConnection = LostConnection;

                    connections.Add(peer.Id, peer);
                    OnPeerConnected(peer);
                }                
            }
        }

        private void HandlerDiconnected(byte[] data, int length, EndPoint endPoint)
        {
            lock (locker)
            {
                if (peers.TryGetValue(endPoint, out UdpPeer peer))
                {
                    if (peer.State != State.NoConnect)
                    {
                        peer.ReasonDisconnection = (Reason)data[UdpIndex.Reason];
                        
                        peer.SetDisconnected();
                        LostConnection(peer);
                    }
                }
            }
        }

        private void HandlerReliable(byte[] data, int length, EndPoint endPoint)
        {
            if (peers.TryGetValue(endPoint, out UdpPeer peer))
            {
                if (peer.IsNewAck(data))
                {
                    byte[] buffer = CopyPacket(data, length, UdpIndex.Reliable);
                    OnPeerReceiveReliable(peer, buffer);
                }  
            }                
        }

        private void HandlerUnreliable(byte[] data, int length, EndPoint endPoint)
        {
            if (peers.TryGetValue(endPoint, out UdpPeer peer))
            {
                byte[] buffer = CopyPacket(data, length, UdpIndex.Unreliable);
                OnPeerReceiveUnreliable(peer, buffer);
            }
        }

        private void HandlerReliableAck(byte[] data, int length, EndPoint endPoint)
        {
            if (peers.TryGetValue(endPoint, out UdpPeer peer))
                peer.ClearAck(data);
        }

        private void HandlerBroadcast(byte[] data, int length, EndPoint endPoint)
        {
            byte[] buffer = new byte[length - UdpIndex.Unreliable];
            Buffer.BlockCopy(data, UdpIndex.Unreliable, buffer, 0, length - UdpIndex.Unreliable);

            OnReceiveBroadcast?.Invoke(endPoint, buffer);
        }

        private void HandlerUnconnected(byte[] data, int length, EndPoint endPoint)
        {
            byte[] buffer = new byte[length - UdpIndex.Unreliable];
            Buffer.BlockCopy(data, UdpIndex.Unreliable, buffer, 0, length - UdpIndex.Unreliable);

            OnReceiveUnconnected?.Invoke(endPoint, buffer);
        }

        private void LostConnection(UdpPeer udpPeer)
        {
            disconnectedPeers.Push(udpPeer);
            
            connections.Remove(udpPeer.Id);
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

        private uint GetNewId()
        {
            uint newId = ++sequenceNumberId;

            while (connections.ContainsKey(newId)) // (;
                newId = ++sequenceNumberId; 

            return newId;
        }

        private byte[] CopyPacket(byte[] data, int length, int offset)
        {
            byte[] buffer = new byte[length - offset];
            Buffer.BlockCopy(data, offset, buffer, 0, length - offset);
            
            return buffer;
        }

        protected override void OnListenerStopped()
        {
            lock (locker)
            {
                foreach (UdpPeer peer in peers.Values)
                    peer.SetDisconnected();
                    
                peers.Clear();
                connections.Clear();
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