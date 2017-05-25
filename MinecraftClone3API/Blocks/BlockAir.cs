using MinecraftClone3API.Util;

namespace MinecraftClone3API.Blocks
{
    public class BlockAir : Block
    {
        internal BlockAir() : base("Air")
        {
        }

        public override bool IsVisible(WorldBase world, Vector3i blockPos) => false;
        public override bool IsFullBlock(WorldBase world, Vector3i blockPos) => false;
        public override bool CanPassThrough(WorldBase world, Vector3i blockPos) => true;
        public override bool CanTarget(WorldBase world, Vector3i blockPos) => false;
    }
}
