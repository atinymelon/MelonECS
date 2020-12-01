namespace MelonECS
{
    internal static class MessageIndex
    {
        internal static int count;
    }
    
    internal static class MessageType<T> where T : struct, IMessage
    {
        internal static readonly int Index;

        static MessageType()
        {
            Index = ++MessageIndex.count;
        }
    }
}