namespace MelonECS
{
    internal static class EventIndex
    {
        internal static int count;
    }
    
    internal static class EventType<T> where T : struct, IEvent
    {
        internal static readonly int Index;

        static EventType()
        {
            Index = ++EventIndex.count;
        }
    }
}