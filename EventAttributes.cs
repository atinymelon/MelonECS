using System;

namespace MelonECS
{
    public class ReadEventsAttribute : Attribute
    {
        public Type[] types;

        public ReadEventsAttribute(params Type[] types)
            => this.types = types;
    }

    public class WriteEventsAttribute : Attribute
    {
        public Type[] types;

        public WriteEventsAttribute(params Type[] types)
            => this.types = types;
    }
}