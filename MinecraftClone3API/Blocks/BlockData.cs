using System.IO;

namespace MinecraftClone3API.Blocks
{
    public abstract class BlockData
    {
        public abstract void Serialize(BinaryWriter writer);
        public abstract void Deserialize(BinaryReader reader);
    }
}
