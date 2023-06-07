using System;
using System.Net;
using System.Collections.Generic;
using Timer = System.Timers.Timer;

namespace SimpleUDP
{
    public static class Header
    {
        public const byte Connect = 1;
        public const byte Disconnect = 2;
        
        public const byte Unreliable = 3;
        public const byte Reliable = 4;
        public const byte Ack = 5;

        public const byte Ping = 6;
    }
    public delegate void SendTo(byte[] data, EndPoint endPoint);
    public class Reliable
    {
        public const ushort MAX_PENDINGS = 256; // = 1 byte

        private const byte UNRELIABLE_HEADER = 1;
        private const byte RELIABLE_HEADER = 3;

        private const byte HEADER = 0;
        private const byte INDEX = 1;
        private const byte ACK = 2;

        public uint Id;
        public DateTime DateTime;
        public EndPoint EndPoint;

        private SendTo sendTo;
        
        private Pending[] pendings = new Pending[MAX_PENDINGS];
        private byte[] send_ack = new byte[MAX_PENDINGS];
        private byte[] rece_ack = new byte[MAX_PENDINGS];
        private byte index;

        public Reliable(SendTo sendTo, EndPoint endPoint)
        {
            this.sendTo = sendTo;
            this.EndPoint = endPoint;

            for (int i = 0; i < MAX_PENDINGS; i++) pendings[i] = new Pending(sendTo, endPoint);

            DateTime = DateTime.UtcNow;
        }

        public void SendTo(params byte[] data)
        {
            sendTo(data, EndPoint);
        } 
        
        public void SendUnreliable(byte[] data, int length)
        {
            SendTo(AddHeader(data, length, Header.Unreliable));
        }

        public void SendReliable(byte[] data, int length)
        {
            lock (pendings) pendings[++index].Send(AddHeader(data, length, Header.Reliable, index, ++send_ack[index]));
        } 
        
        private byte[] AddHeader(byte[] data, int length, params byte[] header)
        {
            byte[] buffer = new byte[header.Length + length];

            int write = 0;
            
            for (int i = 0; i < header.Length; i++) buffer[write++] = header[i];
            for (int i = 0; i < length; i++) buffer[write++] = data[i];

            return buffer;
        }
        
        public bool IsNewAck(byte[] data)
        {   
            SendTo(Header.Ack, data[INDEX], data[ACK]);

            if (rece_ack[data[INDEX]] != data[ACK])
            {
                rece_ack[data[INDEX]] = data[ACK];
                return true;
            }

            return false;
        }

        public void ClearAck(byte[] data)
        {
            pendings[data[INDEX]].Clear();
        }

        public static byte[] Handler(byte[] data, int length, out bool channel)
        {
            channel = (data[HEADER] == Header.Reliable ? true : false);

            if (data[HEADER] == Header.Unreliable)
                return RemoveHeader(data, length, UNRELIABLE_HEADER);
            
            if (data[HEADER] == Header.Reliable)
                return RemoveHeader(data, length, RELIABLE_HEADER);

            return default;
        }

        private static byte[] RemoveHeader(byte[] data, int length, int offset)
        {
            byte[] buffer = new byte[length - offset];

            int write = 0;
            
            for (int i = offset; i < length; i++) buffer[write++] = data[i];

            return buffer;
        }

        public void Close()
        {
            for (int i = 0; i < MAX_PENDINGS; i++) pendings[i].Close();
        }
    }
    public class Pending
    {
        public uint Interval = 50;
        public ushort MaxAttempts = 16;

        public Action Callback;

        private SendTo sendTo;
        private EndPoint endPoint;

        private Timer timer;
        private ushort attempts;

        private Queue<byte[]> segments = new Queue<byte[]>();

        public Pending(SendTo sendTo, EndPoint endPoint, Action callback = null)
        {
            this.sendTo = sendTo;
            this.endPoint = endPoint;
            this.Callback = callback;

            timer = new Timer();
            timer.Elapsed += (o, e) => RetrySend();
        }

        public void Send(params byte[] data)
        {
            lock (segments)
            {
                if (segments.Count == 0)
                    TrySend(data);
                
                segments.Enqueue(data);
            }
        }

        private void RetrySend()
        {
            lock (segments)
            {
                if (attempts <= MaxAttempts)
                    TrySend(segments.Peek());
                else
                    Clear(true);
            }
        }

        private void TrySend(byte[] data)
        {
            sendTo(data, endPoint);
            attempts++;
            
            timer.Interval = Interval;
            timer.Start();
        }

        public void Clear(bool isCallback = false)
        {
            lock (segments)
            {
                attempts = 0;

                if (segments.Count != 0)
                    segments.Dequeue();

                if (isCallback)
                {
                    Log.Warning("[Packet] failed to confirm");
                    Callback?.Invoke();
                }
                
                if (segments.Count != 0)
                    TrySend(segments.Peek());
                else
                    timer.Stop();
            }
        }

        public void Close()
        {
            lock (segments)
            {
                attempts = 0;
                timer.Stop();
                segments.Clear();
            }
        }
    }
}