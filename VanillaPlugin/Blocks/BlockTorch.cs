using MinecraftClone3API.Blocks;
using MinecraftClone3API.IO;
using MinecraftClone3API.Util;
using OpenTK;
using OpenTK.Input;

namespace VanillaPlugin.Blocks
{
    internal class BlockTorch : Block
    {
        public BlockTorch() : base("Torch")
        {
            Model = ResourceReader.ReadBlockModel("Vanilla/Models/Torch.json");
        }

        public override bool IsFullBlock(WorldBase world, Vector3i blockPos) => false;
        public override bool CanPassThrough(WorldBase world, Vector3i blockPos) => true;

        public override LightLevel GetLightLevel(WorldBase world, Vector3i blockPos)
        {
            var l = new LightLevel(15, 11, 11);
            var ks = Keyboard.GetState();
            if (ks.IsKeyDown(Key.G)) l = new LightLevel(11, 15, 11);
            if (ks.IsKeyDown(Key.B)) l = new LightLevel(11, 11, 15);
            return l;
        }
    }
}
