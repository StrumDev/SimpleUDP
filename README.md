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
```csharp
    Methods:
        server.Start(/*Port*/); // Accepts a parameter of type ushort to set the port.
        
        server.Disconnect(/*Peer*/); // Accepts a parameter of type Peer class to force a client to disconnect from the server.

        // Sending data to a specific client takes the following parameters:
        // bool channel = true send reliably / false send unreliably, byte[] data = byte array, int length = length of byte array, Peer peer = to whom to send.
        server.Send(/*Channel*/, /*Data*/, /*Length*/, /*Peer*/);
        
        server.SendAll(/*Channel*/, /*Data*/, /*Length*/); // Send the package to all but one client: true send reliably, false unreliably
        
        server.SendAll(/*Channel*/, /*Data*/, /*Length*/, /*Peer*/); // Send a package all clients: true send reliably, false unreliably
        
        server.GetClientEndPoint(ClientId) // Get client EndPoint
        
        server.Stop(); // Stop the server quietly
        
    Callback:
        OnClientConnected(ClientId); // When the client connects
        
        OnClientDisconnected(ClientId); // When the client disconnects
        
        OnHandler(ClientId, Data, Channel); // Receives packets
        
    Parameters:
        bool IsRunning // If the server expects packets then true
        
        uint MaxTimeOut = Milliseconds; // The time after which the connection will be terminated
```
### Client:
    Methods:
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
