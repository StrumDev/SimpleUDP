using UnityEngine;
using System.Collections.Generic;

public class GameClient : MonoBehaviour
{
    public GameObject Prefab;

    private Player LocalPlayer;
    private Dictionary<uint, Player> Players = new Dictionary<uint, Player>();

    private void Start()
    {
        GameNetwork.Ins.Client.OnDisconnected = Disconnected;
        GameNetwork.Ins.Client.OnHandler = Handler;
    }

    private void Handler(byte[] data, bool channel)
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
        if (LocalPlayer != null) 
            Send(LocalPlayer.GetTransform(), false);
    }

    private void SetTransform(Packet packet)
    {
        uint clientId = packet.GetUInt();
        
        Vector3 pos = new Vector3(packet.GetFloat(), packet.GetFloat(), packet.GetFloat());
        Quaternion rot = new Quaternion(packet.GetFloat(), packet.GetFloat(), packet.GetFloat(), packet.GetFloat());

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
        uint myId = packet.GetUInt();
        
        SpawnPlayer(myId, true);

        int count = packet.GetInt();
        for (int i = 0; i < count; i++)
            SpawnPlayer(packet.GetUInt(), false);
        
        Debug.Log($"(Client): Connected Id: {myId}");
    }

    private void ClientConnected(uint clientId)
    {
        SpawnPlayer(clientId, false);
        Debug.Log($"(Client): Client Connected: {clientId}");
    }

    private void ClientDisconnected(uint clientId)
    {
        Destroy(Players[clientId].gameObject);
        Players.Remove(clientId);
        Debug.Log($"(Client): Client Disconnected: {clientId}");
    }
    
    private void Disconnected()
    {
        foreach (var player in Players.Values)
            Destroy(player.gameObject);
        Players.Clear();
        
        if (LocalPlayer != null) 
            Destroy(LocalPlayer.gameObject);
        LocalPlayer = null;

        Debug.Log($"(Client): Disconnected");
    }

    public void Send(Packet packet, bool channel) => GameNetwork.Ins.Client.Send(channel, packet.Data, packet.Length);
}
