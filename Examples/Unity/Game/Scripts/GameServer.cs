using UnityEngine;
using System.Collections.Generic;
using System.Net;
using SimpleUDP;

public class GameServer : MonoBehaviour
{
    public int Online;
    public const int MAX_PLAYERS = 256;

    private Dictionary<EndPoint, uint> ClientID = new Dictionary<EndPoint, uint>();
    private List<uint> Players = new List<uint>();

    private void Start()
    {
        GameNetwork.Server.OnClientConnected = ClientConnected;
        GameNetwork.Server.OnClientDisconnected = ClientDisconnected;
        GameNetwork.Server.OnHandler = Handler;

        GameNetwork.Server.OnStop = ClearClients;
    }

    private void FixedUpdate()
    {
        Online = Players.Count;
    }

    private void Handler(bool channel, byte[] data, Peer peer)
    {
        Packet packet = new Packet(data, data.Length, true);

        switch (packet.Header)
        {
            case Header.Transform:
                SendAll(channel, packet, peer);
            return;
        }
    }

    private void ClientConnected(Peer peer)
    {
        if (Players.Count >= MAX_PLAYERS)
        {
            GameNetwork.Server.Disconnect(peer);
            return;
        }

        uint newId = (uint)peer.EndPoint.GetHashCode();
        ClientID.Add(peer.EndPoint, newId);

        Packet packet = new Packet(Header.Connected);
        packet.AddUInt(newId);

        packet.AddInt(Players.Count);

        foreach (var id in Players)
            packet.AddUInt(id);

        Send(true, packet, peer);

        Players.Add(newId);
        SendAll(true, new Packet(Header.ClientConnected).AddUInt(newId), peer);
    }

    private void ClientDisconnected(Peer peer)
    {
        if (ClientID.ContainsKey(peer.EndPoint))
        {
            uint clientId = ClientID[peer.EndPoint];
            ClientID.Remove(peer.EndPoint);

            Players.Remove(clientId);
            SendAll(true, new Packet(Header.ClientDisconnected).AddUInt(clientId), peer);
        }
    }

    public void ClearClients()
    {
        ClientID.Clear();
        Players.Clear();
    }

    public void Send(bool channel, Packet packet, Peer peer) => 
        GameNetwork.Server.Send(channel, packet.Data, packet.Length, peer);
    public void SendAll(bool channel, Packet packet) => 
        GameNetwork.Server.SendAll(channel, packet.Data, packet.Length);
    public void SendAll(bool channel, Packet packet, Peer peer) => 
        GameNetwork.Server.SendAll(channel, packet.Data, packet.Length, peer);
}