using UnityEngine;
using System.Collections.Generic;

public class GameServer : MonoBehaviour
{
    public int Online;
    public const int MAX_PLAYERS = 256;

    private List<uint> Players = new List<uint>();

    private void Start()
    {
        GameNetwork.Ins.Server.OnClientConnected = ClientConnected;
        GameNetwork.Ins.Server.OnClientDisconnected = ClientDisconnected;
        GameNetwork.Ins.Server.OnHandler = Handler;
    }

    private void FixedUpdate()
    {
        Online = Players.Count;
    }

    private void Handler(uint clientId, byte[] data, bool channel)
    {
        Packet packet = new Packet(data, data.Length, true);

        switch (packet.Header)
        {
            case Header.Transform:
                SendAll(clientId, channel, packet);
            return;
        }
    }

    private void ClientConnected(uint clientId)
    {
        Packet packet = new Packet(Header.Connected);
        packet.AddUInt(clientId);

        packet.AddInt(Players.Count);

        foreach (var id in Players)
            packet.AddUInt(id);

        Send(clientId, true, packet);

        Players.Add(clientId);
        SendAll(clientId, true, new Packet(Header.ClientConnected).AddUInt(clientId));
    }

    private void ClientDisconnected(uint clientId)
    {
        Players.Remove(clientId);
        SendAll(clientId, true, new Packet(Header.ClientDisconnected).AddUInt(clientId));
    }

    public void Send(uint clientId, bool channel, Packet packet) => GameNetwork.Ins.Server.Send(clientId, channel, packet.Data, packet.Length);
    public void SendAll(bool channel, Packet packet) => GameNetwork.Ins.Server.SendAll(channel, packet.Data, packet.Length);
    public void SendAll(uint sckipId, bool channel, Packet packet) => GameNetwork.Ins.Server.SendAll(sckipId, channel, packet.Data, packet.Length);
}