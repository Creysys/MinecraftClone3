using OpenTK;
using OpenTK.Graphics;

namespace MinecraftClone3API.Util
{
    public static class ExtensionHelper
    {
        public static Vector4 ToVector4(this Color4 color) => new Vector4(color.R, color.G, color.B, color.A);
    }
}
