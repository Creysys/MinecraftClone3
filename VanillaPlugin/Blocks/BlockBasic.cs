using MinecraftClone3API.Blocks;
using MinecraftClone3API.IO;

namespace VanillaPlugin.Blocks
{
    public class BlockBasic : Block
    {
        public BlockBasic(string name, string texturePath) : base(name)
        {
            Texture = ResourceReader.ReadBlockTexture(texturePath);
        }
    }
}
