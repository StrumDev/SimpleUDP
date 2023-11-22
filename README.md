# SimpleUDP v0.2.0

UDP library for C# with implementation of reliable and unreliable messages.

[![Made in Ukraine](https://img.shields.io/badge/made_in-ukraine-ffd700.svg?labelColor=0057b7)](https://stand-with-ukraine.pp.ua)

* Only UDP protocol.
* Reliable and unreliable messages.
* Small packet header size: Unreliable: 1 byte, Reliable: 3 bytes.
* Client-server connections.
* No limit on the number of simultaneous connections.
* Only IPv4 is supported (IPv6 is in development).
* Can be used in Dotnet and Unity applications.
* Licence type (MIT licence).
* The library is under development.

# Description of what is available

### Server:
    Menhods:
        Tick(); // Call this method when you want to process the data
        
        Start(Port); // Starts waiting for packets and connections
        
        Disconnect(ClientId); // Force disconnect the client
        
        Send(ClientId, Channel, Data); // Send a package one client: true send reliably, false unreliably
        
        SendAll(Channel, Data); // Send the package to all but one client: true send reliably, false unreliably
        
        SendAll(ClientId, Channel, Data); // Send a package all clients: true send reliably, false unreliably
        
        GetClientEndPoint(ClientId) // Get client EndPoint
        
        Stop(); // Stop the server quietly
        
    Callback:
        OnClientConnected(ClientId); // When the client connects
        
        OnClientDisconnected(ClientId); // When the client disconnects
        
        OnHandler(ClientId, Data, Channel); // Receives packets
        
    Parameters:
        bool IsRunning // If the server expects packets then true
        
        uint MaxTimeOut = Milliseconds; // The time after which the connection will be terminated

### Client:
    Menhods:
        Tick(); // Call this method when you want to process the data
        
        Connect(IpAddres, Port); // Creates a connection between the server and the client
        
        Disconnect(IsReliable); // Terminates the connection to the server: ( IsReliable = true: The client will wait for a response from the server )
        
        Send(Channel, Data); // Send a package: true send reliably, false unreliably
        
        Stop(); // Stop the client quietly

    Callback:
       OnConnected(); // Called when the client has successfully connected
       
       OnDisconnected(); // Called when the client has disconnected
       
       OnHandler(Data, Channel); // Receives packets
         
    Parameters:
       bool IsRunning // If the client expects packets then true
       
       uint MaxTimeOut = Milliseconds; // The time after which the connection will be terminated
       
       bool IsConnected, IsConnecting, IsDisconnecting;

# Example: Unity Game

How to test the game: create a new project in Unity, copy the UnityGame folder from the examples folder to your project, and copy the SimpleUDP folder to the new project.

https://github.com/StrumDev/SimpleUDP/assets/114677727/29eb500b-608b-49ee-a1fe-3c9632c26d4a

# Example: Console Messanger

How to test Messenger: create a new dotnet project, copy the scripts from the examples/console folder to your project, and copy the SimpleUDP folder to the new project.

https://github.com/StrumDev/SimpleUDP/assets/114677727/559c52f5-f6b8-4211-90f3-a7aa0ea5d608
