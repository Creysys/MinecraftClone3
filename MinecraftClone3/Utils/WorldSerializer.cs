using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using MinecraftClone3.Blocks;

namespace MinecraftClone3.Utils
{
    internal class WorldSerializer
    {
        /*
         * Region file format
         * Header
         *   Header-Length (int)
         *   4194304 bytes 128*128*128*(sizeof(uint)+sizeof(int)) chunkPositionOffset, length (gzip)
         * data (gzip)
         * 
         * sizeof(int) + header-length + chunkPositionOffset = chunkPositionInFile
         */

        public const string WorldFolder = "World";
        public const string RegionsFolder = "Regions";

        public static void SaveRegion(World world, Vector3i region)
        {
            var file = new FileInfo(Path.Combine(WorldFolder, RegionsFolder, GetRegionFilename(region)));
            // ReSharper disable once PossibleNullReferenceException
            file.Directory.Create();

            byte[] headerBytes;
            byte[] dataBytes;

            using (var headerStream = new MemoryStream())
            using (var dataStream = new MemoryStream())
            {
                using (var headerWriter = new BinaryWriter(new GZipStream(headerStream, CompressionMode.Compress)))
                using (var dataWriter = new BinaryWriter(dataStream))
                {
                    var chunkMinPos = World.ChunkInWorld(region * World.RegionSize);
                    for (var x = 0; x < World.ChunksPerRegion; x++)
                        for (var y = 0; y < World.ChunksPerRegion; y++)
                            for (var z = 0; z < World.ChunksPerRegion; z++)
                                if (world.LoadedChunks.TryGetValue(chunkMinPos + new Vector3i(x, y, z), out Chunk chunk))
                                {
                                    var lastPos = dataStream.Position;

                                    byte[] chunkCompressedBytes;
                                    using (var chunkDataStream = new MemoryStream())
                                    {
                                        using (var chunkDataWriter = new BinaryWriter(new GZipStream(chunkDataStream, CompressionMode.Compress)))
                                            chunk.Write(chunkDataWriter);

                                        chunkCompressedBytes = chunkDataStream.ToArray();
                                    }
                                    dataWriter.Write(chunkCompressedBytes);

                                    headerWriter.Write((uint)lastPos);
                                    headerWriter.Write((int)(dataStream.Position - lastPos));
                                }
                                else
                                {
                                    headerWriter.Write(uint.MaxValue);
                                    headerWriter.Write(int.MinValue);
                                }
                }

                dataBytes = dataStream.ToArray();
                headerBytes = headerStream.ToArray();
            }

            using (var writer = new BinaryWriter(file.Create()))
            {
                writer.Write(headerBytes.Length);
                writer.Write(headerBytes);
                writer.Write(dataBytes);
            }
        }

        public static CachedChunk LoadChunk(World world, Vector3i position)
        {
            var file =
                new FileInfo(Path.Combine(WorldFolder, RegionsFolder,
                    GetRegionFilename(World.ChunkToRegion(position))));

            if (!file.Exists) return null;

            using (var reader = new BinaryReader(file.OpenRead()))
            {
                var headerLength = reader.ReadInt32();
                var headerBytesCompressed = reader.ReadBytes(headerLength);

                uint chunkPositionOffset;
                int chunkLengthInFile;

                using (var headerStream = new MemoryStream())
                {
                    using (
                        var headerCompressedStream = new GZipStream(new MemoryStream(headerBytesCompressed),
                            CompressionMode.Decompress))
                    {
                        headerCompressedStream.CopyTo(headerStream);
                    }

                    using (var headerReader = new BinaryReader(headerStream))
                    {
                        var chunkInRegion = World.ChunkInRegion(position);
                        var chunkPositionInHeader = (chunkInRegion.X * World.ChunksPerRegion*World.ChunksPerRegion +
                                                     chunkInRegion.Y * World.ChunksPerRegion + chunkInRegion.Z) *
                                                    sizeof(uint) * 2;

                        if (chunkPositionInHeader >= headerStream.Length) return null;

                        headerStream.Seek(chunkPositionInHeader, SeekOrigin.Begin);
                        chunkPositionOffset = headerReader.ReadUInt32();
                        if (chunkPositionOffset == uint.MaxValue) return null;
                        chunkLengthInFile = headerReader.ReadInt32();
                    }
                }

                reader.BaseStream.Seek(sizeof(int) + headerLength + chunkPositionOffset, SeekOrigin.Begin);
                var chunkDataCompressed = reader.ReadBytes(chunkLengthInFile);

                using (var chunkDataReader = new BinaryReader(new GZipStream(new MemoryStream(chunkDataCompressed), CompressionMode.Decompress)))
                    return new CachedChunk(world, position, chunkDataReader);
            }
        }

        private static string GetRegionFilename(Vector3i region) => $"{region.X} {region.Y} {region.Z}";
    }
}