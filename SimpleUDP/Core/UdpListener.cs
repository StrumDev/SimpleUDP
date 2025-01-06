using System;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SimpleUDP.Core
{   
    using UdpClient = System.Net.Sockets.UdpClient;
    public class UdpListener
    {
        public Action OnStarted;
        public Action OnStopped;

        public bool IsRunning { get; private set; }
        public ushort LocalPort { get; private set; }
        public EndPoint LocalEndPoint { get; private set; }
        
        public bool LimitedSizePackage = true;
        public ushort ReceiveBufferSize = 2048;
        
        public const ushort MaxSizePacket = 1432;
        
        public bool EnableBroadcast => socket.EnableBroadcast;
        
        public int AvailablePackages => IsRunning ? socket.Available : 0;
        public bool SocketPoll => IsRunning ? socket.Poll(500000, SelectMode.SelectRead) : false; //500000 => 0.5s
        
        private Socket socket;
        private EndPoint sender;
        private Stopwatch watch;

        private const int OneKilobyte = 1024;

        private byte[] buffer;
        private object locker = new object();

        // SioUdpConnreset = IOC_IN | IOC_VENDOR | 12
        private const int SioUdpConnreset = -1744830452;

        public UdpListener()
        {
            watch = new Stopwatch();
            sender = new IPEndPoint(IPAddress.Any, 0);
        }

        public void Start(ushort port = 0, bool enableBroadcast = false)
        {
            lock (locker)
            {
                if (!IsRunning)
                {
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    
                    socket.Blocking = true;
                    socket.EnableBroadcast = enableBroadcast; 
                    
                    BindSocket(new IPEndPoint(IPAddress.Any, port));
                    
                    OnListenerStarted();
                }
            }
        }

        protected void AdjustBufferSizes(int kilobytes)
        {
            socket.SendBufferSize = OneKilobyte * kilobytes;
            socket.ReceiveBufferSize = OneKilobyte * kilobytes;
        }

        private void BindSocket(IPEndPoint endPoint)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                socket.IOControl(SioUdpConnreset, new byte[] {0}, null);
            
            socket.Bind(endPoint);

            buffer = new byte[ReceiveBufferSize];
            
            LocalPort = (ushort)((IPEndPoint)socket.LocalEndPoint).Port;
            LocalEndPoint = (IPEndPoint)socket.LocalEndPoint;
            
            IsRunning = true;
        }
        
        protected void SendTo(EndPoint endPoint, params byte[] data)
        {
            SendTo(data, data.Length, endPoint);
        }
        
        protected void SendTo(byte[] data, int size, EndPoint endPoint)
        {
            if (IsRunning)
                socket?.SendTo(data, size, SocketFlags.None, endPoint);
        }

        public void Receive()
        {
            if (IsRunning)
            {
                try
                {
                    while (socket.Available > 0)
                    {
                        int size = socket.ReceiveFrom(buffer, ref sender);

                        if (LimitedSizePackage && size >= MaxSizePacket)
                            continue;
                        
                        OnRawHandler(buffer, size, sender);
                    }
                }
                catch (Exception) { return; }
            }   
        }

        public void TickUpdate()
        {
            uint elapsed = (uint)watch.ElapsedMilliseconds;
            
            watch.Restart();
            OnTickUpdate(elapsed);
        }

        public bool IsAvailablePort(ushort port)
        {
            try
            {
                using (UdpClient udpPort = new UdpClient(port))
                    udpPort.Close();
                
                return true;
            }
            catch (SocketException) { return false; }
        }
        
        public void Stop()
        {
            lock (locker)
            {
                if (IsRunning)
                {
                    IsRunning = false;

                    watch.Stop();
                    socket.Close();

                    OnListenerStopped();
                }
            }
        }

        protected virtual void OnTickUpdate(uint deltaTime) { }

        protected virtual void OnListenerStarted() { OnStarted?.Invoke(); }
        protected virtual void OnListenerStopped() { OnStopped?.Invoke(); }
        
        protected virtual void OnRawHandler(byte[] data, int length, EndPoint endPoint) { }
    }

    public delegate void SendTo(byte[] data, int size, EndPoint endPoint);
}