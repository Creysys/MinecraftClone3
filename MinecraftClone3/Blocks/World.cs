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
        public const int MaxChunkUpdates = 6;
        public const int RegionSize = 64 * Chunk.Size;
        public const int RegionSizeSquared = RegionSize * RegionSize;
        public const int ChunksPerRegion = RegionSize / Chunk.Size;

        public static readonly TimeSpan ChunkLifetime = TimeSpan.FromSeconds(5);

        private readonly Queue<CachedChunk> _chunksReadyToAdd = new Queue<CachedChunk>();
        private readonly Queue<Chunk> _chunksReadyToRemove = new Queue<Chunk>();

        private readonly Queue<Chunk> _chunksReadyToUploadHp = new Queue<Chunk>();
        private readonly Queue<Chunk> _chunksReadyToUploadLp = new Queue<Chunk>();
        private readonly Thread _loadThread;
        private readonly HashSet<Vector3i> _populatedChunks = new HashSet<Vector3i>();

        private readonly Queue<Chunk> _queuedChunkUpdatesHp = new Queue<Chunk>();
        private readonly Queue<Chunk> _queuedChunkUpdatesLp = new Queue<Chunk>();

        private readonly Dictionary<Vector3i, DateTime> _regionTimers = new Dictionary<Vector3i, DateTime>();
        private readonly Thread _unloadThread;

        private readonly Thread _updateThread;

        public readonly Dictionary<Vector3i, Chunk> LoadedChunks = new Dictionary<Vector3i, Chunk>();

        private bool _unloaded;

        public World()
        {
            _updateThread = new Thread(UpdateThread);
            _unloadThread = new Thread(UnloadThread);
            _loadThread = new Thread(LoadThread);

            _updateThread.Start();
            _unloadThread.Start();
            _loadThread.Start();
        }

        public int ChunksQueuedCount => _queuedChunkUpdatesHp.Count + _queuedChunkUpdatesLp.Count;
        public int ChunksReadyCount => _chunksReadyToUploadHp.Count + _chunksReadyToUploadLp.Count;
        public int ChunksLoadedCount => LoadedChunks.Count;

        public static Vector3i ChunkToRegion(Vector3i v) => RegionInWorld(v * Chunk.Size);

        public static Vector3i RegionInWorld(Vector3i v) => RegionInWorld(v.X, v.Y, v.Z);

        public static Vector3i ChunkInRegion(Vector3i v) => ChunkInRegion(v.X, v.Y, v.Z);
        public static Vector3i ChunkInRegion(int x, int y, int z) => new Vector3i(
            x < 0 ? (x + 1) % ChunksPerRegion + ChunksPerRegion - 1 : x % ChunksPerRegion,
            y < 0 ? (y + 1) % ChunksPerRegion + ChunksPerRegion - 1 : y % ChunksPerRegion,
            z < 0 ? (z + 1) % ChunksPerRegion + ChunksPerRegion - 1 : z % ChunksPerRegion);

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

            if (blockInChunk.X == 0)
                QueueChunkUpdate(chunkInWorld + new Vector3i(-1, 0, 0), lowPriority);
            else if (blockInChunk.X == Chunk.Size - 1)
                QueueChunkUpdate(chunkInWorld + new Vector3i(+1, 0, 0), lowPriority);
            if (blockInChunk.Y == 0)
                QueueChunkUpdate(chunkInWorld + new Vector3i(0, -1, 0), lowPriority);
            else if (blockInChunk.Y == Chunk.Size - 1)
                QueueChunkUpdate(chunkInWorld + new Vector3i(0, +1, 0), lowPriority);
            if (blockInChunk.Z == 0)
                QueueChunkUpdate(chunkInWorld + new Vector3i(0, 0, -1), lowPriority);
            else if (blockInChunk.Z == Chunk.Size - 1)
                QueueChunkUpdate(chunkInWorld + new Vector3i(0, 0, +1), lowPriority);
            QueueChunkUpdate(chunk, lowPriority);
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
            if (LoadedChunks.TryGetValue(chunkPos, out Chunk chunk))
                QueueChunkUpdate(chunk, lowPrioriity);
        }

        public void QueueChunkUpdate(Chunk chunk, bool lowPrioriity)
        {
            var queue = lowPrioriity ? _queuedChunkUpdatesLp : _queuedChunkUpdatesHp;
            lock (queue)
            {
                if (!queue.Contains(chunk)) queue.Enqueue(chunk);
            }
        }

        public void Update()
        {
            if (_unloaded) return;
            
            var actions = 0;
            Chunk chunk;
            while (actions < MaxChunkUpdates && _chunksReadyToUploadHp.Count > 0)
            {
                lock (_chunksReadyToUploadHp)
                {
                    chunk = _chunksReadyToUploadHp.Dequeue();
                }
                chunk.Upload();
                actions++;
            }

            if (actions < MaxChunkUpdates && _chunksReadyToUploadLp.Count > 0)
            {
                lock (_chunksReadyToUploadLp)
                {
                    chunk = _chunksReadyToUploadLp.Dequeue();
                }
                chunk.Upload();
                actions++;
            }

            if (actions < MaxChunkUpdates && _chunksReadyToRemove.Count > 0)
            {
                lock (_chunksReadyToRemove)
                {
                    chunk = _chunksReadyToRemove.Dequeue();
                }
                LoadedChunks.Remove(chunk.Position);
                _populatedChunks.Remove(chunk.Position);
                chunk.Dispose();
                actions++;
            }

            while (actions < MaxChunkUpdates && _chunksReadyToAdd.Count > 0)
            {
                CachedChunk cachedChunk;
                lock (_chunksReadyToAdd)
                {
                    cachedChunk = _chunksReadyToAdd.Dequeue();
                }
                chunk = new Chunk(cachedChunk);
                LoadedChunks.Add(chunk.Position, chunk);

                QueueChunkUpdate(chunk, true);
                foreach (var face in BlockFaceHelper.Faces)
                    QueueChunkUpdate(chunk.Position + face.GetNormali(), true);

                actions++;
            }
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

            Logger.Info("Waiting for threads to finish...");
            while (_loadThread.IsAlive || _unloadThread.IsAlive || _updateThread.IsAlive)
                Thread.Sleep(100);

            Logger.Info("Saving world...");
            foreach (var entry in _regionTimers)
                WorldSerializer.SaveRegion(this, entry.Key);
            Logger.Info("World saved");
        }


        private void UpdateThread()
        {
            // ReSharper disable once TooWideLocalVariableScope
            Chunk chunk;
            while (!_unloaded)
            {
                while (_queuedChunkUpdatesHp.Count > 0)
                {
                    lock (_queuedChunkUpdatesHp)
                    {
                        chunk = _queuedChunkUpdatesHp.Dequeue();
                    }
                    chunk.Update();
                    lock (_chunksReadyToUploadHp)
                    {
                        if (!_chunksReadyToUploadHp.Contains(chunk)) _chunksReadyToUploadHp.Enqueue(chunk);
                    }
                }

                if (_queuedChunkUpdatesLp.Count <= 0) continue;
                lock (_queuedChunkUpdatesLp)
                {
                    chunk = _queuedChunkUpdatesLp.Dequeue();
                }
                chunk.Update();
                lock (_chunksReadyToUploadLp)
                {
                    if (!_chunksReadyToUploadLp.Contains(chunk)) _chunksReadyToUploadLp.Enqueue(chunk);
                }

                Thread.Sleep(10);
            }
        }

        private void UnloadThread()
        {
            while (!_unloaded)
            {
                var regionsToRemove = new Stack<Vector3i>();
                lock (_regionTimers)
                {
                    foreach (var entry in _regionTimers)
                        if (DateTime.Now - entry.Value >= ChunkLifetime)
                            regionsToRemove.Push(entry.Key);
                }

                while (regionsToRemove.Count > 0)
                {
                    var region = regionsToRemove.Pop();

                    WorldSerializer.SaveRegion(this, region);

                    var chunkMinPos = ChunkInWorld(region * RegionSize);
                    for (var x = 0; x < ChunksPerRegion; x++)
                    for (var y = 0; y < ChunksPerRegion; y++)
                    for (var z = 0; z < ChunksPerRegion; z++)
                    {
                        var chunkPos = chunkMinPos + new Vector3i(x, y, z);
                        if (LoadedChunks.ContainsKey(chunkPos))
                            _chunksReadyToRemove.Enqueue(LoadedChunks[chunkPos]);
                    }

                    lock (_regionTimers)
                    {
                        _regionTimers.Remove(region);
                    }
                }

                Thread.Sleep(10);
            }
        }

        private void LoadThread()
        {
            while (!_unloaded)
            {
                //Load 3x3 chunks around player
                var playerChunk = ChunkInWorld(PlayerController.Camera.Position.ToVector3i());
                for (var x = -1; x <= 1; x++)
                for (var y = -1; y <= 1; y++)
                for (var z = -1; z <= 1; z++)
                {
                    var chunkPos = playerChunk + new Vector3i(x, y, z);

                    //Reset region unload timer
                    lock (_regionTimers)
                    {
                        _regionTimers[ChunkToRegion(chunkPos)] = DateTime.Now;
                    }

                    if (_populatedChunks.Contains(chunkPos)) continue;
                    _populatedChunks.Add(chunkPos);

                    var chunk = LoadChunk(chunkPos);
                    lock (_chunksReadyToAdd)
                    {
                        _chunksReadyToAdd.Enqueue(chunk);
                    }
                }

                //TODO: Load player view frustum

                Thread.Sleep(10);
            }
        }

        private CachedChunk LoadChunk(Vector3i position)
        {
            var chunk = WorldSerializer.LoadChunk(this, position);
            if (chunk != null) return chunk;

            chunk = new CachedChunk(this, position);
            var worldMin = position * Chunk.Size;
            var worldMax = worldMin + new Vector3i(Chunk.Size - 1);

            for (var x = 0; x < Chunk.Size; x++)
            for (var y = 0; y < Chunk.Size; y++)
            for (var z = 0; z < Chunk.Size; z++)
                if (worldMin.Y + y <= 0)
                    chunk.SetBlock(x, y, z, 2);

            return chunk;
        }
    }
}