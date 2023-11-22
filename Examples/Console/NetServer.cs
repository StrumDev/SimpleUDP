using System;
using SimpleUDP;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Network
{
    public class NetServer
    {
        private const int DeltaTime = 20;
        private Server server;

        public void Start(ushort port)
        {
            server = new Server();
            server.OnHandler = Handler;

            server.Start(port);

            while (true)
            {
                server?.ReceiveAll();
                server?.UpdatePeer((uint)DeltaTime);

                Thread.Sleep(DeltaTime);
            }
        }

        private void Handler(bool channel, byte[] data, Peer peer)
        {
            server.SendAll(channel, data, data.Length, peer);

            string msg = Encoding.UTF8.GetString(data);
            Console.WriteLine(msg);
        }
    }
}