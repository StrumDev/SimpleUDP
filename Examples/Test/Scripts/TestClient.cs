using SimpleUDP;
using System.Net;
using UnityEngine;

namespace SimpleUDP.Examples
{
    public class TestClient : MonoBehaviour
    {
        public string IpAddress = "127.0.0.1";
        public ushort Port = 12700;
        
        public ushort BroadcastPort = 12700;

        public uint TimeOut = 5000;
        public bool Broadcast = true;

        public UdpClient client;

        private void Start()
        {
            client = new UdpClient();
            
            client.OnStarted = OnStarted;
            client.OnStopped = OnStopped;

            client.OnConnected = OnConnected;
            client.OnDisconnected = OnDisconnected;

            client.OnReceiveReliable = OnReceiveReliable;
            client.OnReceiveUnreliable = OnReceiveUnreliable;

            client.TimeOut = TimeOut;
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(270, 40, 250, 999));

            if (client.IsRunning)
            {
                if (GUILayout.Button("Stop Client"))
                    client.Stop();

                if (client.State == State.NoConnect)
                {
                    if (GUILayout.Button("Connect"))
                        client.Connect(IpAddress, Port);
                }
                else if (client.State == State.Connecting)
                {
                    if (GUILayout.Button("Stop Connecting..."))
                        client.QuietDisconnect();
                }
                else if (client.State == State.Connected)
                {
                    if (GUILayout.Button("Disconnect"))
                        client.Disconnect();

                    if (GUILayout.Button("Quiet Disconnect"))
                        client.QuietDisconnect();

                    if (GUILayout.Button("Send Reliable"))
                        SendReliable();

                    if (GUILayout.Button("Send Unreliable"))
                        SendUnreliable();

                    if (GUILayout.Button("Send Broadcast"))
                        SendBroadcast();

                    if (GUILayout.Button("Send Unconnected"))
                        SendUnconnected();
                }
                else if (client.State == State.Disconnecting)
                {
                    if (GUILayout.Button("Stop Disconnecting..."))
                        client.QuietDisconnect();
                }    
            }
            else
            {   // If the port is zero, the available listening port will be given to the client.
                if (GUILayout.Button("Start Client"))
                    client.Start(0, Broadcast); 
            }
            
            GUILayout.EndArea();
        }

        public void SendReliable()
        {
            Packet packet = Packet.Write();
            packet.String("Hello, Reliable from client!");

            client.SendReliable(packet);
        }

        public void SendUnreliable()
        {
            Packet packet = Packet.Write();
            packet.String("Hello, Unreliable from client!");

            client.SendUnreliable(packet);
        }

        public void SendBroadcast()
        {
            Packet packet = Packet.Write();
            packet.String("Hello, Broadcast from client!");

            client.SendBroadcast(BroadcastPort, packet);
        }

        public void SendUnconnected()
        {
            Packet packet = Packet.Write();
            packet.String("Hello, Unconnected from client!");

            client.SendUnconnected(client.EndPoint, packet);
        }

        private void OnStarted()
        {
            Debug.Log($"[Client] OnStarted on port: {client.LocalPort}");
        }

        private void OnStopped()
        {
            Debug.Log($"[Client] OnStopped");
        }

        private void OnConnected()
        {
            Debug.Log($"[Client] OnConnected to: {client.EndPoint}");
        }

        private void OnDisconnected()
        {
            Debug.Log($"[Client] OnDisconnected from: {client.EndPoint}");
        }

        private void OnReceiveReliable(byte[] packet)
        {
            Packet read = Packet.Read(packet);

            Debug.Log($"[Client] OnReceiveReliable: {read.String()}");
        }

        private void OnReceiveUnreliable(byte[] packet)
        {
            Packet read = Packet.Read(packet);

            Debug.Log($"[Client] OnReceiveUnreliable: {read.String()}");
        }

        private void FixedUpdate()
        {
            client.Receive();
            client.TickUpdate();
        }

        private void OnApplicationQuit()
        {
            client.Stop();
        }
    }
}
