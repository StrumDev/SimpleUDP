# SimpleUDP Documentation

## Overview
SimpleUDP is a lightweight and easy-to-use UDP networking library suitable for both .NET applications and Unity projects. It supports both server and client functionality, with features for reliable and unreliable messaging.

**Version:** 0.3.0

## General

The following properties and methods are available in both the `Server` and `Client` classes:

### Properties

- `bool IsRunning` `Read only`
  - Indicates whether the listening is active.
  - ```csharp
    if (server.IsRunning)
    {
        Console.WriteLine("Server is running.");
    }
    ```
  
- `ushort LocalPort` `Read only`
  - The local port being listened to.
  - ```csharp
    ushort port = server.LocalPort;
    Console.WriteLine($"Server is listening on port: {port}");
    ```
  
- `IPEndPoint LocalEndPoint` `Read only`
  - The local IP address and port.
  - ```csharp
    IPEndPoint endPoint = server.LocalEndPoint;
    Console.WriteLine($"Server local end point: {endPoint}");
    ```
    
- `bool EnableBroadcast` `Read only`
  - Indicates if broadcast messages can be sent.
  - Enabling the broadcast is available in the parameters of the "Start()" method.
  - ```csharp
    if (server.EnableBroadcast)
    {
        Console.WriteLine($"Broadcasting is enabled");
    }
    ```
    
- `ushort ReceiveBufferSize`
  - The buffer size in bytes for receiving packets.
  - The default value is 2048.
  - ```csharp
    server.ReceiveBufferSize = 2048;
    ```
    
- `int AvailablePackages`
  - The number of packets waiting to be processed by the `Receive` method.
  - ```csharp
    int pendingPackets = server.AvailablePackages;
    Console.WriteLine($"Pending packets: {pendingPackets}");
    ```
    
- `bool SocketPoll`
  - Polling status of the socket. More info: [Socket.Poll](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.poll?view=net-8.0) reference.
  - Important: use this option only to process Receive in a dedicated thread.
  - ```csharp
    if (server.SocketPoll)
    {
        server.Receive();
    }
    ```
  
### Methods

- `void Start(ushort port = 0, bool enableBroadcast = false)`
  - Important: Must be called before any other operation. Starts listening on the specified port with optional broadcast capability.
  - ```csharp
    server.Start(8080, true);
    ```

- `void Stop()`
  - Stops listening and closes the socket.
  - ```csharp
    server.Stop();
    ```
    
- `void Receive()`
  - Receives and processes all pending packets.
  - Important: must be called in an infinite loop or in updates to constantly receive notifications if they have been received.
  - ```csharp
    server.Receive();
    ```
    
- `void UpdatePeer(uint deltaTime)`
  - Updates timers and disconnects peers as necessary.
  - Important: must be called in an infinite loop or in updates to process received messages or status.
  - Important: this method is a combination of the UpdateTimer, UpdateDisconnecting methods, so do not call them because conflicts will occur.
  - Important: the number is specified in milliseconds.
  - ```csharp
    server.UpdatePeer(10);
    ```
- `void UpdateTimer(uint deltaTime)`
  - Updates only the timers.
  - Important: the number is specified in milliseconds.
  - ```csharp
    server.UpdateTimer(10);
    ```
    
- `void UpdateDisconnecting()`
  - Updates only the disconnecting peers.
  - ```csharp
    server.UpdateDisconnecting();
    ```
    
- `void SendBroadcast(Packet packet, ushort port)`
  - Sends a broadcast message if `EnableBroadcast` is `true`.
  - ```csharp
    Packet packet = Packet.Write();
    client.SendBroadcast(packet, 8080);
    ```
    
- `void SendUnconnected(Packet packet, EndPoint endPoint)`
  - Sends a message without establishing a connection.
  - ```csharp
    Packet packet = Packet.Write();
    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);
    server.SendUnconnected(packet, remoteEndPoint);
    ```
  
## Server

The `Server` class has additional properties, methods, and events specific to server functionality:

### Events

- `Action OnStart`
  - Invoked when the server starts.
  - ```csharp
    server.OnStart += HandleServerStart;

    private void HandleServerStart()
    {
        Console.WriteLine("Server started.");
    }
    ```
    
- `Action OnStop`
  - Invoked when the server stops.
  - ```csharp
    server.OnStop += HandleServerStop;

    private void HandleServerStop()
    {
        Console.WriteLine("Server stopped.");
    }
    ```
    
- `Action<Peer> OnConnected`
  - Invoked when a peer connects.
  - ```csharp
    server.OnConnected += HandlePeerConnected;

    private void HandlePeerConnected(Peer peer)
    {
        Console.WriteLine($"Peer connected: {peer.EndPoint}");
    }
    ```
    
