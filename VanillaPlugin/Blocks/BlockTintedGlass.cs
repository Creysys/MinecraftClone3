using MinecraftClone3API.Blocks;
using MinecraftClone3API.Entities;
using MinecraftClone3API.Graphics;
using MinecraftClone3API.IO;
using MinecraftClone3API.Util;
using OpenTK.Input;
using VanillaPlugin.BlockDatas;

namespace VanillaPlugin.Blocks
{
    public class BlockTintedGlass : Block
    {
        private static readonly Key[] Keys = {Key.U, Key.I, Key.O, Key.P};
        private static readonly string[] TextureNames =
        {
            "Vanilla/Textures/Blocks/glass_black.png",
            "Vanilla/Textures/Blocks/glass_blue.png",
            "Vanilla/Textures/Blocks/glass_green.png",
            "Vanilla/Textures/Blocks/glass_magenta.png"
        };

        private static readonly int[,] LightFilters =
        {
            {4, 4, 4},
            {10, 10, 1 },
            {10, 1, 10 },
            {1, 10, 10 }
        };

        private static BlockTexture[] _textures;

        public BlockTintedGlass() : base("TintedGlass")
        {
            _textures = new BlockTexture[TextureNames.Length];
            for (var i = 0; i < TextureNames.Length; i++)
            {
                _textures[i] = ResourceReader.ReadBlockTexture(TextureNames[i]);
            }
        }

        public override TransparencyType IsTransparent(World world, Vector3i blockPos) => TransparencyType.Transparent;
        public override ConnectionType ConnectsToBlock(World world, Vector3i blockPos, Vector3i otherBlockPos,
            Block otherBlock) => otherBlock == this ? ConnectionType.Connected : ConnectionType.Undefined;

        public override BlockTexture GetTexture(World world, Vector3i blockPos, BlockFace face)
        {
            var data = world.GetBlockData(blockPos) as BlockDataMetadata;
            return data == null ? _textures[0] : _textures[data.Metadata];
        }

        public override void OnPlaced(World world, Vector3i blockPos, EntityPlayer player)
        {
            var m = 0;
            var ks = Keyboard.GetState();
            for (var i = 0; i < Keys.Length; i++)
            {
                if (!ks.IsKeyDown(Keys[i])) continue;

                m = i;
                break;
            }

            world.SetBlockData(blockPos, new BlockDataMetadata(m));
        }

        public override int OnLightPassThrough(World world, Vector3i blockPos, int lightLevel, int color)
        {
            var data = world.GetBlockData(blockPos) as BlockDataMetadata;
            var i = data?.Metadata ?? 0;
            return lightLevel - LightFilters[i, color];
        }
    }
}
