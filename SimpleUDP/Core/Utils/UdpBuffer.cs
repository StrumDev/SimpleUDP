using System.Collections.Generic;

namespace SimpleUDP.Utils
{
    public class UdpBuffer<T>
    {
        public int Count { get; private set; }

        internal readonly ushort MaxQueue = 16;
        internal readonly ushort Capacity = 256;
        
        private Queue<T>[] queues;
        private int queueWrite, queueRead;

        private object locker = new object();

        public UdpBuffer(ushort maxQueue = 16, ushort capacity = 256)
        {
            queues = new Queue<T>[MaxQueue = maxQueue];

            for (int i = 0; i < MaxQueue; i++)
                queues[i] = new Queue<T>(Capacity = capacity);
        }

        public void AddElement(T value)
        {
            lock (locker)
            {
                if (queueWrite >= MaxQueue)
                    queueWrite = 0;
                
                Count++;
                queues[queueWrite++].Enqueue(value);
            }
        }

        public T GetElement()
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
                else return default;
            }
        }

        public void Clear()
        {
            lock (locker)
            {
                Count = 0;
                queueRead = 0;
                queueWrite = 0;
                
                foreach (Queue<T> queue in queues)
                    queue.Clear();
            }
        }
    }
}