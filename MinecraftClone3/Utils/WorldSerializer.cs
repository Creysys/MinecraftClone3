using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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

        private static readonly object _lockObject = new object();


        public static void SaveRegion(World world, Vector3i region)
        {
            var file = new FileInfo(Path.Combine(WorldFolder, RegionsFolder, GetRegionFilename(region)));
            // ReSharper disable once PossibleNullReferenceException
            file.Directory.Create();

            var existingChunks = new Dictionary<Vector3i, byte[]>();
            lock(_lockObject)
                if (file.Exists) LoadExistingChunks(file, existingChunks);

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
                    {
                        var chunkPos = chunkMinPos + new Vector3i(x, y, z);
                        if (world.LoadedChunks.TryGetValue(chunkPos, out Chunk chunk))
                        {
                            var lastPos = dataStream.Position;

                            WriteChunkCompressed(chunk, dataWriter);

                            headerWriter.Write((uint) lastPos);
                            headerWriter.Write((int) (dataStream.Position - lastPos));
                        }
                        else if (existingChunks.TryGetValue(chunkPos, out byte[] chunkData))
                        {
                            var lastPos = dataStream.Position;

                            dataWriter.Write(chunkData);

                            headerWriter.Write((uint) lastPos);
                            headerWriter.Write((int) (dataStream.Position - lastPos));
                        }
                        else
                        {
                            headerWriter.Write(uint.MaxValue);
                            headerWriter.Write(int.MinValue);
                        }
                    }
                }

                dataBytes = dataStream.ToArray();
                headerBytes = headerStream.ToArray();
            }


            lock(_lockObject)
            using (var writer = new BinaryWriter(file.Create()))
            {
                writer.Write(headerBytes.Length);
                writer.Write(headerBytes);
                writer.Write(dataBytes);
            }
        }

        public static CachedChunk LoadChunk(World world, Vector3i chunkPos)
        {
            var file =
                new FileInfo(Path.Combine(WorldFolder, RegionsFolder,
                    GetRegionFilename(World.ChunkToRegion(chunkPos))));

            lock (_lockObject)
            {
                if (!file.Exists) return null;
                using (var reader = new BinaryReader(file.OpenRead()))
                {
                    var header = ReadCompressedHeader(reader);
                    var chunkDataCompressed = ReadCompressedChunkData(chunkPos, header, reader);
                    if (chunkDataCompressed == null) return null;

                    using (
                        var chunkDataReader =
                            new BinaryReader(new GZipStream(new MemoryStream(chunkDataCompressed),
                                CompressionMode.Decompress)))
                        return new CachedChunk(world, chunkPos, chunkDataReader);
                }
            }
        }


        private class Header
        {
            public readonly int CompressedLength;
            public readonly byte[] Data;

            public Header(int compressedLength, byte[] data)
            {
                CompressedLength = compressedLength;
                Data = data;
            }
        }

        private static void WriteChunkCompressed(Chunk chunk, BinaryWriter dataWriter)
        {
            byte[] chunkCompressedBytes;
            using (var chunkDataStream = new MemoryStream())
            {
                using (var chunkDataWriter = new BinaryWriter(new GZipStream(chunkDataStream, CompressionMode.Compress)))
                    chunk.Write(chunkDataWriter);

                chunkCompressedBytes = chunkDataStream.ToArray();
            }
            dataWriter.Write(chunkCompressedBytes);
        }

        private static Header ReadCompressedHeader(BinaryReader reader)
        {
            var headerLength = reader.ReadInt32();
            var headerBytesCompressed = reader.ReadBytes(headerLength);

            using (var headerStream = new MemoryStream())
            {
                using (
                    var headerCompressedStream = new GZipStream(new MemoryStream(headerBytesCompressed),
                        CompressionMode.Decompress))
                {
                    headerCompressedStream.CopyTo(headerStream);
                }

                return new Header(headerLength, headerStream.ToArray());
            }
        }

        private static byte[] ReadCompressedChunkData(Vector3i chunkPos, Header header, BinaryReader reader)
        {
            var chunkInRegion = World.ChunkInRegion(chunkPos);
            var chunkPositionInHeader = (chunkInRegion.X * World.ChunksPerRegion * World.ChunksPerRegion +
                                         chunkInRegion.Y * World.ChunksPerRegion + chunkInRegion.Z) *
                                        sizeof(uint) * 2;

            if (chunkPositionInHeader >= header.Data.Length) return null;

            var chunkPositionOffset = BitConverter.ToUInt32(header.Data, chunkPositionInHeader);
            if (chunkPositionOffset == uint.MaxValue) return null;
            var chunkLengthInFile = BitConverter.ToInt32(header.Data, chunkPositionInHeader + sizeof(uint));
            

            reader.BaseStream.Seek(sizeof(int) + header.CompressedLength + chunkPositionOffset, SeekOrigin.Begin);
            return reader.ReadBytes(chunkLengthInFile);
        }

        private static void LoadExistingChunks(FileInfo file, Dictionary<Vector3i, byte[]> existingChunks)
        {
            using (var reader = new BinaryReader(file.OpenRead()))
            {
                var header = ReadCompressedHeader(reader);

                for (var x = 0; x < World.ChunksPerRegion; x++)
                for (var y = 0; y < World.ChunksPerRegion; y++)
                for (var z = 0; z < World.ChunksPerRegion; z++)
                {
                    var chunkPos = new Vector3i(x, y, z);
                    var chunkDataCompressed = ReadCompressedChunkData(chunkPos, header, reader);
                    if (chunkDataCompressed == null) continue;

                    existingChunks.Add(chunkPos, chunkDataCompressed);
                }
            }
        }

        private static string GetRegionFilename(Vector3i region) => $"{region.X} {region.Y} {region.Z}";
    }
}