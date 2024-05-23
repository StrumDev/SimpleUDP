using System;
using SimpleUDP;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleUDP.Examples
{
    public enum Header : byte
    {
        Movement,
        Connected,
        ClientConnected,
        ClientDisconnected,
    }
    public class NetworkManager : MonoBehaviour
    {
        public string IpAddress = "127.0.0.1";
        public ushort Port = 12700;

        [Header("UI Elements")]
        public Text LocalPortText;
        public InputField InputAddress;
        public GameObject MainPanel;
        public GameObject GamePanel;

        [Header("UI Buttons")]
        public Text TextConnect;
        public Button ButtonСonnect;
        public Text TextDisconnect;
        public Button ButtonDisconnect;

        public static Server Server;
        public static Client Client;

        private void Awake()
        {
            Server = new Server();
            Client = new Client();
        }

        private void Start()
        {
            Client.OnConnected += OnConnected;
            Client.OnDisconnected += OnDisconnected;

            UIConnectButton("Connect", Connect);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (Client.State == State.Connected)
                    GamePanel.SetActive(!GamePanel.activeSelf);
            }
        }
        
        private void OnConnected()
        {
            MainPanel.SetActive(false);
            UIDisconnectButton("Disconnect", Disconnect);
        }

        private void OnDisconnected()
        {
            Server.Stop();
            LocalPortText.text = "";
            
            MainPanel.SetActive(true);
            GamePanel.SetActive(false);

            UIConnectButton("Connect", Connect);
            UIDisconnectButton("Disconnect", Disconnect);
        }

        public void StartHost()
        {
            if (Server.IsAvailablePort(12700))
                Server.Start(12700);
            else Server.Start();

            if (!Client.IsRunning)
                Client.Start();
            
            Client.Connect("127.0.0.1", Server.LocalPort);

            LocalPortText.text = $"LocalPort: {Server.LocalPort}";
        }

        public void Connect()
        {
            if (!string.IsNullOrEmpty(InputAddress.text))
            {   
                string[] str = InputAddress.text.Split(':', System.StringSplitOptions.RemoveEmptyEntries);

                if (str != null && str.Length != 0)
                {
                    IpAddress = str[0];

                    if (str.Length >= 2)
                        ushort.TryParse(str[1], out Port);      
                }
            }

            if (!Client.IsRunning)
                Client.Start();
            
            UIConnectButton("Stop Connecting", StopConnecting);
            Client.Connect(IpAddress, Port);
        }

        public void Disconnect()
        {
            UIDisconnectButton("Stop Disconnecting", StopDisconnecting);
            Client.Disconnect();
        }

        public void StopConnecting()
        {
            Client.Disconnected();
        }

        public void StopDisconnecting()
        {
            Server.Stop();
            Client.Disconnected();
        }

        private void UIConnectButton(string text, Action action)
        {
            TextConnect.text = text;
            ButtonСonnect.onClick.RemoveAllListeners();
            ButtonСonnect.onClick.AddListener(() => action());
        }

        private void UIDisconnectButton(string text, Action action)
        {
            TextDisconnect.text = text;
            ButtonDisconnect.onClick.RemoveAllListeners();
            ButtonDisconnect.onClick.AddListener(() => action());
        }

        private void OnApplicationQuit()
        {
            Server.Stop();
            Client.Stop();
        }
    }    
}