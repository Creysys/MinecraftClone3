using OpenTK.Graphics.OpenGL4;
using GLPixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;
using ExtTextureFilterAnisotropic = OpenTK.Graphics.OpenGL.ExtTextureFilterAnisotropic;

namespace MinecraftClone3API.Graphics
{
    internal class Texture
    {
        private readonly int _id;
        public readonly int Height;

        public readonly int Width;

        public Texture(string filename)
        {
            var data = new TextureData(filename);

            _id = GL.GenTexture();
            Width = data.Width;
            Height = data.Height;

            Bind(TextureUnit.Texture0);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, GLPixelFormat.Bgra,
                PixelType.UnsignedByte, data.DataPtr);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            data.Dispose();
        }

        public void Bind(TextureUnit textureUnit)
        {
            GL.ActiveTexture(textureUnit);
            GL.BindTexture(TextureTarget.Texture2D, _id);
        }
    }
}