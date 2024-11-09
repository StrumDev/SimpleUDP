using System;

namespace SimpleUDP.Core
{
    public class UdpPending
    {
        private Action<byte[]> rawSend;

        internal uint timer { get; private set; }
        internal byte[] buffer { get; private set; }

        internal UdpPending(Action<byte[]> rawSend)
        {
            this.rawSend = rawSend;
        }

        internal void SendPacket(byte[] packet)
        {
            rawSend(buffer = packet);
        }

        internal void UpdateTimer(uint deltaTime, uint interval)
        {
            if (buffer != null)
            {
                if ((timer += deltaTime) >= interval)
                {
                    timer = 0;
                    rawSend(buffer);
                }
            }
        }

        internal void ClearPacket()
        {
            timer = 0;
            buffer = null;
        }
    }
}