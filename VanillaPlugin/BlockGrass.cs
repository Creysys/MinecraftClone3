using MinecraftClone3API.Blocks;
using MinecraftClone3API.Graphics;
using MinecraftClone3API.IO;
using MinecraftClone3API.Util;

namespace VanillaPlugin
{
    internal class BlockGrass : Block
    {
        private readonly BlockTexture TopTexture;
        private readonly BlockTexture BottomTexture;
        private readonly BlockTexture SideTexture;

        public BlockGrass() : base("Grass")
        {
            TopTexture = ResourceReader.ReadBlockTexture("Vanilla/Textures/Blocks/GrassTop.png");
            BottomTexture = ResourceReader.ReadBlockTexture("Vanilla/Textures/Blocks/GrassBottom.png");
            SideTexture = ResourceReader.ReadBlockTexture("Vanilla/Textures/Blocks/GrassSide.png");
        }

        public override BlockTexture GetTexture(World world, Vector3i blockPos, BlockFace face)
            => face == BlockFace.Top ? TopTexture : face == BlockFace.Bottom ? BottomTexture : SideTexture;
    }
}
