namespace SimpleUDP.Core
{   
    internal static class UdpHeader
    {
        public const byte Connect = 1;
        public const byte Connected = 2;

        public const byte Disconnect = 3;
        public const byte Disconnected = 4;

        public const byte Reliable = 5;
        public const byte Unreliable = 6;
        public const byte ReliableAck = 7;
        
        public const byte Ping = 8;
        public const byte Pong = 9;

        public const byte Broadcast = 10;
        public const byte Unconnected = 11;
    }
}