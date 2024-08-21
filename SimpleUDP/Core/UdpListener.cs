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

        public Action<EndPoint, byte[]> OnReceiveBroadcast;
        public Action<EndPoint, byte[]> OnReceiveUnconnected;

        public bool IsRunning { get; private set; }

        public ushort ReceiveBufferSize = 2048;
        
        public bool EnableBroadcast => socket.EnableBroadcast;
        
        public ushort LocalPort => (ushort)LocalEndPoint.Port;
        public IPEndPoint LocalEndPoint => (IPEndPoint)socket.LocalEndPoint;
        
        public int AvailablePackages => IsRunning ? socket.Available : 0;
        public bool SocketPoll => IsRunning ? socket.Poll(500000, SelectMode.SelectRead) : false; //500000 => 0.5s

        private Socket socket;
        private EndPoint sender;
        private Stopwatch watch;

        private byte[] buffer;
        private object locker = new object();
        
        protected const byte IndexHeader = 0;
        protected const byte HeaderReliable = 2;
        protected const byte HeaderUnreliable = 1;

        // SioUdpConnreset = IOC_IN | IOC_VENDOR | 12
        private const int SioUdpConnreset = -1744830452;

        public UdpListener()
        {
            watch = new Stopwatch();
            sender = new IPEndPoint(IPAddress.Any, 0);
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

        public void Start(ushort port = 0, bool enableBroadcast = false)
        {
            lock (locker)
            {
                if (!IsRunning)
                {
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        socket.IOControl(SioUdpConnreset, new byte[] {0}, null);
                    
                    buffer = new byte[ReceiveBufferSize];

                    socket.EnableBroadcast = enableBroadcast;
                    socket.Bind(new IPEndPoint(IPAddress.Any, port));
                    
                    IsRunning = true;

                    OnListenerStarted();
                }
            }
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

        public void TickUpdate()
        {
            uint elapsed = (uint)watch.ElapsedMilliseconds;
            
            watch.Restart();
            OnTickUpdate(elapsed);
        }
        
        public void Stop()
        {
            lock (locker)
            {
                if (IsRunning)
                {
                    watch.Stop();
                    socket.Close(); 
                    IsRunning = false;

                    OnListenerStopped();
                }
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

        protected byte[] CopyPacket(byte[] data, int length, int offset)
        {
            byte[] buffer = new byte[length - offset];
            Buffer.BlockCopy(data, offset, buffer, 0, length - offset);
            
            return buffer;
        }



        protected void HandlerBroadcast(byte[] data, int length, EndPoint endPoint)
        {
            lock (locker)
            {
                byte[] buffer = new byte[length - HeaderUnreliable];
                Buffer.BlockCopy(data, HeaderUnreliable, buffer, 0, length - HeaderUnreliable);
                OnReceiveBroadcast?.Invoke(endPoint, buffer);
            }
        }

        protected void HandlerUnconnected(byte[] data, int length, EndPoint endPoint)
        {
            lock (locker)
            {
                byte[] buffer = new byte[length - HeaderUnreliable];
                Buffer.BlockCopy(data, HeaderUnreliable, buffer, 0, length - HeaderUnreliable);
                OnReceiveUnconnected?.Invoke(endPoint, buffer);
            }
        }
        
        protected virtual void OnListenerStarted() { OnStarted?.Invoke(); }
        protected virtual void OnListenerStopped() { OnStopped?.Invoke(); }
        
        protected virtual void OnTickUpdate(uint deltaTime) { }
        protected virtual void OnRawHandler(byte[] data, int length, EndPoint endPoint) { }
    }

    public delegate void SendTo(byte[] data, int size, EndPoint endPoint);
}