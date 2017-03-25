using System.Collections.Generic;
using System.IO;
using MinecraftClone3API.Blocks;

namespace MinecraftClone3API.Util
{
    public static class GameRegistry
    {
        private const string RegistryFilename = "registry.bin";

        internal static readonly BlockRegistry BlockRegistry = new BlockRegistry();
        internal static readonly Registry<BlockDataRegistryEntry> BlockDataRegistry = new Registry<BlockDataRegistryEntry>();

        public static List<string> GetMissingBlocks() => BlockRegistry.GetMissingBlocks();

        public static Block GetBlock(ushort id) => BlockRegistry[id];
        public static Block GetBlock(string key) => BlockRegistry[key];

        public static void Save(DirectoryInfo saveDir)
        {
            var file = new FileInfo(Path.Combine(saveDir.FullName, RegistryFilename));

            using (var writer = new BinaryWriter(file.Create()))
            {
                BlockRegistry.Write(writer);
            }
        }

        public static void Load(DirectoryInfo saveDir)
        {
            var file = new FileInfo(Path.Combine(saveDir.FullName, RegistryFilename));
            if (!file.Exists) return;

            using (var reader = new BinaryReader(file.OpenRead()))
            {
                BlockRegistry.Read(reader);
            }
        }
    }
}