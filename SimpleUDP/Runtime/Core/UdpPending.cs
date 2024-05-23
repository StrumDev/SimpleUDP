// This file is provided under The MIT License as part of SimpleUDP.
// Copyright (c) StrumDev
// For additional information please see the included LICENSE.md file or view it on GitHub:
// https://github.com/StrumDev/SimpleUDP/blob/main/LICENSE

using System;

namespace SimpleUDP.Core.Net
{
    public class UdpPending
    {
        internal uint attempt { get; private set; }
        internal uint sendTimer { get; private set; }

        internal byte[] buffer { get; private set; }
        internal Action<byte[]> RawSend { get; private set; }
        
        internal UdpPending(Action<byte[]> rawSend)
        {
            RawSend = rawSend;
        }

        internal void SendPacket(params byte[] packet)
        {
            attempt++;
            RawSend(buffer = packet);
        }

        internal void UpdateTimer(uint deltaTime, uint interval)
        {
            if (buffer != null)
            {
                sendTimer += deltaTime;

                if (sendTimer >= interval)
                {
                    sendTimer = 0;
                    SendPacket(buffer);
                }
            }
        }

        internal void ClearPacket()
        {
            attempt = 0;
            sendTimer = 0;
            buffer = null;
        }
    }
}