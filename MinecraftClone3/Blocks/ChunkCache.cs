using System.Collections.Generic;
using MinecraftClone3.Utils;

namespace MinecraftClone3.Blocks
{
    internal class ChunkCache
    {
        public readonly World World;

        private readonly Dictionary<Vector3i, CachedChunk> _chunks = new Dictionary<Vector3i, CachedChunk>();

        public ChunkCache(World world)
        {
            World = world;
        }

        public void SetBlock(int x, int y, int z, uint id)
        {
            var chunkInWorld = World.ChunkInWorld(x, y, z);
            var blockInChunk = World.BlockInChunk(x, y, z);

            if(_chunks.TryGetValue(chunkInWorld, out CachedChunk chunk))
                chunk.SetBlock(blockInChunk.X, blockInChunk.Y, blockInChunk.Z, id);
            else
            {
                chunk = new CachedChunk(World, chunkInWorld);
                chunk.SetBlock(blockInChunk.X, blockInChunk.Y, blockInChunk.Z, id);
                _chunks.Add(chunkInWorld, chunk);
            }
        }

        public void AddChunk(CachedChunk chunk) => _chunks.Add(chunk.Position, chunk);

        public void AddToWorldAndUpdate()
        {
            foreach (var entry in _chunks)
                World.LoadedChunks.Add(entry.Key, new Chunk(entry.Value));

            foreach (var entry in _chunks)
            {
                World.QueueChunkUpdate(entry.Key, true);
                World.QueueChunkUpdate(entry.Key + new Vector3i(-1, 0, 0), true);
                World.QueueChunkUpdate(entry.Key + new Vector3i(+1, 0, 0), true);
                World.QueueChunkUpdate(entry.Key + new Vector3i(0, -1, 0), true);
                World.QueueChunkUpdate(entry.Key + new Vector3i(0, +1, 0), true);
                World.QueueChunkUpdate(entry.Key + new Vector3i(0, 0, -1), true);
                World.QueueChunkUpdate(entry.Key + new Vector3i(0, 0, +1), true);
            }
        }
    }
}