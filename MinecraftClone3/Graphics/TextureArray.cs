using OpenTK.Graphics.OpenGL4;
using ExtTextureFilterAnisotropic = OpenTK.Graphics.OpenGL.ExtTextureFilterAnisotropic;

namespace MinecraftClone3.Graphics
{
    internal class TextureArray
    {
        public readonly int Width;
        public readonly int Height;

        private readonly int _id;

        public TextureArray(int width, int height, int count)
        {
            Width = width;
            Height = height;

            _id = GL.GenTexture();
            Bind(TextureUnit.Texture0);
            GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, SizedInternalFormat.Rgba8, width, height, count);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2DArray, (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, 16);
        }

        public void SetTexture(int index, TextureData data)
        {
            GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, index, data.Width, data.Height, 1, PixelFormat.Bgra,
                PixelType.UnsignedByte, data.DataPtr);
            data.Dispose();
        }

        public void GenerateMipmaps()
        {
            Bind(TextureUnit.Texture0);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2DArray);
        }

        public void Bind(TextureUnit textureUnit)
        {
            GL.ActiveTexture(textureUnit);
            GL.BindTexture(TextureTarget.Texture2DArray, _id);
        }
    }
}