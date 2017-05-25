using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;

namespace MinecraftClone3API.Graphics
{
    public static class BlockTextureManager
    {
        public static readonly int[] Sizes = {16, 64, 256, 1024};
        private static readonly TextureArray[] TextureArrays = new TextureArray[Sizes.Length];
        private static readonly List<TextureData>[] TextureDatas = new List<TextureData>[Sizes.Length];

        static BlockTextureManager()
        {
            for (var i = 0; i < TextureDatas.Length; i++)
                TextureDatas[i] = new List<TextureData>();
        }

        public static void Bind()
        {
            for(var i = 0; i < TextureArrays.Length; i++)
                TextureArrays[i].Bind(TextureUnit.Texture0 + i);
        }

        public static void Upload()
        {
            for (var i = 0; i < Sizes.Length; i++)
            {
                TextureArrays[i]?.Dispose();
                TextureArrays[i] = new TextureArray(Sizes[i], Sizes[i], TextureDatas[i].Count);
                for(var j = 0; j < TextureDatas[i].Count; j++)
                    TextureArrays[i].SetTexture(j, TextureDatas[i][j]);
                TextureArrays[i].GenerateMipmaps();
            }
        }

        internal static BlockTexture LoadTexture(TextureData data)
        {
            var size = Math.Max(data.Width, data.Height);
            
            for (var i = 0; i < Sizes.Length; i++)
            {
                if (size > Sizes[i]) continue;

                var texture = new BlockTexture(i, TextureDatas[i].Count);
                TextureDatas[i].Add(data);
                return texture;
            }

            throw new Exception("Texture is too big!");
        }
    }
}