- `Action<Peer> OnDisconnected`
  - Invoked when a peer disconnects.
  - ```csharp
    server.OnDisconnected += HandlePeerDisconnected;

    private void HandlePeerDisconnected(Peer peer)
    {
        Console.WriteLine($"Peer disconnected: {peer.EndPoint}");
    }
    ```
    
- `Action<Packet, Peer> OnReceiveReliable`
  - Invoked when a reliable message is received.
  - ```csharp
    server.OnReceiveReliable += HandleReceiveReliable;

    private void HandleReceiveReliable(Packet packet, Peer peer)
    {
        Console.WriteLine("Reliable message received.");
    }
    ```
    
- `Action<Packet, Peer> OnReceiveUnreliable`
  - Invoked when an unreliable message is received.
  - ```csharp
    server.OnReceiveUnreliable += HandleReceiveUnreliable;

    private void HandleReceiveUnreliable(Packet packet, Peer peer)
    {
        Console.WriteLine("Unreliable message received.");
    }
    ```
    
- `Action<Packet, EndPoint> OnReceiveBroadcast`
  - Invoked when a broadcast message is received.
  - ```csharp
    server.OnReceiveBroadcast += HandleReceiveBroadcast;

    private void HandleReceiveBroadcast(Packet packet, EndPoint endPoint)
    {
        Console.WriteLine("Broadcast message received.");
    }
    ```
    
- `Action<Packet, EndPoint> OnReceiveUnconnected`
  - Invoked when a message is received from an unconnected host.
  - ```csharp
    server.OnReceiveUnconnected += HandleReceiveUnconnected;

    private void HandleReceiveUnconnected(Packet packet, EndPoint endPoint)
    {
        Console.WriteLine("Unconnected message received.");
    }
    ```
  
### Methods

- `void SendAllReliable(Packet packet, Peer ignore = null)`
  - Sends a reliable message to all peers, optionally ignoring one.
  - ```csharp
    Packet packet = Packet.Write();
    server.SendAllReliable(packet, peer);
    ```
    
- `void SendAllUnreliable(Packet packet, Peer ignore = null)`
  - Sends an unreliable message to all peers, optionally ignoring one.
  - ```csharp
    Packet packet = Packet.Write();
    server.SendAllUnreliable(packet, peer);
    ```
  
## Client

The `Client` class has properties, methods, and events specific to client functionality:

### Properties

- `uint Rtt` `Read only`
  - The round-trip time, measured every 500 milliseconds to check for connection status.
  - ```csharp
    uint rtt = client.Rtt;
    Console.WriteLine($"Round-trip time: {rtt}");
    ```
    
- `State State` `Read only`
  - The connection state of the client.
  - ```csharp
    State state = client.State;
    Console.WriteLine($"Client state: {state}");
    ```
  
### Events

- `Action OnStart`
  - Invoked when the client starts.
  - ```csharp
    client.OnStart += HandleClientStart;

    private void HandleClientStart()
    {
        Console.WriteLine("Client started.");
    }
    ```
    
- `Action OnStop`
  - Invoked when the client stops.
  - ```csharp
    client.OnStop += HandleClientStop;

    private void HandleClientStop()
    {
        Console.WriteLine("Client stopped.");
    }
    ```
    
- `Action OnConnected`
  - Invoked when the client successfully connects to the server.
  - ```csharp
    client.OnConnected += HandleClientConnected;

    private void HandleClientConnected()
    {
        Console.WriteLine("Connected to server.");
    }
    ```
    
- `Action OnDisconnected`
  - Invoked when the client disconnects.
  - ```csharp
    client.OnDisconnected += HandleClientDisconnected;

    private void HandleClientDisconnected()
    {
        Console.WriteLine("Disconnected from server.");
    }
    ```
  
- `Action<Packet> OnReceiveReliable`
  - Invoked when a reliable message is received.
  - ```csharp
    client.OnReceiveReliable += HandleReceiveReliable;

    private void HandleReceiveReliable(Packet packet)
    {
        Console.WriteLine("Reliable message received.");
    }
    ```
    
- `Action<Packet> OnReceiveUnreliable`
  - Invoked when an unreliable message is received.
  - ```csharp
    client.OnReceiveUnreliable += HandleReceiveUnreliable;

    private void HandleReceiveUnreliable(Packet packet)
    {
        Console.WriteLine("Unreliable message received.");
    }
    ```
    
