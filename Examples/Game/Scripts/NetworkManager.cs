using SimpleUDP;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace SimpleUDP.Examples
{
    public enum Header : byte
    {
        Movement,
        ClientConnected,
        ClientDisconnected,
    }
    public class NetworkManager : MonoBehaviour
    {
        public string IpAddress = "127.0.0.1";
        public ushort Port = 12700;

        public static UdpServer Server;
        public static UdpClient Client;

        [Header("Diagnostics Server")]
        public uint ElapsedTickServer;
        public int AvailableServer;
        
        [Header("Diagnostics Client")]
        public uint ElapsedTickClient;
        public int AvailableClient;

        private bool isActive;
        private UIMenager uiMenager;

        private void Awake()
        {
            Server = new UdpServer();
            Client = new UdpClient();    
        
            Client.OnConnected += OnConnected;
            Client.OnDisconnected += OnDisconnected;
        }

        private void Start()
        {
            isActive = true;

            UpdateAsyncClient();
            
            uiMenager = GetComponent<UIMenager>();

            uiMenager.SetActionConnect("Connect", Connect);
            uiMenager.SetActionDisconnect("Disconnect", Disconnect);

            Client.Start();
        }

        private void OnConnected()
        {
            uiMenager.Connected();
            
            if (Server.IsRunning)
            {
                uiMenager.LocalPortText.gameObject.SetActive(true);
                uiMenager.LocalPortText.text = $"LocalPort: {Server.LocalPort}";
            }  
        }

        private void OnDisconnected()
        {
            uiMenager.Disconnected();

            uiMenager.SetActionConnect("Connect", Connect);
            uiMenager.SetActionDisconnect("Disconnect", Disconnect);

            if (Server.IsRunning)
                Server.Stop();
            
            uiMenager.LocalPortText.gameObject.SetActive(false);
        }

        public void CreateGame()
        {
            if (Server.IsAvailablePort(12700))
                Server.Start(12700);
            else
                Server.Start();

            new Thread(UpdateThreadServer).Start();
            Client.Connect("127.0.0.1", Server.LocalPort);
        }

        public void Connect()
        {
            if (!string.IsNullOrEmpty(uiMenager.InputAddress.text))
            {   
                string[] str = uiMenager.InputAddress.text.Split(':', System.StringSplitOptions.RemoveEmptyEntries);

                if (str != null && str.Length != 0)
                {
                    IpAddress = str[0];

                    if (str.Length >= 2)
                        ushort.TryParse(str[1], out Port);
                    else
                        Port = 12700;
                }
            }

            uiMenager.SetActionConnect("Stop Connecting", Client.QuietDisconnect);
            Client.Connect(IpAddress, Port);
        }

        public void Disconnect()
        {
            uiMenager.SetActionDisconnect("Stop Disconnecting", Client.QuietDisconnect);
            Client.Disconnect();
        }

        private void FixedUpdate()
        {
            if (Client.State == State.Connected)
                uiMenager.SetRttText(Client.Rtt);

            AvailableServer = Server.AvailablePackages;
            AvailableClient = Client.AvailablePackages;
        }

        private void UpdateThreadServer()
        {
            Stopwatch swServer = new Stopwatch();

            while (isActive)
            {
                if (!Server.IsRunning)
                    return;
                
                if (Server.SocketPoll)
                {
                    ElapsedTickServer = (uint)swServer.ElapsedMilliseconds;
                    swServer.Restart();

                    // 1000ms / 10 = TickRate: 100
                    Server.Receive();
                    Server.TickUpdate();
                }

                Thread.Sleep(10);
            }
        }

        private async void UpdateAsyncClient()
        {
            Stopwatch swClient = new Stopwatch();

            while (isActive)
            {
                ElapsedTickClient = (uint)swClient.ElapsedMilliseconds;
                swClient.Restart();

                // 1000ms / 20 = TickRate: 50
                Client.Receive();
                Client.TickUpdate();
                
                await Task.Delay(20);
            }
        }

        private void OnApplicationQuit()
        {
            isActive = false;

            Server.Stop();
            Client.Stop();
        }
    }    
}