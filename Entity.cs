using System;

namespace MelonECS
{
    public readonly struct Entity : IEquatable<Entity>
    {
        public readonly int Index;
        public readonly int Generation;
        public readonly World World;

        public Entity(World world, int index, int generation)
        {
            Index = index;
            Generation = generation;
            World = world;
        }

        public override bool Equals(object obj) => obj is Entity entity && Equals(entity);
        public bool Equals(Entity other) => Index == other.Index && Generation == other.Generation && World == other.World;
        public override int GetHashCode() => Index * 31 + Generation;
        public static bool operator ==(Entity a, Entity b) => a.Index == b.Index && a.Generation == b.Generation;
        public static bool operator !=(Entity a, Entity b) => a.Index != b.Index || a.Generation != b.Generation;

        public override string ToString() => $"Entity[{Index}|{Generation}]";
    }
}