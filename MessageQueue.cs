using System;

namespace MelonECS
{
    public interface IMessageQueue
    {
        void Clear();
    } 
    
    public class MessageQueue<T> : IMessageQueue where T : struct
    {
        private T[] messages = new T[1];
        private int count;

        public void Push(T evt)
        {
            if (count >= messages.Length)
            {
                var temp = new T[messages.Length * 2];
                Array.Copy(messages, temp, messages.Length);
                messages = temp;
            }

            messages[count] = evt;
            count++;
        }

        public void Clear()
        {
            for (int i = 0; i < count; i++)
                messages[i] = default;
            count = 0;
        }

        public Span<T> Read() => new Span<T>(messages, 0, count);
    }
}