- `Action<Packet, EndPoint> OnReceiveBroadcast`
  - Invoked when a broadcast message is received.
  - ```csharp
    client.OnReceiveBroadcast += HandleReceiveBroadcast;

    private void HandleReceiveBroadcast(Packet packet, EndPoint endPoint)
    {
        Console.WriteLine("Broadcast message received.");
    }
    ```
    
- `Action<Packet, EndPoint> OnReceiveUnconnected`
  - Invoked when a message is received from an unconnected host.
  - ```csharp
    client.OnReceiveUnconnected += HandleReceiveUnconnected;

    private void HandleReceiveUnconnected(Packet packet, EndPoint endPoint)
    {
        Console.WriteLine("Unconnected message received.");
    }
    ```
  
### Methods

- `void Connect(string ipAddress, ushort port)`
  - `Important:` Must be called after `Start()` to establish a connection. Connects to a server.
  - ```csharp
    client.Connect("127.0.0.1", 8080);
    ```
    
- `void Disconnect()`
  - Disconnects from the server and waits for a response.
  - ```csharp
    client.Disconnect();
    ```
    
- `void Disconnected()`
  - Immediately disconnects without notifying the server or stops any ongoing connection/disconnection actions.
  - ```csharp
    client.Disconnected();
    ```
  
- `void SendReliable(Packet packet)`
  - Sends a reliable message to the server.
  - ```csharp
    Packet packet = Packet.Write();
    client.SendReliable(packet);
    ```
    
- `void SendUnreliable(Packet packet)`
  - Sends an unreliable message to the server.
  - ```csharp
    Packet packet = Packet.Write();
    client.SendUnreliable(packet);
    ```
  
## Peer

The `Peer` class is used in conjunction with the `Server` class for managing connected clients:

### Properties

- `uint Rtt` `Read only`
  - The round-trip time, measured every 500 milliseconds to check for connection status.
  - ```csharp
    uint rtt = peer.Rtt;
    Console.WriteLine($"Peer round-trip time: {rtt}");
    ```
    
- `State State` `Read only`
  - The connection state of the peer.
  - ```csharp
    State state = peer.State;
    Console.WriteLine($"Peer state: {state}");
    ```

- `EndPoint EndPoint` `Read only`
  - The end point of this peer.
  - ```csharp
    Console.WriteLine($"Peer EndPoint: {peer.EndPoint}");
    ```
  
### Methods

- `void Disconnect()`
  - Disconnects from the client and waits for a response.
  - ```csharp
    peer.Disconnect();
    ```
    
- `void Disconnected()`
  - Immediately disconnects without notifying the client or stops any ongoing connection/disconnection actions.
  - ```csharp
    peer.Disconnected();
    ```
  
- `void UpdateTimer(uint deltaTime)`
  - Updates the timers for this peer.
  - Important: call it when you need to split peer updates across threads and do not call it when UpdatePeer or UpdateTimer is called because there will be conflicts.
  - ```csharp
    peer.UpdateTimer(10);
    ```
    
- `void SendReliable(Packet packet)`
  - Sends a reliable message to this peer.
  - ```csharp
    Packet packet = Packet.Write();
    client.SendReliable(packet);
    ```
    
- `void SendUnreliable(Packet packet)`
  - Sends an unreliable message to this peer.
  - ```csharp
    Packet packet = Packet.Write();
    client.SendUnreliable(packet);
    ```
  
# Packet

The `Packet` class in the SimpleUDP library provides functionality to create, write, read, and manage packets of data. It supports various data types and ensures efficient handling of network data.

## Properties

- `int Offset`
  - Gets the current read offset in the packet.
- `int Length`
  - Gets the current write offset in the packet.
- `const ushort MaxSizeData = 1432`
  - The maximum size of data in the packet.
- `byte[] Data`
  - The byte array that holds the packet data.

## Static Methods

- `static Packet.Write(ushort maxSizeData = MaxSizeData)`
  - Creates a new writable packet with the specified maximum data size.
  - ```csharp
    Packet packet = Packet.Write();
    ```

### Byte

- `static Packet Byte(byte value, ushort maxSizeData = 64)`
  - Creates a new packet and writes a byte value.
  - ```csharp
    Packet packet = Packet.Byte(1);
    ```
- `Packet Byte(byte value)`
  - Writes a byte value to the packet.
  - ```csharp
    packet.Byte(1);
    ```
- `byte Byte()`
  - Reads a byte value from the packet.
  - ```csharp
    byte value = packet.Byte();
    ```

### SByte

- `static Packet SByte(sbyte value, ushort maxSizeData = 64)`
  - Creates a new packet and writes an sbyte value.
  - ```csharp
    Packet packet = Packet.SByte(-1);
    ```
