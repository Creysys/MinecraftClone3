using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using MinecraftClone3API.Entities;
using MinecraftClone3API.Util;
using OpenTK;

namespace MinecraftClone3API.Blocks
{
    public class World
    {
        public const int MaxChunkUpdates = 6;

        static OpenSimplexNoise simplex = new OpenSimplexNoise(42237560);

        public static readonly TimeSpan ChunkLifetime = TimeSpan.FromSeconds(30);
        private readonly Dictionary<Vector3i, CachedChunk> _chunksReadyToAdd = new Dictionary<Vector3i, CachedChunk>();
        private readonly HashSet<Vector3i> _chunksReadyToRemove = new HashSet<Vector3i>();

        private readonly Queue<Chunk> _chunksReadyToUploadHp = new Queue<Chunk>();
        private readonly Queue<Chunk> _chunksReadyToUploadLp = new Queue<Chunk>();

        private readonly HashSet<Vector3i> _populatedChunks = new HashSet<Vector3i>();

        private readonly Queue<Chunk> _queuedChunkUpdatesHp = new Queue<Chunk>();
        private readonly Queue<Chunk> _queuedChunkUpdatesLp = new Queue<Chunk>();
        private readonly Queue<Vector3i> _queuedLightUpdates = new Queue<Vector3i>();

        private readonly Thread _unloadThread;
        private readonly Thread _loadThread;

        private readonly Thread _updateThread;

        public readonly Dictionary<Vector3i, Chunk> LoadedChunks = new Dictionary<Vector3i, Chunk>();
        public readonly HashSet<EntityPlayer> PlayerEntities = new HashSet<EntityPlayer>();
        public readonly HashSet<Entity> Entities = new HashSet<Entity>();

        private bool _unloaded;

        public World()
        {
            _unloadThread = new Thread(UnloadThread) {Name = "Unload Thread"};
            _loadThread = new Thread(LoadThread) {Name = "Load Thread"};
            _updateThread = new Thread(UpdateThread) {Name = "Update Thread"};

            _unloadThread.Start();
            _loadThread.Start();
            _updateThread.Start();
        }

        public int ChunksQueuedCount => _queuedChunkUpdatesHp.Count + _queuedChunkUpdatesLp.Count;
        public int ChunksReadyCount => _chunksReadyToUploadHp.Count + _chunksReadyToUploadLp.Count;
        public int ChunksLoadedCount => LoadedChunks.Count;


        public static Vector3i ChunkInWorld(Vector3i v) => ChunkInWorld(v.X, v.Y, v.Z);

        public static Vector3i ChunkInWorld(int x, int y, int z) => new Vector3i(
            x < 0 ? (x + 1) / Chunk.Size - 1 : x / Chunk.Size,
            y < 0 ? (y + 1) / Chunk.Size - 1 : y / Chunk.Size,
            z < 0 ? (z + 1) / Chunk.Size - 1 : z / Chunk.Size);

        public static Vector3i BlockInChunk(int x, int y, int z) => new Vector3i(
            x < 0 ? (x + 1) % Chunk.Size + Chunk.Size - 1 : x % Chunk.Size,
            y < 0 ? (y + 1) % Chunk.Size + Chunk.Size - 1 : y % Chunk.Size,
            z < 0 ? (z + 1) % Chunk.Size + Chunk.Size - 1 : z % Chunk.Size);

        public void SetBlock(Vector3i blockPos, Block block) => SetBlock(blockPos.X, blockPos.Y, blockPos.Z, block);
        public void SetBlock(int x, int y, int z, Block block) => SetBlock(x, y, z, block, true, false);

