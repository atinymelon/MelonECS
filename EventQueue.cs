using System;

namespace MelonECS
{
    public interface IEventQueue
    {
        void Clear();
        
        int Count { get; }
    } 
    
    public class EventQueue<T> : IEventQueue where T : struct
    {
        public int Count { get; private set; }
        
        private T[] events = new T[1];

        public void Push(T evt)
        {
            if (Count >= events.Length)
            {
                var temp = new T[events.Length * 2];
                Array.Copy(events, temp, events.Length);
                events = temp;
            }

            events[Count] = evt;
            Count++;
        }

        public void Clear()
        {
            for (int i = 0; i < Count; i++)
                events[i] = default;
            Count = 0;
        }

        public ArrayRef<T> Read() => new ArrayRef<T>(events, 0, Count);
    }
}