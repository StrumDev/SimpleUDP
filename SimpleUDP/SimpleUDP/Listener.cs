using System;
using System.Net;
using System.Net.Sockets;

namespace SimpleUDP
{
    public abstract class Listener
    {
        public bool IsRunning = false;
        public bool IsPoll => IsRunning ? socket.Poll(500000, SelectMode.SelectRead) : false; //500000 => 0.5s

        private Socket socket;
        private byte[] buffer;
        EndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        
        private object locker = new object();

        protected const byte GET_HEADER = 0;

        protected void StartListener(ushort port = 0)
        {
            lock (locker)
            {
                if (!IsRunning)
                {
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP);
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

        private void Receive()
        {
            if (!IsRunning)
                return;
            
            if (socket.Available == 0)
                return;

            try
            {
                for (int i = 0; i < socket.Available; i++)
                {
                    int size = socket.ReceiveFrom(buffer, ref sender);

                    RawHandler(buffer, size, sender);
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        public void Tick() => Receive();
        
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