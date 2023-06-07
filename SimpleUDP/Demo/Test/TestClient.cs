using System;
using SimpleUDP;
using UnityEngine;
using System.Threading.Tasks;

public class TestClient : MonoBehaviour
{
    public bool IsSend;
    public byte CountSend = 1;
    public bool IsReliable = true;
    public uint CurrentSend;

    private Client client;

    private void Awake()
    {
        client = new Client();
    }

    private void Start()
    {
        Tick();
        
        client.Connect("127.0.0.1", 12700);
    }

    private void FixedUpdate()
    {
        if (IsSend)
        {
            for (int i = 0; i < CountSend; i++)
            {
                CurrentSend++;
                client.Send(IsReliable, new byte[256], 256);
            }  
        }
    }

    private async void Tick()
    {
        while (true)
        {
           client.Tick(); 
           await Task.Delay(10);
        } 
    }

    private void OnApplicationQuit()
    {
        client.Stop();
    }
}
