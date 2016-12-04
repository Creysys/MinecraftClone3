using System;
using System.IO;
using MinecraftClone3.Blocks;

namespace MinecraftClone3.Utils
{
    internal class WorldSerializer
    {
        public const string WorldFolder = "World";
        public const string RegionsFolder = "Regions";

        public static void SaveRegion(World world, Vector3i region)
        {
            var file = new FileInfo(Path.Combine(WorldFolder, RegionsFolder, GetRegionFilename(region)));
            // ReSharper disable once PossibleNullReferenceException
            file.Directory.Create();

            using (var writer = new BinaryWriter(file.Create()))
            {
                var count = 0;
                writer.Seek(sizeof(int), SeekOrigin.Begin);
                var chunkMinPos = World.ChunkInWorld(region * World.RegionSize);
                for (var x = 0; x < World.ChunksPerRegion; x++)
                    for (var y = 0; y < World.ChunksPerRegion; y++)
                        for (var z = 0; z < World.ChunksPerRegion; z++)
                        {
                            var key = chunkMinPos + new Vector3i(x, y, z);
                            if (!world.LoadedChunks.TryGetValue(key, out Chunk chunk)) continue;

                            writer.Write(key.X);
                            writer.Write(key.Y);
                            writer.Write(key.Z);
                            chunk.Write(writer);

                            count++;
                        }
                writer.Seek(0, SeekOrigin.Begin);
                writer.Write((uint)count);
            }
        }

        public static bool LoadRegion(ChunkCache cache, Vector3i region)
        {
            var file = new FileInfo(Path.Combine(WorldFolder, RegionsFolder, GetRegionFilename(region)));
            if (!file.Exists) return false;

            using (var reader = new BinaryReader(file.OpenRead()))
            {
                var count = reader.ReadUInt32();
                for (var i = 0; i < count; i++)
                {
                    var chunkPos = new Vector3i(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
                    var chunk = new CachedChunk(cache.World, chunkPos, reader);
                    cache.AddChunk(chunk);
                }
            }

            return true;
        }


        private static string GetRegionFilename(Vector3i region) => $"{region.X} {region.Y} {region.Z}";
    }
}