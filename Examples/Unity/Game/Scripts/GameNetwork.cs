using SimpleUDP;
using UnityEngine;
using System.Threading.Tasks;

public enum Header : byte { Connected, ClientConnected, ClientDisconnected, Transform }
public class GameNetwork : MonoBehaviour
{
    public string IpAddress = "127.0.0.1";
    public ushort Port = 12700;
    
    public static Server Server;
    public static Client Client;

    private GUIStyle style;
    private bool isActive;

    private void Awake()
    {
        Server = new Server();
        Client = new Client();
    }

    private void Start()
    {
        Log.Initializer(Debug.Log, Debug.LogWarning);

        style = new GUIStyle();
        style.fontStyle = FontStyle.Bold;

        isActive = true;
        
        Tick();
        TickPeer();
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 250, 9999));
        
        ServerGUI();
        ClientGUI();
        AddressGUI();
            
        GUILayout.Label("Reset position Key: [R]", style);

        GUILayout.EndArea();
    }

    private void ServerGUI()
    {
        if (!Server.IsRunning)
        {
            if (GUILayout.Button("Start Server"))
                Server.Start(Port);
        }
        else
        {
            if (GUILayout.Button("Stop Server"))
                Server.Stop();
        }
    }

    private void ClientGUI()
    {
        if (Client.State == State.NoConnect)
        {
            if (GUILayout.Button("Connect"))
                Client.Connect(IpAddress, Port);
        }
        else if (Client.State == State.Connecting)
        {
            if (GUILayout.Button("Stop Connecting..."))
                Client.Stop();
        }
        else if (Client.State == State.Connected)
        {
            if (GUILayout.Button("Disconnect"))
                Client.Disconnect();
        }
        else if (Client.State == State.Disconnecting)
        {
            if (GUILayout.Button("Disconnecting..."))
                Client.Stop();
        }
    }

    private void AddressGUI()
    {
        GUILayout.BeginHorizontal();
        try {
            IpAddress = GUILayout.TextField(IpAddress);
            Port = ushort.Parse(GUILayout.TextField(Port.ToString()));
        } catch (System.Exception) {}
        GUILayout.EndHorizontal();
    }

    private async void Tick()
    {
        while (isActive)
        {
            Server?.ReceiveAll();
            Client?.ReceiveAll();
            await Task.Delay(10);
        }
    }

    private async void TickPeer()
    {
        while (isActive)
        {
            Server?.UpdatePeer(10);
            Client?.UpdatePeer(10);
            await Task.Delay(10);
        }
    }

    private void OnApplicationQuit()
    {
        isActive = false;
        Server?.Stop();
        Client?.Disconnect();
    }
}