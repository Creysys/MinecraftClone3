using MinecraftClone3API.Blocks;
using MinecraftClone3API.IO;

namespace VanillaPlugin
{
    internal class BlockBasic : Block
    {
        public BlockBasic(string name, string texturePath) : base(name)
        {
            Texture = ResourceReader.ReadBlockTexture(texturePath);
        }
    }
}
