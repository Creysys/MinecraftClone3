using System;
using System.Collections.Generic;
using System.Threading;
using MinecraftClone3.Entities;
using MinecraftClone3.Utils;
using OpenTK;

namespace MinecraftClone3.Blocks
{
    internal class World
    {
        public const int MaxChunkUploads = 1;
        public const int RegionSize = 8 * Chunk.Size;
        public const int RegionSizeSquared = RegionSize * RegionSize;
        public const int ChunksPerRegion = RegionSize / Chunk.Size;
         
        public static readonly int MaxAsyncChunkUpdates = Environment.ProcessorCount*4;

        public static Vector3i RegionInWorld(Vector3i v) => RegionInWorld(v.X, v.Y, v.Z);
        public static Vector3i RegionInWorld(int x, int y, int z) => new Vector3i(
            x < 0 ? (x + 1) / RegionSize - 1 : x / RegionSize,
            y < 0 ? (y + 1) / RegionSize - 1 : y / RegionSize,
            z < 0 ? (z + 1) / RegionSize - 1 : z / RegionSize);

        public static Vector3i ChunkInWorld(Vector3i v) => ChunkInWorld(v.X, v.Y, v.Z);
        public static Vector3i ChunkInWorld(int x, int y, int z) => new Vector3i(
            x < 0 ? (x + 1) / Chunk.Size - 1 : x / Chunk.Size,
            y < 0 ? (y + 1) / Chunk.Size - 1 : y / Chunk.Size,
            z < 0 ? (z + 1) / Chunk.Size - 1 : z / Chunk.Size);

        public static Vector3i BlockInChunk(int x, int y, int z) => new Vector3i(
            x < 0 ? (x + 1) % Chunk.Size + Chunk.Size - 1 : x % Chunk.Size,
            y < 0 ? (y + 1) % Chunk.Size + Chunk.Size - 1 : y % Chunk.Size,
            z < 0 ? (z + 1) % Chunk.Size + Chunk.Size - 1 : z % Chunk.Size);

        public readonly Dictionary<Vector3i, Chunk> LoadedChunks = new Dictionary<Vector3i, Chunk>();

        public int ChunksQueuedCount => _queuedChunkUpdatesHp.Count + _queuedChunkUpdatesLp.Count;
        public int ChunksReadyCount => _chunksReadyToUploadHp.Count + _chunksReadyToUploadLp.Count;
        public int ChunksLoadedCount => LoadedChunks.Count;
        public int ChunkThreadsCount => _chunkThreadCount;

        private readonly List<Vector3i> _loadedRegions = new List<Vector3i>();

        private readonly Queue<Chunk> _queuedChunkUpdatesHp = new Queue<Chunk>();
        private readonly Queue<Chunk> _queuedChunkUpdatesLp = new Queue<Chunk>();

        private readonly Queue<Chunk> _chunksReadyToUploadHp = new Queue<Chunk>();
        private readonly Queue<Chunk> _chunksReadyToUploadLp = new Queue<Chunk>();

        private readonly Queue<ChunkCache> _regionsReadyToAdd = new Queue<ChunkCache>();
        private readonly Queue<Vector3i> _regionsReadyToRemove = new Queue<Vector3i>();


        private int _chunkThreadCount;
        private bool _unloaded;

        public void SetBlock(Vector3i blockPos, uint id) => SetBlock(blockPos.X, blockPos.Y, blockPos.Z, id);
        public void SetBlock(int x, int y, int z, uint id) => SetBlock(x, y, z, id, true, false);

        public void SetBlock(int x, int y, int z, uint id, bool update, bool lowPriority)
        {
            var chunkInWorld = ChunkInWorld(x, y, z);
            var blockInChunk = BlockInChunk(x, y, z);

            if (LoadedChunks.TryGetValue(chunkInWorld, out Chunk chunk))
                chunk.SetBlock(blockInChunk.X, blockInChunk.Y, blockInChunk.Z, id);
            else
            {
                chunk = new Chunk(this, chunkInWorld);
                chunk.SetBlock(blockInChunk.X, blockInChunk.Y, blockInChunk.Z, id);
                LoadedChunks.Add(chunkInWorld, chunk);
            }

            if (!update) return;

            QueueChunkUpdate(chunk, lowPriority);
            if (blockInChunk.X == 0)
                QueueChunkUpdate(chunkInWorld + new Vector3i(-1, 0, 0), lowPriority);
            else if (blockInChunk.X == Chunk.Size-1)
                QueueChunkUpdate(chunkInWorld + new Vector3i(+1, 0, 0), lowPriority);
            if (blockInChunk.Y == 0)
                QueueChunkUpdate(chunkInWorld + new Vector3i(0, -1, 0), lowPriority);
            else if (blockInChunk.Y == Chunk.Size - 1)
                QueueChunkUpdate(chunkInWorld + new Vector3i(0, +1, 0), lowPriority);
            if (blockInChunk.Z == 0)
                QueueChunkUpdate(chunkInWorld + new Vector3i(0, 0, -1), lowPriority);
            else if (blockInChunk.Z == Chunk.Size - 1)
                QueueChunkUpdate(chunkInWorld + new Vector3i(0, 0, +1), lowPriority);
        }

