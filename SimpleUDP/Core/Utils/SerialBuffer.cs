using System.Collections.Generic;

namespace SimpleUDP.Utils
{
    public class SerialBuffer
    {
        public int Count { get; private set; }

        internal readonly ushort MaxQueue = 16;
        internal readonly ushort Сapacity = 256;
        
        private Queue<byte[]>[] queues;
        private int queueWrite, queueRead;

        private object locker = new object();

        public SerialBuffer(ushort maxQueue = 16, ushort capacity = 256)
        {
            queues = new Queue<byte[]>[MaxQueue = maxQueue];

            for (int i = 0; i < MaxQueue; i++)
                queues[i] = new Queue<byte[]>(Сapacity = capacity);
        }

        public void AddElement(byte[] data)
        {
            lock (locker)
            {
                if (queueWrite >= MaxQueue)
                    queueWrite = 0;
                
                Count++;
                queues[queueWrite++].Enqueue(data);
            }
        }

        public byte[] GetElement()
        {
            lock (locker)
            {
                if (Count != 0)
                {
                    if (queueRead >= MaxQueue)
                        queueRead = 0;
                    
                    Count--;
                    return queues[queueRead++].Dequeue();
                }
                else return null;
            }
        }

        public void Clear()
        {
            lock (locker)
            {
                Count = 0;
                queueRead = 0;
                queueWrite = 0;
                
                foreach (Queue<byte[]> queue in queues)
                    queue.Clear();
            }
        }
    }
}