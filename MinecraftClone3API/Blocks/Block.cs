using MinecraftClone3API.Graphics;
using MinecraftClone3API.Util;

namespace MinecraftClone3API.Blocks
{
    public class Block : RegistryEntry
    {
        public string TextureResource;
        public BlockTexture Texture;

        public Block(string name) : base(name)
        {
        }

        public uint Id { get; internal set; }

        public virtual bool IsVisible(World world, Vector3i blockPos) => true;
        public virtual bool IsOpaque(World world, Vector3i blockPos) => true;
        public virtual bool CanPassThrough(World world, Vector3i blockPos) => false;

        public virtual BlockTexture GetTexture(World world, Vector3i blockPos, BlockFace face) => Texture;
    }
}
