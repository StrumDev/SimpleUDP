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

```csharp
### Server:

    Methods:
        // Accepts a parameter of type ushort to set the port.
            server.Start(/*Port*/);

        // Accepts a parameter of type Peer class to force a client to disconnect from the server.
            server.Disconnect(/*Peer*/);

        // Sending data to a specific client takes the following parameters:
        // bool channel = true send reliably / false send unreliably, byte[] data = byte array, int length = length of byte array, Peer peer = to whom to send.
            server.Send(/*Channel*/, /*Data*/, /*Length*/, /*Peer*/);

        // Sending data to all clients takes the following parameters:
        // bool channel = true send reliably / false send unreliably, byte[] data = byte array, int length = length of byte array.
            server.SendAll(/*Channel*/, /*Data*/, /*Length*/);

        // Sending data to all clients with client rejection has the following parameters:
        // bool channel = true send reliably / false send unreliably, byte[] data = byte array, int length = length of byte array, Peer to peer = to whom not to send.
            server.SendAll(/*Channel*/, /*Data*/, /*Length*/, /*Peer*/);

        // Receiving and processing all received data.
        // Note: call this method in loops or updates to process messages.
            server.ReceiveAll();

        // Stop the server.
            server.Stop(); 
        
    Callback:
        // Called when a new client is connected, returning the <Peer> parameter.
            server.OnClientConnected = Method(<Peer>);

        // Called when a new client is disconnected, returning the <Peer> parameter.
            server.OnClientDisconnected = Method(<Peer>);

        // Called when a new message is received, returning the <bool, byte[], Peer> parameters
            server.OnHandler = Method(<bool, byte[], Peer>);

### Client:
    Methods:
        // Accepts a parameter of type ushort to set the port.
            client.Connect(/*Port*/);

        // Accepts a parameter of type Peer class to force a client to disconnect from the server.
            client.Disconnect(/*Peer*/);

        // Sending data to a specific client takes the following parameters:
        // bool channel = true send reliably / false send unreliably, byte[] data = byte array, int length = length of byte array.
            client.Send(/*Channel*/, /*Data*/, /*Length*/);

        // Receiving and processing all received data.
        // Note: call this method in loops or updates to process messages.
            client.ReceiveAll();

        // Stop the client.
            client.Stop(); 

    Callback:
       OnConnected(); // Called when the client has successfully connected
       
       OnDisconnected(); // Called when the client has disconnected
       
       OnHandler(Data, Channel); // Receives packets
```

# Example: Unity Game

How to test the game: create a new project in Unity, copy the UnityGame folder from the examples folder to your project, and copy the SimpleUDP folder to the new project.

https://github.com/StrumDev/SimpleUDP/assets/114677727/29eb500b-608b-49ee-a1fe-3c9632c26d4a

# Example: Console Messanger

How to test Messenger: create a new dotnet project, copy the scripts from the examples/console folder to your project, and copy the SimpleUDP folder to the new project.

https://github.com/StrumDev/SimpleUDP/assets/114677727/559c52f5-f6b8-4211-90f3-a7aa0ea5d608
