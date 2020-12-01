using System;

namespace MelonECS
{
    public class QueryWithAttribute : Attribute
    {
        public Type[] types;

        public QueryWithAttribute(params Type[] types)
            => this.types = types;
    }

    public class QueryExcludeAttribute : Attribute
    {
        public Type[] types;

        public QueryExcludeAttribute(params Type[] types)
            => this.types = types;
    }
}