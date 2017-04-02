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
            {
                if (chunk.GetBlock(blockInChunk) == block.Id) return;
                chunk.SetBlock(blockInChunk, block.Id);
            }
            else
            {
                chunk = new Chunk(this, chunkInWorld);
                chunk.SetBlock(blockInChunk, block.Id);
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
                ? GameRegistry.GetBlock(chunk.GetBlock(blockInChunk))
                : BlockRegistry.BlockAir;
        }

        public void SetBlockData(Vector3i blockPos, BlockData data)=> SetBlockData(blockPos.X, blockPos.Y, blockPos.Z, data);

        public void SetBlockData(int x, int y, int z, BlockData data)
        {
            var chunkInWorld = ChunkInWorld(x, y, z);
            var blockInChunk = BlockInChunk(x, y, z);

            if (LoadedChunks.TryGetValue(chunkInWorld, out var chunk))
                chunk.SetBlockData(blockInChunk, data);

            var pos = new Vector3i(x, y, z);
            lock (_queuedLightUpdates)
            {
                if (!_queuedLightUpdates.Contains(pos)) _queuedLightUpdates.Enqueue(pos);
            }

            if (blockInChunk.X == 0)
                QueueChunkUpdate(chunkInWorld + new Vector3i(-1, 0, 0), false);
            else if (blockInChunk.X == Chunk.Size - 1)
                QueueChunkUpdate(chunkInWorld + new Vector3i(+1, 0, 0), false);
            if (blockInChunk.Y == 0)
                QueueChunkUpdate(chunkInWorld + new Vector3i(0, -1, 0), false);
            else if (blockInChunk.Y == Chunk.Size - 1)
                QueueChunkUpdate(chunkInWorld + new Vector3i(0, +1, 0), false);
            if (blockInChunk.Z == 0)
                QueueChunkUpdate(chunkInWorld + new Vector3i(0, 0, -1), false);
            else if (blockInChunk.Z == Chunk.Size - 1)
                QueueChunkUpdate(chunkInWorld + new Vector3i(0, 0, +1), false);
            QueueChunkUpdate(chunk, false);
        }

        public void SetBlockLightLevel(Vector3i blockPos, LightLevel lightLevel)
            => SetBlockLightLevel(blockPos.X, blockPos.Y, blockPos.Z, lightLevel);

        public void SetBlockLightLevel(int x, int y, int z, LightLevel lightLevel)
        {
            var chunkInWorld = ChunkInWorld(x, y, z);
            var blockInChunk = BlockInChunk(x, y, z);

            if (!LoadedChunks.TryGetValue(chunkInWorld, out var chunk)) return;
            chunk.SetLightLevel(blockInChunk, lightLevel);

            if (blockInChunk.X == 0)
                QueueChunkUpdate(chunkInWorld + new Vector3i(-1, 0, 0), false);
            else if (blockInChunk.X == Chunk.Size - 1)
                QueueChunkUpdate(chunkInWorld + new Vector3i(+1, 0, 0), false);
            if (blockInChunk.Y == 0)
                QueueChunkUpdate(chunkInWorld + new Vector3i(0, -1, 0), false);
            else if (blockInChunk.Y == Chunk.Size - 1)
                QueueChunkUpdate(chunkInWorld + new Vector3i(0, +1, 0), false);
            if (blockInChunk.Z == 0)
                QueueChunkUpdate(chunkInWorld + new Vector3i(0, 0, -1), false);
            else if (blockInChunk.Z == Chunk.Size - 1)
                QueueChunkUpdate(chunkInWorld + new Vector3i(0, 0, +1), false);
            QueueChunkUpdate(chunk, false);
        }

        public void SetBlockLightLevelColor(Vector3i blockPos, int value, int color)
        {
            var lightLevel = GetBlockLightLevel(blockPos);
            lightLevel[color] = value;
            SetBlockLightLevel(blockPos, lightLevel);
        }

        public LightLevel GetBlockLightLevel(Vector3i blockPos) => GetBlockLightLevel(blockPos.X, blockPos.Y, blockPos.Z);
        public LightLevel GetBlockLightLevel(int x, int y, int z)
        {
            var chunkInWorld = ChunkInWorld(x, y, z);
            var blockInChunk = BlockInChunk(x, y, z);

            return LoadedChunks.TryGetValue(chunkInWorld, out Chunk chunk)
                ? chunk.GetLightLevel(blockInChunk)
                : LightLevel.Zero;
        }

        public int GetBlockLightLevelColor(Vector3i blockPos, int color) => GetBlockLightLevel(blockPos)[color];

        public BlockData GetBlockData(Vector3i blockPos) => GetBlockData(blockPos.X, blockPos.Y, blockPos.Z);
        public BlockData GetBlockData(int x, int y, int z)
        {
            var chunkInWorld = ChunkInWorld(x, y, z);
            var blockInChunk = BlockInChunk(x, y, z);

            return LoadedChunks.TryGetValue(chunkInWorld, out Chunk chunk) ? chunk.GetBlockData(blockInChunk) : null;
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

        public bool IsBlockInEmptyChunk(Vector3i blockPos) => !LoadedChunks.ContainsKey(ChunkInWorld(blockPos));

        public bool IsOpaqueFullBlock(Vector3i blockPos) => GetBlock(blockPos).IsOpaqueFullBlock(this, blockPos);
        public bool IsFullBlock(Vector3i blockPos) => GetBlock(blockPos).IsFullBlock(this, blockPos);

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
                var blockPosi = new Vector3i(x, y, z);
                var bb = block.GetBoundingBox(this, blockPosi);
                if (bb == null || !block.CanTarget(this, blockPosi)) continue;

                var translation = bb.Min + (bb.Max - bb.Min) * 0.5f;
                var scale = bb.Max - bb.Min;

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
                            point, bb);
                }
            }

            return result;
        }

        public void PlaceBlock(EntityPlayer player, Vector3i blockPos, Block block)
        {
            SetBlock(blockPos, block);
            block.OnPlaced(this, blockPos, player);
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
                        if(!LoadedChunks.ContainsKey(entry.Key))
                            LoadedChunks.Add(entry.Key, new Chunk(entry.Value));
                        else Logger.Error("Chunk has already been loaded! " + entry.Key);
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
                
                var playerChunksLists = new List<List<Vector3i>>();
                foreach (var playerEntity in PlayerEntities)
                {
                    var playerChunksToLoad = new List<Vector3i>();
                    var playerChunk = ChunkInWorld(playerEntity.Position.ToVector3i());

                    //Load 7x7x7 around player
                    for (var x = -3; x <= 3; x++)
                        for (var y = -3; y <= 3; y++)
                            for (var z = -3; z <= 3; z++)
                            {
                                var chunkPos = playerChunk + new Vector3i(x, y, z);
                                if (playerChunksToLoad.Contains(chunkPos)) continue;
                                if (_populatedChunks.Contains(chunkPos) || _chunksReadyToAdd.ContainsKey(chunkPos))
                                {
                                    //Reset chunk time so it will not be unloaded
                                    if (LoadedChunks.TryGetValue(chunkPos, out var chunk)) chunk.Time = DateTime.Now;
                                    continue;
                                }
                                
                                playerChunksToLoad.Add(chunkPos);
                            }


                    //Load 61x3x61 terrain if overworld
                    //heightmap(x*Chunk.Size + Chunk.Size/2, z*Chunk.Size + Chunk.Size/2)
                    
                    var height = 0 + Chunk.Size/2;
                    var heightMapChunkY = ChunkInWorld(0, height, 0).Y;
                    for (var x = -30; x <= 30; x++)
                        for (var y = -1; y <= 1; y++)
                            for (var z = -30; z <= 30; z++)
                            {
                                if (x <= 3 && x >= -3 && y <= 3 && y >= -3 && z <= 3 && z >= -3) continue;

                                var chunkPos = new Vector3i(playerChunk.X + x, heightMapChunkY + y, playerChunk.Z + z);
                                
                                if (_populatedChunks.Contains(chunkPos) || _chunksReadyToAdd.ContainsKey(chunkPos))
                                {
                                    //Reset chunk time so it will not be unloaded
                                    if (LoadedChunks.TryGetValue(chunkPos, out var chunk)) chunk.Time = DateTime.Now;
                                    continue;
                                }
                                
                                playerChunksToLoad.Add(chunkPos);
                            }


                    
                    playerChunksToLoad.Sort(
                        (v0, v1) =>
                            (int) (v0.ToVector3() - playerChunk.ToVector3()).LengthSquared -
                            (int) (v1.ToVector3() - playerChunk.ToVector3()).LengthSquared);

                    //Cap player chunk load tasks to 16
                    if(playerChunksToLoad.Count > 16)
                        playerChunksToLoad.RemoveRange(16, playerChunksToLoad.Count - 16);

                    playerChunksLists.Add(playerChunksToLoad);
                }

                var merged = ExtensionHelper.ZipMerge(playerChunksLists.ToArray());
                foreach (var chunkPos in merged)
                {
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

                Thread.Sleep(10);
            }
        }


        private struct LightNode
        {
            public readonly Vector3i Position;
            public readonly int Value;

            public LightNode(Vector3i position, int value)
            {
                Position = position;
                Value = value;
            }
        }

        private void UpdateLightValues(Vector3i blockPos)
        {
            var stopwatch = Stopwatch.StartNew();

            var block = GetBlock(blockPos);
            var blockEmittingLightLevel = block.GetLightLevel(this, blockPos);
            var oldBlockLightLevel = GetBlockLightLevel(blockPos);
            var occludingBlock = IsOpaqueFullBlock(blockPos);


            //If the block is not occluding its faces it can accept lighting from nearby blocks
            if (!occludingBlock)
            {
                foreach (var face in BlockFaceHelper.Faces)
                {
                    var neighborLightLevel = GetBlockLightLevel(blockPos + face.GetNormali());
                    for (var color = 0; color < 3; color++)
                    {
                        blockEmittingLightLevel[color] = Math.Max(blockEmittingLightLevel[color],
                            neighborLightLevel[color]);
                    }
                }

                for (var color = 0; color < 3; color++)
                    blockEmittingLightLevel[color] = block.OnLightPassThrough(this, blockPos,
                        blockEmittingLightLevel[color], color);
            }

            var cachedLightLevels = new Dictionary<Vector3i, LightLevel>();
            cachedLightLevels[blockPos] = blockEmittingLightLevel;

            var cachedBlocks = new Dictionary<Vector3i, Block>();

            //foreach color channel
            for (var color = 0; color < 3; color++)
            {
                var spreadQueue = new Queue<LightNode>(1024);
                var removeQueue = new Queue<LightNode>(1024);

                if (blockEmittingLightLevel[color] > oldBlockLightLevel[color])
                {
                    spreadQueue.Enqueue(new LightNode(blockPos, blockEmittingLightLevel[color]));
                }
                else if (blockEmittingLightLevel[color] < oldBlockLightLevel[color])
                {
                    removeQueue.Enqueue(new LightNode(blockPos, oldBlockLightLevel[color]));
                }

                while (removeQueue.Count > 0)
                {
                    var node = removeQueue.Dequeue();

                    foreach (var face in BlockFaceHelper.Faces)
                    {
                        var nextNode = node.Position + face.GetNormali();

                        //If chunk does not exist stop
                        if (IsBlockInEmptyChunk(nextNode)) continue;

                        //Cache light level if not already cached
                        if (!cachedLightLevels.TryGetValue(nextNode, out var nextNodeLightLevel))
                        {
                            nextNodeLightLevel = GetBlockLightLevel(nextNode);
                            cachedLightLevels[nextNode] = nextNodeLightLevel;
                        }

                        //If the next nodes light level is higher or equal to our value spread light to fill the holes
                        if (nextNodeLightLevel[color] >= node.Value)
                        {
                            spreadQueue.Enqueue(new LightNode(nextNode, node.Value));
                            continue;
                        }
                        //If the next nodes light level is zero stop
                        if (nextNodeLightLevel[color] == 0) continue;

                        //If next node block is an occluder stop
                        if (IsOpaqueFullBlock(nextNode)) continue;

                        //Set next nodes light level and advance
                        nextNodeLightLevel[color] = 0;
                        cachedLightLevels[nextNode] = nextNodeLightLevel;

                        if (node.Value - 1 > 0)
                            removeQueue.Enqueue(new LightNode(nextNode, node.Value - 1));
                    }
                }

                while (spreadQueue.Count > 0)
                {
                    var node = spreadQueue.Dequeue();

                    foreach (var face in BlockFaceHelper.Faces)
                    {
                        var nextNode = node.Position + face.GetNormali();
                        
                        //If chunk does not exist stop
                        //TODO: Fix potential bugs
                        if (IsBlockInEmptyChunk(nextNode)) continue;
                        
                        //Cache light level if not already cached
                        if (!cachedLightLevels.TryGetValue(nextNode, out var nextNodeLightLevel))
                        {
                            nextNodeLightLevel = GetBlockLightLevel(nextNode);
                            cachedLightLevels[nextNode] = nextNodeLightLevel;
                        }

                        //Cache block if not already cached
                        if (!cachedBlocks.TryGetValue(nextNode, out var nextNodeBlock))
                        {
                            nextNodeBlock = GetBlock(nextNode);
                            cachedBlocks[nextNode] = nextNodeBlock;
                        }

                        var newValue = nextNodeBlock.OnLightPassThrough(this, nextNode, node.Value, color);

                        //If the next nodes light level is higher or equal to our value stop
                        if (nextNodeLightLevel[color] >= newValue) continue;

                        //If next node block is an occluder stop
                        if (nextNodeBlock.IsOpaqueFullBlock(this, nextNode)) continue;

                        //Set next nodes light level and advance
                        nextNodeLightLevel[color] = newValue;
                        cachedLightLevels[nextNode] = nextNodeLightLevel;

                        if(newValue > 0)
                            spreadQueue.Enqueue(new LightNode(nextNode, newValue));
                    }
                }
            }

            foreach (var cachedLightLevel in cachedLightLevels)
            {
                SetBlockLightLevel(cachedLightLevel.Key, cachedLightLevel.Value);
            }

            stopwatch.Stop();
            Console.WriteLine($"Light update took: {stopwatch.ElapsedMilliseconds} ms");
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
                    
                var height = OpenSimplexNoise.Generate((worldMin.X + x)*0.06f, (worldMin.Z + z)*0.06f)*5;
                height += OpenSimplexNoise.Generate((worldMin.X + x) * 0.1f, (worldMin.Z + z) * 0.1f)*2;
                //height += OpenSimplexNoise.Generate((worldMin.X + x) * 0.005f, (worldMin.Z + z) * 0.005f) * 10;
                height = 0;
                
                    
                    for (var y = 0; y < Chunk.Size; y++)
                    if (worldMin.Y + y <= height)
                        chunk.SetBlock(x, y, z, (worldMin.Y + y == (int)height) ? GameRegistry.GetBlock("Vanilla:Grass") : GameRegistry.GetBlock("Vanilla:Dirt"));
                       
                    /*
                for (var y = 0; y < Chunk.Size; y++)
                {
                    var density = (OpenSimplexNoise.Generate((worldMin.X + x) * 0.045f, (worldMin.Y + y) * 0.075f, (worldMin.Z + z) * 0.045f) +1)*30;
                    if (density > 13+15)
                    {
                        chunk.SetBlock(x,y,z, GameRegistry.GetBlock("Vanilla:Stone"));
                            if(chunk.GetBlock(x,y-1,z).RegistryKey == "Vanilla:Grass")
                                chunk.SetBlock(x,y-1,z, GameRegistry.GetBlock("Vanilla:Dirt"));
                    }
                    else if (density > 10+15)
                    {
                        chunk.SetBlock(x, y, z,
                            chunk.GetBlock(x, y + 1, z).IsOpaqueFullBlock(this, new Vector3i(x, y + 1, z))
                                ? GameRegistry.GetBlock("Vanilla:Dirt")
                                : GameRegistry.GetBlock("Vanilla:Grass"));

                            if (chunk.GetBlock(x, y - 1, z).RegistryKey == "Vanilla:Grass")
                                chunk.SetBlock(x, y - 1, z, GameRegistry.GetBlock("Vanilla:Dirt"));
                        }
                }
                */
            }


            return chunk;
        }
    }
}