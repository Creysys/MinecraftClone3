﻿using System;
using System.IO;
using MinecraftClone3API.Graphics;
using MinecraftClone3API.Util;

namespace MinecraftClone3API.Blocks
{
    public class Chunk : IDisposable
    {
        public const int Size = 16;
        public const float Radius = 13.856406211853f;

        public readonly World World;
        public readonly Vector3i Position;

        public bool Updated;
        public bool Uploaded;
        public bool NeedsSaving;
        public DateTime Time;

        private readonly uint[,,] _blockIds = new uint[Size, Size, Size];
        
        private Vector3i _min = new Vector3i(Size);
        private Vector3i _max = new Vector3i(-1);

        private readonly VertexArrayObject _vao = new VertexArrayObject();


        public Chunk(World world, Vector3i position)
        {
            World = world;
            Position = position;

            Time = DateTime.Now;
        }

        internal Chunk(CachedChunk cachedChunk) : this(cachedChunk.World, cachedChunk.Position)
        {
            _blockIds = cachedChunk.BlockIds;
            _min = cachedChunk.Min;
            _max = cachedChunk.Max;
        }

        public void SetBlock(int x, int y, int z, uint id)
        {
            if (_blockIds[x, y, z] == id) return;

            NeedsSaving = true;

            _blockIds[x, y, z] = id;

            if (x < _min.X) _min.X = x;
            if (y < _min.Y) _min.Y = y;
            if (z < _min.Z) _min.Z = z;
            if (x > _max.X) _max.X = x;
            if (y > _max.Y) _max.Y = y;
            if (z > _max.Z) _max.Z = z;
        }

        public uint GetBlock(int x, int y, int z)
        {
            if (_blockIds == null || x < 0 || x >= Size || y < 0 || y >= Size || z < 0 || z >= Size)
                return 0;

            return _blockIds[x, y, z];
        }

        public void Update()
        {
            lock (_vao) AddBlocksToVao();
            Updated = true;
        }

        public void Upload()
        {
            lock (_vao)
            {
                _vao.Upload();
                _vao.Clear();
            }
            Uploaded = true;
        }

        public void Draw() => _vao.Draw();

        public void Dispose()
        {
            lock (_vao) _vao.Dispose();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(_min.X);
            writer.Write(_min.Y);
            writer.Write(_min.Z);

            writer.Write(_max.X);
            writer.Write(_max.Y);
            writer.Write(_max.Z);

            for (var x = _min.X; x <= _max.X; x++)
            for (var y = _min.Y; y <= _max.Y; y++)
            for (var z = _min.Z; z <= _max.Z; z++)
                writer.Write(_blockIds[x, y, z]);
        }

        private void AddBlocksToVao()
        {
            for (var x = _min.X; x <= _max.X; x++)
            for (var y = _min.Y; y <= _max.Y; y++)
            for (var z = _min.Z; z <= _max.Z; z++)
                VaoHelper.AddBlockToVao(World, Position * Size + new Vector3i(x, y, z), x, y, z,
                    _blockIds[x, y, z], _vao);

        }
    }
}