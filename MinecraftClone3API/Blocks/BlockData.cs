using System;
using System.IO;
using MinecraftClone3API.Util;

namespace MinecraftClone3API.Blocks
{
    public abstract class BlockData
    {
        internal static void WriteToStream(BlockData blockData, BinaryWriter writer)
        {
            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                    try
                    {
                        blockData.Serialize(bw);
                    }
                    catch (Exception e)
                    {
                        Logger.Error("Error during serialization of " + blockData);
                        Logger.Exception(e);
                    }

                bytes = ms.ToArray();
            }

            writer.Write(GameRegistry.GetBlockDataRegistryKey(blockData));
            writer.Write((ushort)bytes.Length);
            writer.Write(bytes);
        }

        internal static BlockData ReadFromStream(BinaryReader reader)
        {
            var blockDataRegistryKey = reader.ReadString();
            var bytesLength = reader.ReadUInt16();
            var bytes = reader.ReadBytes(bytesLength);

            var entry = GameRegistry.BlockDataRegistry[blockDataRegistryKey];
            var blockData = (BlockData)Activator.CreateInstance(entry.Type);

            using (var ms = new MemoryStream(bytes))
            using (var br = new BinaryReader(ms))
                blockData.Deserialize(br);

            return blockData;
        }

        public abstract void Serialize(BinaryWriter writer);
        public abstract void Deserialize(BinaryReader reader);
    }
}
