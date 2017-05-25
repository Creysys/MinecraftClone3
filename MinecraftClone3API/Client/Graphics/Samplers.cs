﻿using OpenTK.Graphics.OpenGL4;

namespace MinecraftClone3API.Graphics
{
    public static class Samplers
    {
        private static int _blockTexture;
        private static int _framebufferTexture;

        public static void Load()
        {
            _blockTexture = GL.GenSampler();
            GL.SamplerParameter(_blockTexture, SamplerParameterName.TextureMinFilter, (float)TextureMinFilter.LinearMipmapLinear);
            GL.SamplerParameter(_blockTexture, SamplerParameterName.TextureMagFilter, (float)TextureMinFilter.Nearest);
            GL.SamplerParameter(_blockTexture, SamplerParameterName.TextureMaxAnisotropyExt, 16);

            _framebufferTexture = GL.GenSampler();
            GL.SamplerParameter(_blockTexture, SamplerParameterName.TextureMinFilter, (float)TextureMinFilter.Nearest);
            GL.SamplerParameter(_blockTexture, SamplerParameterName.TextureMagFilter, (float)TextureMinFilter.Nearest);
        }

        public static void BindBlockTextureSampler()
        {
            for (var x = 0; x < BlockTextureManager.Sizes.Length; x++)
                GL.BindSampler(x, _blockTexture);
        }

        public static void BindFramebufferTextureSampler(int unit) => GL.BindSampler(unit, _framebufferTexture);
    }
}
