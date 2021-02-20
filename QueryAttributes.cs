using System;

namespace MelonECS
{
    public class QueryIncludeAttribute : Attribute
    {
        public Type[] types;

        public QueryIncludeAttribute(params Type[] types)
            => this.types = types;
    }

    public class QueryExcludeAttribute : Attribute
    {
        public Type[] types;

        public QueryExcludeAttribute(params Type[] types)
            => this.types = types;
    }
}