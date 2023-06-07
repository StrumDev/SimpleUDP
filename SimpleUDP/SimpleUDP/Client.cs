using System;
using System.Net;
using Timer = System.Timers.Timer;

namespace SimpleUDP
{
    public class Client : Listener
    {
        public Action OnConnected;
        public Action OnDisconnected;
        public Action<byte[], bool> OnHandler;

        public uint MaxTimeOut = 10000; // 10s

        public bool IsConnected { get; private set; }
        public bool IsConnecting { get; private set; }
        public bool IsDisconnecting { get; private set; }

        private const uint UPDATE_TIME_OUT = 1000; // 1s
        private EndPoint server;
        private Pending pending;
        private Reliable reliable;
        private Timer timer;

        private bool IsTimeOut => (DateTime.UtcNow - reliable.DateTime).TotalMilliseconds > MaxTimeOut;

        public Client()
        {
            timer = new Timer(UPDATE_TIME_OUT);
            timer.Elapsed += (o, e) => UpdateTimeOut();
            timer.AutoReset = true;
        }

        public void Connect(string ipAddress, ushort port)
        {
            if (IsRunning)
                return;

            server = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            pending = new Pending(SendTo, server, Disconnected){ Interval = 250 };

            StartListener();
            Log.Info($"[Client] Connect to: {server}");
            
            IsConnecting = true;
            pending.Send(Header.Connect);
        }

        public void Disconnect(bool isReliable = false)
        {
            if (!IsRunning && IsDisconnecting)
                return;

            if (isReliable && IsConnected)
            {
                pending.Interval = 50;
                pending.Send(Header.Disconnect);
                IsDisconnecting = true;
                return;
            }
            
            SendTo(server, Header.Disconnect);

            Disconnected();
        }

        public void Send(bool channel, byte[] data, int length)
        {
            if (!IsConnected)
                return;

            if (channel) reliable.SendReliable(data, length);
            else reliable.SendUnreliable(data, length);
        }

        private bool IsConnection(byte header, EndPoint endPoint)
        {
            switch (header)
            {
                case Header.Ping:
                    ResetTimeOut();
                    return false;
                case Header.Connect:
                    Connected();
                    return false;
                case Header.Disconnect:
                    Disconnected();
                return false;
            }
            
            if (!server.Equals(endPoint))
                return false;
            
            return IsConnected;
        }

        protected override void RawHandler(byte[] data, int length, EndPoint endPoint)
        {
            if (!IsConnection(data[GET_HEADER], endPoint))
                return;

            if (!IsHandlerPacket(data, length))
                return;
        }

        private bool IsHandlerPacket(byte[] data, int length)
        {
            if (data[GET_HEADER] >= Header.Unreliable && data[GET_HEADER] <= Header.Ack)
            {
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

                OnHandler?.Invoke(Reliable.Handler(data, length, out bool channel), channel);
                return true;
            }

            return false;
        }

        private void Connected()
        {
            if (!IsConnected)
            {
                IsConnected = true;
                IsConnecting = false;

                reliable = new Reliable(SendTo, server);
                timer.Start();

                pending.Interval = 50;
                pending.Clear();

                OnConnected?.Invoke();
                Log.Info($"[Client] Connected to: {server}");
            }
        }

        public void Disconnected()
        {
            Stop();

            OnDisconnected?.Invoke();
            Log.Info($"[Client] Disconnected from: {server}");
        }

        private void UpdateTimeOut()
        {
            if (!IsConnected)
                return;

            if (!IsTimeOut)
                SendTo(server, Header.Ping);
            else
                Disconnected();
        }

        private void ResetTimeOut()
        {
            if (!IsConnected)
                return;

            reliable.DateTime = DateTime.UtcNow;
        }

        public void Stop()
        {
            if (!IsRunning)
                return;

            IsConnected = false;
            IsConnecting = false;
            IsDisconnecting = false;
            
            pending?.Close();
            reliable?.Close();
            timer?.Stop();

            StopListener();
            Log.Info($"[Client] Stop");
        }
    }
}
