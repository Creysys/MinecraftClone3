using System.IO;
using MinecraftClone3.Graphics;
using MinecraftClone3.Utils;

namespace MinecraftClone3.Blocks
{
    internal class CachedChunk
    {
        public readonly World World;
        public readonly Vector3i Position;
        public readonly uint[,,] BlockIds = new uint[Chunk.Size, Chunk.Size, Chunk.Size];

        public bool IsEmpty => Min.X == Chunk.Size;

        public Vector3i Min = new Vector3i(Chunk.Size);
        public Vector3i Max = new Vector3i(-1);

        public CachedChunk(World world, Vector3i position)
        {
            World = world;
            Position = position;
        }

        public CachedChunk(World world, Vector3i position, BinaryReader reader) : this(world, position)
        {
            Min = new Vector3i(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
            Max = new Vector3i(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());

            for (var x = Min.X; x <= Max.X; x++)
            for (var y = Min.Y; y <= Max.Y; y++)
            for (var z = Min.Z; z <= Max.Z; z++)
            {
                BlockIds[x, y, z] = reader.ReadUInt32();
            }
        }

        public void SetBlock(int x, int y, int z, uint id)
        {
            if (BlockIds[x, y, z] == id) return;

            BlockIds[x, y, z] = id;

            if (x < Min.X) Min.X = x;
            if (y < Min.Y) Min.Y = y;
            if (z < Min.Z) Min.Z = z;
            if (x > Max.X) Max.X = x;
            if (y > Max.Y) Max.Y = y;
            if (z > Max.Z) Max.Z = z;
        }
    }
}