        public void SetBlock(int x, int y, int z, Block block, bool update, bool lowPriority)
        {
            var chunkInWorld = ChunkInWorld(x, y, z);
            var blockInChunk = BlockInChunk(x, y, z);

            if (LoadedChunks.TryGetValue(chunkInWorld, out var chunk))
                chunk.SetBlock(blockInChunk.X, blockInChunk.Y, blockInChunk.Z, block.Id);
            else
            {
                chunk = new Chunk(this, chunkInWorld);
                chunk.SetBlock(blockInChunk.X, blockInChunk.Y, blockInChunk.Z, block.Id);
                lock (LoadedChunks)
                {
                    LoadedChunks.Add(chunkInWorld, chunk);
                }
            }

            if (!update) return;

            var pos = new Vector3i(x, y, z);
            lock (_queuedLightUpdates)
            {
                if (!_queuedLightUpdates.Contains(pos)) _queuedLightUpdates.Enqueue(pos);
            }

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

        public Block GetBlock(Vector3i blockPos) => GetBlock(blockPos.X, blockPos.Y, blockPos.Z);

        public Block GetBlock(int x, int y, int z)
        {
            var chunkInWorld = ChunkInWorld(x, y, z);
            var blockInChunk = BlockInChunk(x, y, z);

            return LoadedChunks.TryGetValue(chunkInWorld, out Chunk chunk)
                ? GameRegistry.GetBlock(chunk.GetBlock(blockInChunk.X, blockInChunk.Y, blockInChunk.Z))
                : BlockRegistry.BlockAir;
        }

        public void SetBlockLightLevel(Vector3i blockPos, int lightLevel)
            => SetBlockLightLevel(blockPos.X, blockPos.Y, blockPos.Z, lightLevel);
        public void SetBlockLightLevel(int x, int y, int z, int lightLevel)
        {
            var chunkInWorld = ChunkInWorld(x, y, z);
            var blockInChunk = BlockInChunk(x, y, z);

            if (!LoadedChunks.TryGetValue(chunkInWorld, out var chunk)) return;
            chunk.SetLightLevel(blockInChunk.X, blockInChunk.Y, blockInChunk.Z, (byte) lightLevel);
            QueueChunkUpdate(chunk, false);
        }

        public byte GetBlockLightLevel(Vector3i blockPos) => GetBlockLightLevel(blockPos.X, blockPos.Y, blockPos.Z);
        public byte GetBlockLightLevel(int x, int y, int z)
        {
            var chunkInWorld = ChunkInWorld(x, y, z);
            var blockInChunk = BlockInChunk(x, y, z);

            return LoadedChunks.TryGetValue(chunkInWorld, out Chunk chunk)
                ? chunk.GetLightLevel(blockInChunk.X, blockInChunk.Y, blockInChunk.Z)
                : (byte)0;
        }

        public void QueueChunkUpdate(Vector3i chunkPos, bool lowPrioriity)
        {
            if (LoadedChunks.TryGetValue(chunkPos, out var chunk))
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
                        //var blocPo
                        //var bb = block.GetBoundingBox(this, )
                        //TODO: finish bb
                if (block.CanPassThrough(this, new Vector3i(x, y, z))) continue;

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
                        result = new BlockRaytraceResult(block, face, new Vector3i(x, y, z), distance,
                            point);
                }
            }

            return result;
        }

        public void Update()
        {
            if (_unloaded) return;

            //Update entities
            foreach (var playerEntity in PlayerEntities)
            {
                playerEntity.Update();
            }
            foreach (var entity in Entities)
            {
                entity.Update();
            }

            lock (_chunksReadyToRemove)
            {
                while (_chunksReadyToRemove.Count > 0)
                {
                    var chunkPos = _chunksReadyToRemove.First();

                    if (LoadedChunks.TryGetValue(chunkPos, out var chunk))
                    {
                        lock (LoadedChunks)
                        {
                            LoadedChunks.Remove(chunkPos);
                        }
                        _populatedChunks.Remove(chunkPos);
                        chunk.Dispose();
                    }

                    _chunksReadyToRemove.Remove(chunkPos);
                }
            }

            lock (_chunksReadyToAdd)
            {
                while (_chunksReadyToAdd.Count > 0)
                {
                    var entry = _chunksReadyToAdd.First();
                    lock (LoadedChunks)
                    {
                        LoadedChunks.Add(entry.Key, new Chunk(entry.Value));
                    }
                    _populatedChunks.Add(entry.Key);
                    _chunksReadyToAdd.Remove(entry.Key);
                }
            }

            while (_chunksReadyToUploadHp.Count > 0)
                lock (_chunksReadyToUploadHp)
                {
                    _chunksReadyToUploadHp.Dequeue().Upload();
                }

            while (_chunksReadyToUploadLp.Count > 0)
                lock (_chunksReadyToUploadLp)
                {
                    _chunksReadyToUploadLp.Dequeue().Upload();
                }
        }

