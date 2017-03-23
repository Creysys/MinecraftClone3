using MinecraftClone3API.Blocks;
using MinecraftClone3API.Util;

namespace VanillaPlugin
{
    class BlockGlass : BlockBasic
    {
        public BlockGlass() : base("Glass", "Vanilla/Textures/Blocks/Glass.png")
        {
        }
        
        public override bool IsTransparent(World world, Vector3i blockPos) => true;
    }
}
