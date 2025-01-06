# SimpleUDP Documentation

## Overview
SimpleUDP is a lightweight and easy-to-use UDP networking library suitable for both .NET applications and Unity projects. It supports both server and client functionality, with features for reliable and unreliable messaging.

### The project at this stage is already suitable for use in small commercial applications and games.
 - I don't plan to make any major changes to the functionality in the future, mostly bug fixes and performance improvements.

**Version:** 0.7.0

[General](#General)

[Log](#Log)

[Server](#Server)

[Client](#Client)

[Peer](#Peer)

[Packet](#Packet)

[License](#License)

## General

  The following properties and methods are available in both the `Server` and `Client` classes:

### Properties

- `bool IsRunning` `Read only`
  - Indicates whether the listening is active.

- `ushort LocalPort` `Read only`
  - The local port being listened to.
 
- `IPEndPoint LocalEndPoint` `Read only`
  - The local IP address and port.

- `bool EnableBroadcast` `Read only`
  - Indicates if broadcast messages can be sent.
  - Enabling the broadcast is available in the parameters of the "Start()" method.

- `LimitedSizePackage = true`
  - Limits the maximum packet size for receiving to 1432 bytes.

- `const ushort MaxSizePacket = 1432`
  - The maximum packet size is 1432 bytes..

- `ushort ReceiveBufferSize`
  - The buffer size in bytes for receiving packets.
  - The default value is 2048.
 
- `int AvailablePackages`
  - The number of packets waiting to be processed by the `Receive` method.
 
- `bool SocketPoll`
  - Polling status of the socket. More info: [Socket.Poll](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.poll?view=net-8.0) reference.
  - Important: use this option only to process Receive in a dedicated thread.
 
### Callbacks

- `Action OnStarted`
  - Invoked when the server starts.
 
- `Action OnStopped`
  - Invoked when the server stops.
 
- `Action<EndPoint, byte[]> OnReceiveBroadcast`
  - Invoked when a broadcast message is received.
 
- `Action<EndPoint, byte[]> OnReceiveUnconnected`
  - Invoked when a message is received from an unconnected host.
  - It is not necessary for the client to specify the port as it will be assigned by the system.

### Methods

- `void Start(ushort port, bool enableBroadcast = false)`
  - Important: Must be called before any other operation. Starts listening on the specified port with optional broadcast capability.

- `void Stop()`
  - Stops listening and closes the socket.

- `void Receive()`
  - Receives and processes all pending packets.
  - Important: must be called in an infinite loop or in updates to constantly receive notifications if they have been received.

- `void TickUpdate()`
  - Update the status of connections.
  - Important: must be called in an infinite loop or in updates to process received messages or status.
  - Important: this method is a combination of methods for the UpdateTimer, UpdateDisconnecting server, so do not call them as conflicts will occur.

- `void SendBroadcast(ushort port, byte[] packet, int length)`
  - Sends a broadcast message if `EnableBroadcast` is `true`.

- `void SendUnconnected(EndPoint endPoint, byte[] packet, int length)`
  - Sends a message without establishing a connection.
 
- `void SendBroadcast(ushort port, Packet packet)` `Extensions`
  - Sends a broadcast message if `EnableBroadcast` is `true`.

- `void SendUnconnected(EndPoint endPoint, Packet packet)` `Extensions`
  - Sends a message without establishing a connection.

## Log

  The `UdpLog` class has additional properties, methods, and events specific to server functionality:

### Methods

- `void Initialize(Log logInfo, Log logWarning = null, Log logError = null)`
  - Initializing logging for Server and Client.

## Server

  The `Server` class has additional properties, methods, and events specific to server functionality:

### Properties

- `uint TimeOut = 5000`
  - The time in milliseconds after which you can assume that the client has disconnected from the network and can be disconnected.

- `uint MaxConnections = 256`
  - Maximum number of possible simultaneous connections.

- `string KeyConnection = ""`
  - Server key, when clients try to establish a connection to the server, they must have the correct key for the server to allow the connection.

- `uint MaxNumberId = ushort.MaxValue`
  - Limits the maximum number of identifiers that can be issued.
  - Note: It is desirable that the number of identifiers exceed the maximum number of connections by 2 times to reduce the likelihood of identifier collisions; in the event of a collision, a new available one is searched for.

- `uint ConnectionsCount` `Read only`
  - The current number of connections to the server.

### Callbacks

- `Action<UdpPeer> OnConnected`
  - Called when someone joins the server.
 
- `Action<UdpPeer> OnDisconnected`
  - Called when someone leaves the server.
 
- `Action<UdpPeer, byte[]> OnReceiveReliable`
  - Invoked when a reliable message is received.
 
- `Action<UdpPeer, byte[]> OnReceiveUnreliable`
  - Invoked when an unreliable message is received.

### Methods

- `void Disconnect(uint peerId)`
  - Forcibly disconnect from the client's server.
 
- `void SendReliable(uint peerId, Packet packet)` `Extensions`
  - Sends a reliable message to peer.
 
- `void SendUnreliable(uint peerId, Packet packet)` `Extensions`
  - Sends an unreliable message to peer.

- `void SendReliable(uint peerId, byte[] packet)`
  - Sends a reliable message to peer.
 
- `void SendUnreliable(uint peerId, byte[] packet)`
  - Sends an unreliable message to peer.
 
- `void SendReliable(uint peerId, byte[] packet, int length, int offset = 0)`
  - Sends a reliable message to peer.
 
- `void SendUnreliable(uint peerId, byte[] packet, int length, int offset = 0)`
  - Sends an unreliable message to peer.

- `void SendAllReliable(Packet packet, uint ignoreId = 0)` `Extensions`
  - Sends a reliable message to all peers, optionally ignoring one.

- `void SendAllUnreliable(Packet packet, uint ignoreId = 0)` `Extensions`
  - Sends an unreliable message to all peers, optionally ignoring one.

- `void SendAllReliable(byte[] packet, uint ignoreId = 0)`
  - Sends a reliable message to all peers, optionally ignoring one.

- `void SendAllUnreliable(byte[] packet, uint ignoreId = 0)`
  - Sends an unreliable message to all peers, optionally ignoring one.
 
- `void SendAllReliable(byte[] packet, int length, uint ignoreId = 0)`
  - Sends a reliable message to all peers, optionally ignoring one.

- `void SendAllUnreliable(byte[] packet, int length, uint ignoreId = 0)`
  - Sends an unreliable message to all peers, optionally ignoring one.
 
- `void SendAllReliable(byte[] packet, int length, int offset, uint ignoreId = 0)`
  - Sends a reliable message to all peers, optionally ignoring one.

- `void SendAllUnreliable(byte[] packet, int length, int offset, uint ignoreId = 0)`
  - Sends an unreliable message to all peers, optionally ignoring one.

- `void UpdateTimer(uint deltaTime)`
  - Updates only the timers.

- `void UpdateDisconnecting()`
  - Updates only the disconnecting peers. 

## Client

  The `Client` class has properties, methods, and events specific to client functionality:

- `uint TimeOut = 5000`
  - The time in milliseconds after which the connection to the server can be considered disconnected.

- `string KeyConnection = ""`
  - The client's key must be correct as on the server to attempt to establish a connection.

- `uint Id` `Read only`
  - The client's ID is synchronized with the ID that was issued on the server.

- `uint Rtt` `Read only`
  - The round-trip time, measured every 1000 milliseconds to check for connection status.
 
- `State State` `Read only`
  - The connection state of the client.

- `EndPoint EndPoint` `Read only`
  - IP address of the server

- `Reason ReasonDisconnection` `Read only`
  - The reason why the client was disconnected from the server.

### Callbacks

- `Action OnConnected`
  - Called when the connection is successful.
 
- `Action OnDisconnected`
  - Called in case of disconnection.
 
- `Action<byte[]> OnReceiveReliable`
  - Invoked when a reliable message is received.
 
- `Action<byte[]> OnReceiveUnreliable`
  - Invoked when an unreliable message is received.

### Methods

- `void Connect(string ipAddress, ushort port)` 
  - Important: `Start()` must be called before `Connect()` for a correct connection attempt.
 
- `void Disconnect()`
  - Notifies the server about the disconnection and then disconnects the connection.
 
- `void QuietDisconnect()`
  - Immediately disconnects without notifying the server or stops any ongoing connection/disconnection actions.

- `void SendReliable(Packet packet)` `Extensions`
  - Sends a reliable message to the host.
 
- `void SendUnreliable(Packet packet)` `Extensions`
  - Sends an unreliable message to the host.

- `void SendReliable(byte[] packet)`
  - Sends a reliable message to the server.
 
- `void SendUnreliable(byte[] packet)`
  - Sends an unreliable message to the server.
 
- `void SendReliable(byte[] packet, int length, int offset = 0)`
  - Sends a reliable message to the server.
 
- `void SendUnreliable(byte[] packet, int length, int offset = 0)`
  - Sends an unreliable message to the server.

## Peer

The `Peer` class is used in conjunction with the `Server` class for managing connected clients:

### Properties

- `uint Id` `Read only`
  - The peer's ID.

- `uint Rtt` `Read only`
  - The round-trip time, measured every 1000 milliseconds to check for connection status.
 
- `State State` `Read only`
  - The connection state of the peer's.

- `EndPoint EndPoint` `Read only`
  - IP address of the host
 
- `Reason ReasonDisconnection` `Read only`
  - The reason why the peer was disconnected from the host.

### Methods

- `void Disconnect()`
  - Notifies the host of the disconnection and then disconnects the connection.

- `void QuietDisconnect()`
  - Immediately disconnects without notifying the server or stops any ongoing connection/disconnection actions.

- `void UpdateTimer(uint deltaTime)`
  - Updates the timers for this peer.
  - Important: call it when you need to split peer updates across threads and do not call it when TickUpdate or UpdateTimer is called because there will be conflicts.

- `void SendReliable(Packet packet)` `Extensions`
  - Sends a reliable message to the host.
 
- `void SendUnreliable(Packet packet)` `Extensions`
  - Sends an unreliable message to the host.

- `void SendReliable(byte[] packet)`
  - Sends a reliable message to the host.
 
- `void SendUnreliable(byte[] packet)`
  - Sends an unreliable message to the host.
 
- `void SendReliable(byte[] packet, int length, int offset = 0)`
  - Sends a reliable message to the host.
 
- `void SendUnreliable(byte[] packet, int length, int offset = 0)`
  - Sends an unreliable message to the host.

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

## Methods

- `static Packet.Write(ushort maxSizeData = MaxSizeData)`
  - Creates a new writable packet with the specified maximum data size.
  - ```csharp
    Packet packet = Packet.Write();
    ```

- `static Packet.Read(byte[] packet)`
  - Create a package to read.
  - ```csharp
    void Handler(byte[] packet)
    {
        Packet packet = Packet.Read(packet);
    }
    ```

- `static Packet.Read(byte[] packet, int length, int offset)`
  - Create a package to read.
  - ```csharp
    void Handler(byte[] packet)
    {
        Packet packet = Packet.Read(packet, packet.Length, 0);
    }
    ```

### Bool

- `static Packet Bool(bool value, ushort maxSizeData = 256)`
  - Creates a new packet and writes a bool value.
  - ```csharp
    Packet packet = Packet.Bool(true);
    ```
- `Packet Bool(bool value)`
  - Writes a bool value to the packet.
  - ```csharp
    packet.Bool(true);
    ```
- `bool Bool()`
  - Reads a bool value from the packet.
  - ```csharp
    bool value = packet.Bool();
    ```

### Byte

- `static Packet Byte(byte value, ushort maxSizeData = 256)`
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

- `static Packet SByte(sbyte value, ushort maxSizeData = 256)`
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

- `static Packet Short(short value, ushort maxSizeData = 256)`
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

- `static Packet UShort(ushort value, ushort maxSizeData = 256)`
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

- `static Packet Int(int value, ushort maxSizeData = 256)`
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

- `static Packet UInt(uint value, ushort maxSizeData = 256)`
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

- `static Packet Long(long value, ushort maxSizeData = 256)`
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

- `static Packet ULong(ulong value, ushort maxSizeData = 256)`
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

- `static Packet Float(float value, ushort maxSizeData = 256)`
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

- `static Packet Double(double value, ushort maxSizeData = 256)`
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

### Char

- `static Packet Char(char value, ushort maxSizeData = MaxSizeData)`
  - Creates a new packet and writes a char value.
  - ```csharp
    Packet packet = Packet.Char('C');
    ```
- `Packet Char(char value)`
  - Writes a char value to the packet.
  - ```csharp
    packet.Char('C');
    ```
- `string Char()`
  - Reads a char value from the packet.
  - ```csharp
    char value = packet.Char();
    ```

### String

- `static Packet String(string value, ushort maxSizeData = MaxSizeData)`
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