        public void Unload()
        {
            _unloaded = true;

            Logger.Info("Waiting for threads to finish...");
            while (_loadThread.IsAlive || _unloadThread.IsAlive || _updateThread.IsAlive)
                Thread.Sleep(100);

            Logger.Info("Saving world...");
            foreach (var entry in LoadedChunks)
                WorldSerializer.SaveChunk(entry.Value);
            Logger.Info("World saved");
        }


        private void UpdateThread()
        {
            Vector3i blockPos;
            Chunk chunk;

            while (!_unloaded)
            {
                while (_queuedLightUpdates.Count > 0)
                {
                    lock (_queuedLightUpdates)
                    {
                        blockPos = _queuedLightUpdates.Dequeue();
                    }

                    UpdateLightValues(blockPos);
                }

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

                Thread.Sleep(1);
            }
        }

        private void UnloadThread()
        {
            while (!_unloaded)
            {
                List<Chunk> chunksToUnload;

                lock (LoadedChunks)
                {
                    chunksToUnload =
                        LoadedChunks.Where(
                            pair =>
                                DateTime.Now - pair.Value.Time > ChunkLifetime &&
                                !_chunksReadyToRemove.Contains(pair.Key)).Select(pair => pair.Value).ToList();
                }

                foreach (var chunk in chunksToUnload)
                {
                    WorldSerializer.SaveChunk(chunk);
                    lock (_chunksReadyToRemove)
                    {
                        _chunksReadyToRemove.Add(chunk.Position);
                    }
                }

                Thread.Sleep(1000);
            }
        }

        private void LoadThread()
        {
            var chunksWaitingForNeighbours = new HashSet<Vector3i>();

            while (!_unloaded)
            {
                //Remove chunks waiting that have been unloaded
                var chunksToRemove =
                    chunksWaitingForNeighbours.Where(
                        chunkPos =>
                            !_populatedChunks.Contains(chunkPos) && !_chunksReadyToAdd.ContainsKey(chunkPos) ||
                            _populatedChunks.Contains(chunkPos) && !LoadedChunks.ContainsKey(chunkPos)).ToList();
                chunksToRemove.ForEach(chunkPos => chunksWaitingForNeighbours.Remove(chunkPos));

                //Update chunks waiting for neighbours
                var chunksToUpdate =
                    chunksWaitingForNeighbours.Where(
                            chunkPos =>
                                BlockFaceHelper.Faces.All(
                                    face => _populatedChunks.Contains(chunkPos + face.GetNormali())))
                        .ToList();
                foreach (var chunkPos in chunksToUpdate)
                {
                    QueueChunkUpdate(chunkPos, true);
                    chunksWaitingForNeighbours.Remove(chunkPos);
                }

                //Load 5x3x5 chunks around player
                foreach (var playerEntity in PlayerEntities)
                {
                    var playerChunk = ChunkInWorld(playerEntity.Position.ToVector3i());
                    for (var x = -5; x <= 5; x++)
                    for (var y = -3; y <= 3; y++)
                    for (var z = -5; z <= 5; z++)
                    {
                        var chunkPos = playerChunk + new Vector3i(x, y, z);
                        if (_populatedChunks.Contains(chunkPos) || _chunksReadyToAdd.ContainsKey(chunkPos))
                        {
                            //Reset chunk time so it will not be unloaded
                            if (LoadedChunks.TryGetValue(chunkPos, out var chunk)) chunk.Time = DateTime.Now;
                            continue;
                        }

                        var cachedChunk = LoadChunk(chunkPos);

                        if (cachedChunk.IsEmpty)
                        {
                            //Empty chunks dont need to be added to LoadedChunks
                            lock (_populatedChunks)
                            {
                                _populatedChunks.Add(chunkPos);
                            }
                        }
                        else
                        {
                            lock (_chunksReadyToAdd)
                            {
                                _chunksReadyToAdd.Add(chunkPos, cachedChunk);
                            }
                            chunksWaitingForNeighbours.Add(chunkPos);
                        }
                    }

                    //TODO: Load player view frustum

                }
                Thread.Sleep(10);
            }
        }

