using MinecraftClone3API.Util;

namespace MinecraftClone3API.Blocks
{
    public class BlockAir : Block
    {
        internal BlockAir() : base("Air")
        {
        }

        public override bool IsVisible(World world, Vector3i blockPos) => false;
        public override bool IsFullBlock(World world, Vector3i blockPos) => true;
        public override bool CanPassThrough(World world, Vector3i blockPos) => true;
        public override bool CanTarget(World world, Vector3i blockPos) => false;
    }
}
