using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameClient : MonoBehaviour
{
    public GameObject Prefab;
    public Text RTTText;

    private Player LocalPlayer;
    private Dictionary<uint, Player> Players = new Dictionary<uint, Player>();

    private Stack<Player> didDisconnect = new Stack<Player>();

    private void Start()
    {
        GameNetwork.Client.OnDisconnected = Disconnected;
        GameNetwork.Client.OnHandler = Handler;
    }

    private void Handler(bool channel, byte[] data)
    {
        Packet packet = new Packet(data, data.Length, true); 

        switch (packet.Header)
        {
            case Header.Transform:
                SetTransform(packet);
                return;
            case Header.Connected:
                Connected(packet);
                return;
            case Header.ClientConnected:
                ClientConnected(packet.GetUInt());
                return;
            case Header.ClientDisconnected:
                ClientDisconnected(packet.GetUInt());
            return;
        }
    }

    private void FixedUpdate()
    {
        if (GameNetwork.Client.State == SimpleUDP.State.Connected)
            RTTText.text = $"RTT: {GameNetwork.Client.Peer.RTT}";

        if (LocalPlayer != null)
            Send(LocalPlayer.GetTransform(), false);
        
  
        if (didDisconnect.Count != 0)   
            for (int i = 0; i < didDisconnect.Count; i++)
                Destroy(didDisconnect.Pop().gameObject);
    }

    private void SetTransform(Packet packet)
    {
        uint clientId = packet.GetUInt();
        
        Vector3 pos = new Vector3(packet.GetFloat(), packet.GetFloat(), packet.GetFloat());
        Quaternion rot = Quaternion.Euler(packet.GetFloat(), packet.GetFloat(), packet.GetFloat());

        Players[clientId].SetPosition(pos, rot);
    }

    private void SpawnPlayer(uint clientId, bool isLocal)
    {
        Player player = Instantiate(Prefab).GetComponent<Player>();
        player.IsLocal = isLocal;
        player.ClientId = clientId;

        if (isLocal) LocalPlayer = player;
        else Players.Add(clientId, player);
    }

    private void Connected(Packet packet)
    {
        lock (Players)
        {
            uint myId = packet.GetUInt();
            
            SpawnPlayer(myId, true);

            int count = packet.GetInt();
            for (int i = 0; i < count; i++)
                SpawnPlayer(packet.GetUInt(), false);
            
            Debug.Log($"(Client): Connected Id: {myId}");
        }
    }

    private void ClientConnected(uint clientId)
    {
        lock (Players)
        {
            SpawnPlayer(clientId, false);
            Debug.Log($"(Client): Client Connected: {clientId}");
        }
    }

    private void ClientDisconnected(uint clientId)
    {
        lock (Players)
        {
            didDisconnect.Push(Players[clientId]);
            Players.Remove(clientId);
            Debug.Log($"(Client): Client Disconnected: {clientId}");
        }
    }
    
    private void Disconnected()
    {
        if (LocalPlayer != null)
            didDisconnect.Push(LocalPlayer);
            
        LocalPlayer = null;

        foreach (var player in Players.Values)
            didDisconnect.Push(player);

        Players.Clear();
        
        Debug.Log($"(Client): Disconnected");
    }

    public void Send(Packet packet, bool channel) => 
        GameNetwork.Client.Send(channel, packet.Data, packet.Length);
}