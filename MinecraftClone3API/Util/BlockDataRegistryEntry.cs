using System;

namespace MinecraftClone3API.Util
{
    internal class BlockDataRegistryEntry : RegistryEntry
    {
        public BlockDataRegistryEntry(Type type) : base(type.Name)
        {
        }
    }
}