        public uint GetBlock(Vector3i blockPos) => GetBlock(blockPos.X, blockPos.Y, blockPos.Z);
        public uint GetBlock(int x, int y, int z)
        {
            var chunkInWorld = ChunkInWorld(x, y, z);
            var blockInChunk = BlockInChunk(x, y, z);

            return LoadedChunks.TryGetValue(chunkInWorld, out Chunk chunk) && chunk != null
                ? chunk.GetBlock(blockInChunk.X, blockInChunk.Y, blockInChunk.Z)
                : 0;
        }

        public void QueueChunkUpdate(Vector3i chunkPos, bool lowPrioriity)
        {
            if(LoadedChunks.TryGetValue(chunkPos, out Chunk chunk))
                QueueChunkUpdate(chunk, lowPrioriity);
        }

        public void QueueChunkUpdate(Chunk chunk, bool lowPrioriity)
        {
            chunk.InterruptUpdate();

            var queue = lowPrioriity ? _queuedChunkUpdatesLp : _queuedChunkUpdatesHp;
            queue.Enqueue(chunk);
        }

        public void Update()
        {
            if (_unloaded) return;

            UnloadChunks(false);
            LoadChunks();

            UpdateChunks();
        }

        public BlockRaytraceResult BlockRaytrace(Vector3 position, Vector3 direction, float range)
        {
            const float epsilon = -1e-6f;

            direction.Normalize();
            var start = (position - direction * 0.5f).ToVector3i();
            var end = (position + direction * (range + 0.5f)).ToVector3i();

            var minX = Math.Min(start.X, end.X) - 1;
            var minY = Math.Min(start.Y, end.Y) - 1;
            var minZ = Math.Min(start.Z, end.Z) - 1;

            var maxX = Math.Max(start.X, end.X) + 1;
            var maxY = Math.Max(start.Y, end.Y) + 1;
            var maxZ = Math.Max(start.Z, end.Z) + 1;

            BlockRaytraceResult result = null;
            for (var x = minX; x <= maxX; x++)
            for (var y = minY; y <= maxY; y++)
            for (var z = minZ; z <= maxZ; z++)
            {
                var block = GetBlock(x, y, z);
                if (block == 0) continue;

                var translation = Vector3.Zero;
                var scale = Vector3.One;

                foreach (var face in BlockFaceHelper.Faces)
                {
                    var normal = face.GetNormal();
                    var divisor = Vector3.Dot(normal, direction);

                    //ignore back faces
                    if (divisor >= epsilon) continue;

                    var planeNormal = normal * normal;
                    var blockPos = new Vector3(x, y, z) + translation;
                    var blockSize = new Vector3(0.5f) * scale;
                    var d = -(Vector3.Dot(blockPos, planeNormal) + Vector3.Dot(blockSize, normal));
                    var numerator = Vector3.Dot(planeNormal, position) + d;
                    var distance = Math.Abs(-numerator / divisor);

                    var point = position + distance * direction;
                    if (point.X < x + translation.X - blockSize.X + epsilon ||
                        point.X > x + translation.X + blockSize.X - epsilon ||
                        point.Y < y + translation.Y - blockSize.Y + epsilon ||
                        point.Y > y + translation.Y + blockSize.Y - epsilon ||
                        point.Z < z + translation.Z - blockSize.Z + epsilon ||
                        point.Z > z + translation.Z + blockSize.Z - epsilon) continue;

                    if (distance <= range && (result == null || result.Distance > distance))
                        result = new BlockRaytraceResult(face, new Vector3i(x, y, z), distance,
                            point);
                }
            }

            return result;
        }

        public void Unload()
        {
            _unloaded = true;
            Logger.Info("Saving world...");
            while (LoadedChunks.Count > 0)
            {
                UnloadChunks(true);
                Thread.Sleep(100);
            }
            Logger.Info("World saved");
        }


