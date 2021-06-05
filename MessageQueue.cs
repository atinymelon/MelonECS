using System;

namespace MelonECS
{
    public interface IMessageQueue
    {
        void Clear();
        
        int Count { get; }
    } 
    
    public class MessageQueue<T> : IMessageQueue where T : struct
    {
        public int Count { get; private set; }
        
        private T[] messages = new T[1];

        public void Push(T evt)
        {
            if (Count >= messages.Length)
            {
                var temp = new T[messages.Length * 2];
                Array.Copy(messages, temp, messages.Length);
                messages = temp;
            }

            messages[Count] = evt;
            Count++;
        }

        public void Clear()
        {
            for (int i = 0; i < Count; i++)
                messages[i] = default;
            Count = 0;
        }

        public ArrayRef<T> Read() => new ArrayRef<T>(messages, 0, Count);
    }
}