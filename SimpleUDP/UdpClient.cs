using System;
using System.Net;
using SimpleUDP.Core;
using SimpleUDP.Utils;

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
        public string KeyConnection = "";
        
        public uint Id => udpPeer.Id;
        public uint Rtt => udpPeer.Rtt;
        public State State => udpPeer.State;
        public EndPoint EndPoint => udpPeer.EndPoint;

        public Reason ReasonDisconnection => udpPeer.ReasonDisconnection;

        private UdpPeer udpPeer;
        private EndPoint remoteEndPoint;

        private const int ClientKilobytes = 128;

        public UdpClient()
        {
            udpPeer = new UdpPeer(SendTo)
            {
                TimeOut = TimeOut,
                OnLostConnection = LostConnection,
            };
        }

        protected override void OnListenerStarted()
        {
            AdjustBufferSizes(ClientKilobytes);

            UdpLog.Info($"[Client] Started on port: {LocalPort}");
            base.OnListenerStarted();
        }

        public void Connect(string address, ushort port)
        {
            if (udpPeer.State != State.NoConnect)
                return;
            
            SetIPEndPoint(address, port);
            udpPeer.SendConnect(remoteEndPoint, Id, KeyConnection);

            UdpLog.Info($"[Client] Connection to: {EndPoint}");
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
            UpdateTimer(deltaTime);
        }

        public void UpdateTimer(uint deltaTime)
        {
            if (IsRunning)
                udpPeer.UpdateTimer(deltaTime);   
        }

        private void AcceptConnect(byte[] data, int length, EndPoint endPoint)
        {
            if (!UdpPeer.KeyMatching(KeyConnection, data, length, UdpPeer.HeaderSize))
            {
                SendTo(endPoint, UdpHeader.Disconnected, (byte)Reason.IncorrectKey);
                return;
            }

            if (!remoteEndPoint.Equals(endPoint))
            {
                SendTo(endPoint, UdpHeader.Disconnected, 0);
                return;
            }

            if (udpPeer.State == State.Connecting)
            {
                udpPeer.SetConnected(UdpConverter.GetUInt(data, UdpIndex.Unreliable));
                
                UdpLog.Info($"[Client] Connected successfully!");
                OnPeerConnected();
            }
            
            SendTo(endPoint, UdpHeader.Connected);
        }

        private void AcceptDisconnect(byte[] data, int length, EndPoint endPoint)
        {
            byte reason = data[UdpIndex.Reason];

            HandlerDiconnected(data, length, endPoint);
            SendTo(endPoint, UdpHeader.Disconnected, reason);
        }

        private void AcceptPing(EndPoint endPoint)
        {
            if (udpPeer.State == State.Connected)
                SendTo(endPoint, UdpHeader.Pong);
            else 
                SendTo(endPoint, UdpHeader.Disconnected, 0);
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
                    udpPeer.HandlerPong();
                    return;
                case UdpHeader.Connect:
                    AcceptConnect(data, length, endPoint);
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

        private void HandlerDiconnected(byte[] data, int length, EndPoint endPoint)
        {
            if (udpPeer.State != State.NoConnect)
            {
                udpPeer.ReasonDisconnection = (Reason)data[UdpIndex.Reason];
                LostConnection(udpPeer);
            }
        }

        private void HandlerReliable(byte[] data, int length, EndPoint endPoint)
        {
            if (udpPeer.IsNewAck(data))
            {
                byte[] buffer = CopyPacket(data, length, UdpIndex.Reliable);
                OnPeerReceiveReliable(buffer);
            }           
        }

        private void HandlerUnreliable(byte[] data, int length, EndPoint endPoint)
        {
            byte[] buffer = CopyPacket(data, length, UdpIndex.Unreliable);
            OnPeerReceiveUnreliable(buffer);
        }

        private void HandlerReliableAck(byte[] data, int length, EndPoint endPoint)
        {
            udpPeer.ClearAck(data);
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
            udpPeer.SetDisconnected();

            UdpLog.Info($"[Client] Disconnected from: {EndPoint}");
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

            UdpLog.Info($"[Client] Stopped!");
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