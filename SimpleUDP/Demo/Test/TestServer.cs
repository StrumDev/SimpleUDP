using System;
using SimpleUDP;
using UnityEngine;
using System.Threading.Tasks;

public class TestServer : MonoBehaviour
{
    public uint CountHandler;
    private Server server;

    private void Awake()
    {
        Log.Info = Debug.Log;
        Log.Warning = Debug.LogWarning;

        server = new Server();
        server.OnHandler = Handler;
    }

    private void Start()
    {
        Tick();

        server.Start(12700);
    }

    private void Handler(uint clientId, byte[] data, bool channel)
    {
        CountHandler++;
    }

    private async void Tick()
    {
        while (true)
        {
           server.Tick(); 
           await Task.Delay(10);
        } 
    }

    private void OnApplicationQuit()
    {
        server.Stop();
    }
}
