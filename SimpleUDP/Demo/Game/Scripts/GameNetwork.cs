using SimpleUDP;
using UnityEngine;
using System.Threading.Tasks;

public enum Header : byte { Connected, ClientConnected, ClientDisconnected, Transform }
public class GameNetwork : MonoBehaviour
{
    public string IpAddress = "127.0.0.1";
    public ushort Port = 12700;
    
    public Server Server;
    public Client Client;

    public static GameNetwork Ins;

    private GUIStyle style;

    private void Awake()
    {
        Ins = this;

        Server = new Server();
        Client = new Client();
    }

    private void Start()
    {
        style = new GUIStyle();
        style.fontStyle = FontStyle.Bold;

        Tick();
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
        if (!Client.IsConnected && !Client.IsConnecting)
        {
            if (GUILayout.Button("Connect"))
                Client.Connect(IpAddress, Port);
        }
        else if (Client.IsConnecting)
        {
            if (GUILayout.Button("Stop Connecting"))
                Client.Stop();
        }
        else if (Client.IsConnected)
        {
            if (GUILayout.Button("Disconnect"))
                Client.Disconnect(true);
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
        while (true)
        {
            Server?.Tick();
            Client?.Tick();
            await Task.Delay(10);
        }
    }

    private void OnApplicationQuit()
    {
        Server?.Stop();
        Client?.Disconnect();
    }
}