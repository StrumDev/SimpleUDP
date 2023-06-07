using System;
using System.Net;
using System.Collections.Generic;
using Timer = System.Timers.Timer;

namespace SimpleUDP
{
    public class Server : Listener
    {
        public Action<uint> OnClientConnected;
        public Action<uint> OnClientDisconnected;
        public Action<uint, byte[], bool> OnHandler;

        public uint MaxTimeOut = 10000; //10s

        private const uint CHECK_TIME_OUT = 1000; // 1s
        private Dictionary<EndPoint, Reliable> clientsEp = new Dictionary<EndPoint, Reliable>();
        private Dictionary<uint, Reliable> clientsId = new Dictionary<uint, Reliable>();
        private List<EndPoint> timeOut = new List<EndPoint>();
        private Timer timer;

        public Server()
        {
            timer = new Timer(CHECK_TIME_OUT);
            timer.Elapsed += (o, e) => UpdateTimeOut();
            timer.AutoReset = true;

            IsRunning = false;
        }

        public void Start(ushort port)
        {
            if (IsRunning)
                return;
            
            StartListener(port);

            timer.Start();
            Log.Info($"[Server] Start: {port}");
        }

        public void Send(uint clientId, bool channel, byte[] data, int length)
        {
            if (!IsRunning)
                return;
                
            if (channel) clientsId[clientId].SendReliable(data, length);
            else clientsId[clientId].SendUnreliable(data, length);  
        }

        public void SendAll(bool channel, byte[] data, int length)
        {
            if (!IsRunning)
                return;

            foreach (var reliable in clientsEp.Values)
            {
                if (channel) reliable.SendReliable(data, length);
                else reliable.SendUnreliable(data, length);
            }    
        }

        public void SendAll(uint skipClientId, bool channel, byte[] data, int length)
        {
            if (!IsRunning)
                return;

            foreach (var reliable in clientsId.Values)
            {
                if (reliable.Id == skipClientId)
                    continue;

                if (channel) reliable.SendReliable(data, length);
                else reliable.SendUnreliable(data, length);
            }    
        }

        public void Disconnect(uint clientId)
        {
            if (!IsRunning)
                return;
            
            ClientDisconnected(clientsId[clientId].EndPoint);
        }

        public EndPoint GetClientEndPoint(uint clientId) => clientsId[clientId].EndPoint;

        private void UpdateTimeOut()
        {
            lock (clientsEp)
            {
                foreach (var client in clientsEp)
                    if ((DateTime.UtcNow - client.Value.DateTime).TotalMilliseconds > MaxTimeOut)
                        timeOut.Add(client.Key);

                foreach (EndPoint endPoint in timeOut)
                    ClientDisconnected(endPoint);
                
                timeOut.Clear();
            }
        }

        private bool TryGetClient(byte header, EndPoint endPoint)
        {
            switch (header)
            {
                case Header.Ping:
                    SendTo(endPoint, Header.Ping);
                    if (clientsEp.ContainsKey(endPoint)) clientsEp[endPoint].DateTime = DateTime.UtcNow;
                    return false;
                case Header.Connect:
                    lock (clientsEp) ClientConnected(endPoint);
                    return false;
                case Header.Disconnect:
                    lock (clientsEp) ClientDisconnected(endPoint);
                return false;
            }
            return clientsEp.ContainsKey(endPoint);
        }

        protected override void RawHandler(byte[] data, int length, EndPoint endPoint)
        {
            if (!TryGetClient(data[GET_HEADER], endPoint))
                return;

            if (!IsHandlerPacket(data, length, endPoint))
                return;
        }

        private bool IsHandlerPacket(byte[] data, int length, EndPoint endPoint)
        {
            if (data[GET_HEADER] >= Header.Unreliable && data[GET_HEADER] <= Header.Ack)
            {
                Reliable reliable = clientsEp[endPoint];

                if (data[GET_HEADER] == Header.Ack)
                {
                    reliable.ClearAck(data);
                    return true;
                }

                if (data[GET_HEADER] == Header.Reliable)
                {
                    if (!reliable.IsNewAck(data))
                        return true;
                }

                OnHandler?.Invoke(reliable.Id, Reliable.Handler(data, length, out bool channel), channel);
                return true;
            }

            return false;
        }

        private void ClientConnected(EndPoint endPoint)
        {
            if (!clientsEp.ContainsKey(endPoint))
            {
                Reliable newReliable = new Reliable(SendTo, endPoint){ Id = (uint)endPoint.GetHashCode() };

                clientsEp.Add(endPoint, newReliable);
                clientsId.Add(newReliable.Id, newReliable);

                OnClientConnected?.Invoke(newReliable.Id);
                Log.Info($"[Server] Client Connected: Id {newReliable.Id} {endPoint}");
            } 
            SendTo(endPoint, Header.Connect);
        }

        private void ClientDisconnected(EndPoint endPoint)
        {
            if (clientsEp.TryGetValue(endPoint, out Reliable reliable))
            {
                reliable.Close();
                
                clientsEp.Remove(endPoint);
                clientsId.Remove(reliable.Id);
                
                OnClientDisconnected?.Invoke(reliable.Id);
                Log.Info($"[Server] Client Disconnected: Id {reliable.Id} {endPoint}");
            }
            SendTo(endPoint, Header.Disconnect);
        }

        public void Stop()
        {
            if (!IsRunning)
                return;
            
            foreach (var client in clientsEp.Values)
                client.Close();
            
            clientsEp.Clear();
            clientsId.Clear();
            timer.Stop();
            
            StopListener();

            Log.Info($"[Server] Stop");
        }
    }
}