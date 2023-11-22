using System;
using SimpleUDP;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Network
{
    public class NetClient
    {
        private const int DeltaTime = 20;
        
        private string name;
        private Client client;

        public void Start(string ipAddress, ushort port, string name)
        {
            client = new Client();
            client.OnHandler = Handler;

            client.Connect(ipAddress, port);

            this.name = name;
            SendMessage();

            while (true)
            {
                client?.ReceiveAll();
                client?.UpdatePeer((uint)DeltaTime);

                Thread.Sleep(DeltaTime);
            }
        }

        private async void SendMessage()
        {
            while (true)
            {
                await Task.Yield();
                
                string msg = Console.ReadLine();

                byte[] packet = Encoding.UTF8.GetBytes($"[{name}]: {msg}");
                client.Send(true, packet, packet.Length); 
            }
        }

        private void Handler(bool channel, byte[] data)
        {
            string msg = Encoding.UTF8.GetString(data);
            Console.WriteLine(msg);
        }
    }
}