- `Packet SByte(sbyte value)`
  - Writes an sbyte value to the packet.
  - ```csharp
    packet.SByte(-1);
    ```
- `sbyte SByte()`
  - Reads an sbyte value from the packet.
  - ```csharp
    sbyte value = packet.SByte();
    ```

### Short

- `static Packet Short(short value, ushort maxSizeData = 64)`
  - Creates a new packet and writes a short value.
  - ```csharp
    Packet packet = Packet.Short(123);
    ```
- `Packet Short(short value)`
  - Writes a short value to the packet.
  - ```csharp
    packet.Short(123);
    ```
- `short Short()`
  - Reads a short value from the packet.
  - ```csharp
    short value = packet.Short();
    ```

### UShort

- `static Packet UShort(ushort value, ushort maxSizeData = 64)`
  - Creates a new packet and writes a ushort value.
  - ```csharp
    Packet packet = Packet.UShort(123);
    ```
- `Packet UShort(ushort value)`
  - Writes a ushort value to the packet.
  - ```csharp
    packet.UShort(123);
    ```
- `ushort UShort()`
  - Reads a ushort value from the packet.
  - ```csharp
    ushort value = packet.UShort();
    ```

### Int

- `static Packet Int(int value, ushort maxSizeData = 64)`
  - Creates a new packet and writes an int value.
  - ```csharp
    Packet packet = Packet.Int(123);
    ```
- `Packet Int(int value)`
  - Writes an int value to the packet.
  - ```csharp
    packet.Int(123);
    ```
- `int Int()`
  - Reads an int value from the packet.
  - ```csharp
    int value = packet.Int();
    ```

### UInt

- `static Packet UInt(uint value, ushort maxSizeData = 64)`
  - Creates a new packet and writes a uint value.
  - ```csharp
    Packet packet = Packet.UInt(123U);
    ```
- `Packet UInt(uint value)`
  - Writes a uint value to the packet.
  - ```csharp
    packet.UInt(123U);
    ```
- `uint UInt()`
  - Reads a uint value from the packet.
  - ```csharp
    uint value = packet.UInt();
    ```

### Long

- `static Packet Long(long value, ushort maxSizeData = 64)`
  - Creates a new packet and writes a long value.
  - ```csharp
    Packet packet = Packet.Long(123L);
    ```
- `Packet Long(long value)`
  - Writes a long value to the packet.
  - ```csharp
    packet.Long(123L);
    ```
- `long Long()`
  - Reads a long value from the packet.
  - ```csharp
    long value = packet.Long();
    ```

### ULong

- `static Packet ULong(ulong value, ushort maxSizeData = 64)`
  - Creates a new packet and writes a ulong value.
  - ```csharp
    Packet packet = Packet.ULong(123UL);
    ```
- `Packet ULong(ulong value)`
  - Writes a ulong value to the packet.
  - ```csharp
    packet.ULong(123UL);
    ```
- `ulong ULong()`
  - Reads a ulong value from the packet.
  - ```csharp
    ulong value = packet.ULong();
    ```

### Float

- `static Packet Float(float value, ushort maxSizeData = 64)`
  - Creates a new packet and writes a float value.
  - ```csharp
    Packet packet = Packet.Float(123.45f);
    ```
- `Packet Float(float value)`
  - Writes a float value to the packet.
  - ```csharp
    packet.Float(123.45f);
    ```
- `float Float()`
  - Reads a float value from the packet.
  - ```csharp
    float value = packet.Float();
    ```

### Double

- `static Packet Double(double value, ushort maxSizeData = 64)`
  - Creates a new packet and writes a double value.
  - ```csharp
    Packet packet = Packet.Double(123.45);
    ```
- `Packet Double(double value)`
  - Writes a double value to the packet.
  - ```csharp
    packet.Double(123.45);
    ```
- `double Double()`
  - Reads a double value from the packet.
  - ```csharp
    double value = packet.Double();
    ```

### String

- `static Packet String(string value, ushort maxSizeData = 256)`
  - Creates a new packet and writes a string value.
  - ```csharp
    Packet packet = Packet.String("Hello, world!");
    ```
- `Packet String(string value)`
  - Writes a string value to the packet.
  - ```csharp
    packet.String("Hello, world!");
    ```
- `string String()`
  - Reads a string value from the packet.
  - ```csharp
    string value = packet.String();
    ```

## License

This library is available under the MIT License. See the [LICENSE](LICENSE) file for more information.

---

For more details, examples, and updates, visit the [GitHub repository](https://github.com/StrumDev/SimpleUDP) and the [Unity Asset Store page](https://assetstore.unity.com).