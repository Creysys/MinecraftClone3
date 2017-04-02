using System.IO;
using MinecraftClone3API.Blocks;

namespace VanillaPlugin.BlockDatas
{
    public class BlockDataMetadata : BlockData
    {
        public int Metadata;

        public BlockDataMetadata()
        {
        }

        public BlockDataMetadata(int metadata)
        {
            Metadata = metadata;
        }

        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(Metadata);
        }

        public override void Deserialize(BinaryReader reader)
        {
            Metadata = reader.ReadInt32();
        }
    }
}
