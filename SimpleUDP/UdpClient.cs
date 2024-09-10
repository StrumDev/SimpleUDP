using System;
using System.Net;
using SimpleUDP.Core;

namespace SimpleUDP
{
    public class UdpClient : UdpListener
    {
        public Action OnConnected;
        public Action OnDisconnected;

        public Action<byte[]> OnReceiveReliable;
        public Action<byte[]> OnReceiveUnreliable;

        public Action<EndPoint, byte[]> OnReceiveBroadcast;
        public Action<EndPoint, byte[]> OnReceiveUnconnected;

        public uint TimeOut = 5000;
        
        public uint Id => udpPeer.Id;
        public uint Rtt => udpPeer.Rtt;
        public State State => udpPeer.State;
        public EndPoint EndPoint => udpPeer.EndPoint;

        private UdpPeer udpPeer;
        private EndPoint remoteEndPoint;

        private object locker = new object();

        public UdpClient()
        {
            udpPeer = new UdpPeer(SendTo)
            {
                TimeOut = TimeOut,
                OnLostConnection = LostConnection,
            };
        }

        public void Connect(string address, ushort port)
        {
            SetIPEndPoint(address, port);
            udpPeer.SendConnect(remoteEndPoint);
        }

        public void Disconnect()
        {
            udpPeer.Disconnect();
        }

        public void QuietDisconnect()
        {
            udpPeer.QuietDisconnect();
        }
        
        public void SendReliable(byte[] packet)
        {
            udpPeer.SendReliable(packet, packet.Length);
        }

        public void SendUnreliable(byte[] packet)
        {
            udpPeer.SendUnreliable(packet, packet.Length);
        }

        public void SendReliable(byte[] packet, int length, int offset = 0)
        {
            udpPeer.SendReliable(packet, length, offset);
        }

        public void SendUnreliable(byte[] packet, int length, int offset = 0)
        {
            udpPeer.SendUnreliable(packet, length, offset);
        }

        protected sealed override void OnTickUpdate(uint deltaTime)
        {
            lock (locker)
            {
                UpdateTimer(deltaTime);
            }
        }

        public void UpdateTimer(uint deltaTime)
        {
            lock (locker)
            {
                if (IsRunning)
                    udpPeer.UpdateTimer(deltaTime);   
            }
        }

        private void AcceptConnect(byte[] data, int length, EndPoint endPoint)
        {
            lock (locker)
            {
                if (!remoteEndPoint.Equals(endPoint))
                {
                    SendTo(endPoint, UdpHeader.Disconnected);
                    return;
                }

                if (udpPeer.State == State.Connecting)
                {
                    udpPeer.SetConnected(BitConverter.ToUInt32(data, HeaderUnreliable));
                    OnPeerConnected();
                }
                
                SendTo(endPoint, UdpHeader.Connected);
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

        private void AcceptPing(EndPoint endPoint)
        {
            lock (locker)
            {
                if (udpPeer.State == State.Connected)
                    SendTo(endPoint, UdpHeader.Pong);
                else 
                    SendTo(endPoint, UdpHeader.Disconnected);
            }
        }

        public void SendBroadcast(ushort port, byte[] packet, int length)
        {
            lock (locker)
            {
                if (IsRunning && EnableBroadcast && port != 0)
                {
                    byte[] buffer = new byte[length + HeaderUnreliable];
                    
                    buffer[IndexHeader] = UdpHeader.Broadcast;
                    Buffer.BlockCopy(packet, 0, buffer, HeaderUnreliable, length);

                    SendTo(new IPEndPoint(IPAddress.Broadcast, port), buffer);
                }
                else throw new Exception("It is not possible to send a Broadcast message if EnableBroadcast = false or Port = 0.");
            }
        }

        public void SendUnconnected(EndPoint endPoint, byte[] packet, int length)
        {
            lock (locker)
            {
                if (IsRunning)
                {
                    byte[] buffer = new byte[length + HeaderUnreliable];
                    
                    buffer[IndexHeader] = UdpHeader.Unconnected;
                    Buffer.BlockCopy(packet, 0, buffer, HeaderUnreliable, length);

                    SendTo(endPoint, buffer);                      
                }
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
                    udpPeer.HandlerPong();
                    return;
                case UdpHeader.Connect:
                    AcceptConnect(data, length, endPoint);
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

        private void HandlerDiconnected(EndPoint endPoint)
        {
            lock (locker)
            {
                if (udpPeer.State != State.NoConnect)
                    LostConnection(udpPeer);
            }
        }

        private void HandlerReliable(byte[] data, int length, EndPoint endPoint)
        {
            lock (locker)
            {
                if (udpPeer.IsNewAck(data))
                {
                    byte[] buffer = CopyPacket(data, length, HeaderReliable);
                    OnPeerReceiveReliable(buffer);
                }           
            }
        }

        private void HandlerUnreliable(byte[] data, int length, EndPoint endPoint)
        {
            lock (locker)
            {
                byte[] buffer = CopyPacket(data, length, HeaderUnreliable);
                OnPeerReceiveUnreliable(buffer);
            }
        }

        private void HandlerReliableAck(byte[] data, int length, EndPoint endPoint)
        {
            lock (locker)
                udpPeer.ClearAck(data);
        }

        private void HandlerBroadcast(byte[] data, int length, EndPoint endPoint)
        {
            lock (locker)
            {
                byte[] buffer = new byte[length - HeaderUnreliable];
                Buffer.BlockCopy(data, HeaderUnreliable, buffer, 0, length - HeaderUnreliable);
                OnReceiveBroadcast?.Invoke(endPoint, buffer);
            }
        }

        private void HandlerUnconnected(byte[] data, int length, EndPoint endPoint)
        {
            lock (locker)
            {
                byte[] buffer = new byte[length - HeaderUnreliable];
                Buffer.BlockCopy(data, HeaderUnreliable, buffer, 0, length - HeaderUnreliable);
                OnReceiveUnconnected?.Invoke(endPoint, buffer);
            }
        }

        private void LostConnection(UdpPeer udpPeer)
        {
            udpPeer.SetDisconnected();
            OnPeerDisconnected();
        }

        private void SetIPEndPoint(string host, ushort port)
        {
            IPAddress address = Dns.GetHostAddresses(host)[0];

            remoteEndPoint = new IPEndPoint(address, port);
        }

        private byte[] CopyPacket(byte[] data, int length, int offset)
        {
            byte[] buffer = new byte[length - offset];
            Buffer.BlockCopy(data, offset, buffer, 0, length - offset);
            
            return buffer;
        }

        protected override void OnListenerStopped()
        {
            udpPeer.SetDisconnected();
            base.OnListenerStopped();
        }

        protected virtual void OnPeerConnected()
        {
            OnConnected?.Invoke();
        }

        protected virtual void OnPeerDisconnected()
        {
            OnDisconnected?.Invoke();
        }

        protected virtual void OnPeerReceiveReliable(byte[] packet)
        {
            OnReceiveReliable?.Invoke(packet);
        }

        protected virtual void OnPeerReceiveUnreliable(byte[] packet)
        {
            OnReceiveUnreliable?.Invoke(packet);
        }
    }
}