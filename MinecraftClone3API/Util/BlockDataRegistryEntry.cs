using System;

namespace MinecraftClone3API.Util
{
    internal class BlockDataRegistryEntry : RegistryEntry, IEquatable<BlockDataRegistryEntry>
    {
        public readonly Type Type;

        public BlockDataRegistryEntry(Type type) : base(type.Name)
        {
            Type = type;
        }

        public override int GetHashCode() => Type.GetHashCode();
        public override bool Equals(object obj)
        {
            var v = obj as BlockDataRegistryEntry;
            return v != null && Equals(this, v);
        }

        public bool Equals(BlockDataRegistryEntry other) => other != null && Type == other.Type;
    }
}
