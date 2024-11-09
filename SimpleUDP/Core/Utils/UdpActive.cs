using System;

namespace SimpleUDP
{
    public class UdpActive<T>
    {
        private struct Element
        {
            internal T value;
            internal byte active;
        }

        private byte[] actives;
        private Element[] elements;

        public byte ActiveCount { get; private set; }
        public byte ElementCount { get; private set; }
        
        public UdpActive(byte capacity)
        {
            actives = new byte[capacity];
            elements = new Element[capacity];
        }

        public void AddElement(T value)
        {
            if (ElementCount < elements.Length)
            {
                actives[ElementCount] = ElementCount;
                elements[ElementCount].value = value;

                ElementCount++;
            }
        }

        public bool TrySetActive(out byte index)
        {
            index = 0;

            if (ActiveCount < ElementCount)
            {
                index = actives[ActiveCount];
                elements[index].active = ActiveCount;

                ActiveCount++;
                return true;
            }
            
            return false;
        }

        public void RemoveActive(byte index)
        {
            if (ActiveCount > 0)
            {   
                byte elementActive = elements[index].active;
                byte lastActiveIndex = actives[ActiveCount - 1];

                actives[elementActive] = lastActiveIndex;
                elements[lastActiveIndex].active = elementActive;
                
                elements[index].active = 0;
                actives[ActiveCount - 1] = index;

                ActiveCount--;
            }
        }
        
        public T GetActive(int nextActive)
        {
            if (nextActive >= ActiveCount)
                return default;
            
            return elements[actives[nextActive]].value; 
        }

        public T this[int index]
        {
            get
            {
                if (index >= ElementCount)
                    return default;

                return elements[index].value;    
            }
        }
    }
}