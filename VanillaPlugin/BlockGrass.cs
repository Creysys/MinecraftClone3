using System;
using MinecraftClone3API.Blocks;
using MinecraftClone3API.Graphics;
using MinecraftClone3API.IO;
using MinecraftClone3API.Util;
using OpenTK;
using OpenTK.Graphics;

namespace VanillaPlugin
{
    internal class BlockGrass : Block
    {
        private readonly BlockTexture _topTexture;
        private readonly BlockTexture _overlayTexture;
        private readonly BlockTexture _dirtTexture;

        public BlockGrass() : base("Grass")
        {
            _topTexture = ResourceReader.ReadBlockTexture("Vanilla/Textures/Blocks/GrassTop.png");
            _overlayTexture = ResourceReader.ReadBlockTexture("Vanilla/Textures/Blocks/GrassOverlay.png");
            _dirtTexture = ResourceReader.ReadBlockTexture("Vanilla/Textures/Blocks/Dirt.png");
        }

        public override BlockTexture GetTexture(World world, Vector3i blockPos, BlockFace face)
            => face == BlockFace.Top ? _topTexture : _dirtTexture;

        public override BlockTexture GetOverlayTexture(World world, Vector3i blockPos, BlockFace face)
            => face == BlockFace.Top || face == BlockFace.Bottom ? null : _overlayTexture;

        public override Color4 GetColor(World world, Vector3i blockPos, BlockFace face)
            => face == BlockFace.Top ? GetCurrentColor(blockPos) : Color4.White;
        public override Color4 GetOverlayColor(World world, Vector3i blockPos, BlockFace face) => GetCurrentColor(blockPos);

        private Color4 GetCurrentColor(Vector3i blockPos)
        {
            return new Color4((float) Math.Sin(blockPos.X * 0.02f)*0.5f+0.5f, (float) Math.Cos(blockPos.Z * 0.02f) * 0.5f + 0.5f,
                (float) (Math.Sin(blockPos.X * 0.02f) * (float) Math.Cos(blockPos.Z * 0.02f)) * 0.5f + 0.5f, 1);
        }
    }
}
