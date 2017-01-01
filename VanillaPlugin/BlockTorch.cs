using MinecraftClone3API.Blocks;
using MinecraftClone3API.IO;
using MinecraftClone3API.Util;
using OpenTK;

namespace VanillaPlugin
{
    internal class BlockTorch : Block
    {
        private const float PixelSize = 1 / 16f;

        private static readonly Matrix4 Transform = Matrix4.CreateScale(PixelSize * 2, PixelSize * 10, PixelSize * 2) *
                                                    Matrix4.CreateTranslation(0, -PixelSize * 3, 0);

        private static readonly Vector2[] TopTexCoords = new[]
        {
            new Vector2(7, 6) * PixelSize, new Vector2(9, 6) * PixelSize,
            new Vector2(7, 8) * PixelSize, new Vector2(9, 8) * PixelSize,
        };

        private static readonly Vector2[] BottomTexCoords = new[]
        {
            new Vector2(7, 14) * PixelSize, new Vector2(9, 14) * PixelSize,
            new Vector2(7, 16) * PixelSize, new Vector2(9, 16) * PixelSize,
        };

        private static readonly Vector2[] SideTexCoords = new[]
        {
            new Vector2(7, 6) * PixelSize, new Vector2(9, 6) * PixelSize,
            new Vector2(7, 16) * PixelSize, new Vector2(9, 16) * PixelSize,
        };

        public BlockTorch() : base("Torch")
        {
            Texture = ResourceReader.ReadBlockTexture("Vanilla/Textures/Blocks/Torch.png");
        }

        public override bool IsOpaqueFullBlock(World world, Vector3i blockPos) => false;
        public override bool CanPassThrough(World world, Vector3i blockPos) => true;

        public override Vector2[] GetTexCoords(World world, Vector3i blockPos, BlockFace face)
            => face == BlockFace.Top ? TopTexCoords : face == BlockFace.Bottom ? BottomTexCoords : SideTexCoords;

        public override Matrix4 GetTransform(World world, Vector3i blockPos, BlockFace face) => Transform;

        public override LightLevel GetLightLevel(World world, Vector3i blockPos) => new LightLevel(15, 11, 11);
    }
}
