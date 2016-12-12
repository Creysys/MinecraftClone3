using MinecraftClone3API.Util;

namespace MinecraftClone3API.Blocks
{
    public class BlockAir : Block
    {
        public BlockAir() : base("Air")
        {
        }

        public override bool IsVisible(World world, Vector3i blockPos) => false;
        public override bool CanPassThrough(World world, Vector3i blockPos) => true;
    }
}