        private void UpdateLightValues(Vector3i blockPos)
        {
            var block = GetBlock(blockPos);
            if (block.IsOpaqueFullBlock(this, blockPos))
            {
                SetBlockLightLevel(blockPos, 0);
                //TODO: occlude light
                return;
            }

            //get max neighbour light level - 1
            var maxNeighbourLightLevel = BlockFaceHelper.Faces.Aggregate(0,
                (current, face) => Math.Max(current, GetBlockLightLevel(blockPos + face.GetNormali()))) - 1;

            var blockEmittingLightLevel = Math.Max(maxNeighbourLightLevel, block.GetLightValue(this, blockPos));
            SetBlockLightLevel(blockPos, blockEmittingLightLevel);


            var stopwatch = new Stopwatch();
            stopwatch.Start();

            SpreadLightRecursive(blockPos, blockEmittingLightLevel, (BlockFace)int.MaxValue);

            stopwatch.Stop();
            Console.WriteLine($"Recursive light update took: {stopwatch.ElapsedMilliseconds} ms");
        }

        private void SpreadLightRecursive(Vector3i blockPos, int remainingLight, BlockFace from)
        {
            if (remainingLight == 0)
                return;

            var spreadToFaces = new Stack<BlockFace>();
            foreach (var face in BlockFaceHelper.Faces)
            {
                if (face == from) continue;

                var neighbourPos = blockPos + face.GetNormali();
                if (GetBlock(neighbourPos).IsOpaqueFullBlock(this, blockPos)) continue;
                if (GetBlockLightLevel(neighbourPos) >= remainingLight - 1) continue;

                SetBlockLightLevel(neighbourPos, remainingLight - 1);
                spreadToFaces.Push(face);
            }

            while (spreadToFaces.Count > 0)
            {
                var face = spreadToFaces.Pop();
                SpreadLightRecursive(blockPos + face.GetNormali(), remainingLight - 1, face.GetOpposite());
            }
        }

        private CachedChunk LoadChunk(Vector3i position)
        {
            var chunk = WorldSerializer.LoadChunk(this, position);
            if (chunk != null) return chunk;

            //TODO: implement terrain gen
            chunk = new CachedChunk(this, position);
            var worldMin = position * Chunk.Size;
            var worldMax = worldMin + new Vector3i(Chunk.Size - 1);

            for (var x = 0; x < Chunk.Size; x++)
            for (var z = 0; z < Chunk.Size; z++)
            {
                //var height = Perlin.OctavePerlin(worldMin.X + x, 0, worldMin.Z + z, 3, 0.5f, 0.234543)*0.001f;
                var height = simplex.eval((worldMin.X + x)*0.06f, (worldMin.Z + z)*0.06f)*5;
                height += simplex.eval((worldMin.X + x) * 0.1f, (worldMin.Z + z) * 0.1f)*2;
                height += simplex.eval((worldMin.X + x) * 0.005f, (worldMin.Z + z) * 0.005f) * 10;
                //height = 0;

                    for (var y = 0; y < Chunk.Size; y++)
                    if (worldMin.Y + y <= height)
                        chunk.SetBlock(x, y, z, (worldMin.Y + y == (int)height) ? GameRegistry.GetBlock("Vanilla:Grass") : GameRegistry.GetBlock("Vanilla:Dirt"));
            }


            return chunk;
        }
    }
}