        private void LoadChunks()
        {
            var playerRegion = RegionInWorld(PlayerController.Camera.Position.ToVector3i());
            for (var x = -1; x <= 1; x++)
            for (var y = -1; y <= 1; y++)
            for (var z = -1; z <= 1; z++)
            {
                var regionPos = playerRegion + new Vector3i(x, y, z);
                if (_loadedRegions.Contains(regionPos)) continue;
                _loadedRegions.Add(regionPos);

                ThreadPool.QueueUserWorkItem(state =>
                {
                    var cache = new ChunkCache(this);
                    LoadRegion(cache, regionPos, regionPos * RegionSize,
                        regionPos * RegionSize + new Vector3i(RegionSize - 1));
                    lock (_regionsReadyToAdd) _regionsReadyToAdd.Enqueue(cache);
                });
            }
        }

        private void UnloadChunks(bool unloadAll)
        {
            var regionsToUnload = new Stack<Vector3i>();

            var playerRegion = RegionInWorld(PlayerController.Camera.Position.ToVector3i());
            foreach (var region in _loadedRegions)
            {
                var v = region - playerRegion;
                if (!unloadAll && v.X >= -2 && v.X <= 2 && v.Y >= -2 && v.Y <= 2 && v.Z >= -2 && v.Z <= 2) continue;

                regionsToUnload.Push(region);
                ThreadPool.QueueUserWorkItem(state =>
                {
                    SaveRegion(region);
                    lock (_regionsReadyToRemove) _regionsReadyToRemove.Enqueue(region);
                });
            }

            while (regionsToUnload.Count > 0)
                _loadedRegions.Remove(regionsToUnload.Pop());

            while (_regionsReadyToRemove.Count > 0)
            {
                Vector3i region;
                lock (_regionsReadyToRemove) region = _regionsReadyToRemove.Dequeue();
                var chunkMinPos = ChunkInWorld(region * RegionSize);
                for (var x = 0; x < ChunksPerRegion; x++)
                for (var y = 0; y < ChunksPerRegion; y++)
                for (var z = 0; z < ChunksPerRegion; z++)
                {
                    var key = chunkMinPos + new Vector3i(x, y, z);
                    if (!LoadedChunks.TryGetValue(key, out Chunk chunk)) continue;

                    chunk.Dispose();
                    LoadedChunks.Remove(key);
                }
            }
        }

        private void SaveRegion(Vector3i region)
        {
            WorldSerializer.SaveRegion(this, region);
        }

        private void LoadRegion(ChunkCache cache, Vector3i region, Vector3i worldMin, Vector3i worldMax)
        {
            if (WorldSerializer.LoadRegion(cache, region))
                return;

            if (worldMin.Y != 0) return;

            for (var x = worldMin.X; x <= worldMax.X; x++)
            for (var z = worldMin.Z; z <= worldMax.Z; z++)
                cache.SetBlock(x, 0, z, 1);
        }

        private void UpdateChunks()
        {
            UploadChunkQueue(_chunksReadyToUploadHp, 6);
            UploadChunkQueue(_chunksReadyToUploadLp, MaxChunkUploads);

            UpdateChunkQueue(_queuedChunkUpdatesHp, _chunksReadyToUploadHp);
            UpdateChunkQueue(_queuedChunkUpdatesLp, _chunksReadyToUploadLp);

            lock(_regionsReadyToAdd) if (_regionsReadyToAdd.Count > 0) _regionsReadyToAdd.Dequeue().AddToWorldAndUpdate();
        }

        private void UploadChunkQueue(Queue<Chunk> queue, int max)
        {
            for (var i = 0; queue.Count > 0 && i < max; i++)
                lock (queue) queue.Dequeue().Upload();
        }

        private void UpdateChunkQueue(Queue<Chunk> queue, Queue<Chunk> uploadQueue)
        {
            while (queue.Count > 0 && _chunkThreadCount < MaxAsyncChunkUpdates)
            {
                var chunk = queue.Dequeue();
                Interlocked.Increment(ref _chunkThreadCount);
                ThreadPool.QueueUserWorkItem(state =>
                {
                    if (chunk.Update())
                        lock (uploadQueue)
                            if (!uploadQueue.Contains(chunk)) uploadQueue.Enqueue(chunk);

                    Interlocked.Decrement(ref _chunkThreadCount);
                });
            }
        }
    }
}