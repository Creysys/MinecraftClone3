using MinecraftClone3API.Blocks;
using MinecraftClone3API.Util;

namespace VanillaPlugin.Blocks
{
    public class BlockGlass : BlockBasic
    {
        public BlockGlass() : base("Glass", "Vanilla/Textures/Blocks/Glass.png", false)
        {
        }

        public override TransparencyType IsTransparent(World world, Vector3i blockPos) => TransparencyType.Cutoff;

        public override ConnectionType ConnectsToBlock(World world, Vector3i blockPos, Vector3i otherBlockPos,
            Block otherBlock) => otherBlock == this ? ConnectionType.Connected : ConnectionType.Undefined;
    }
}
