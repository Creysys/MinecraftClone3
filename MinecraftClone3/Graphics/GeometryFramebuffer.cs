﻿using MinecraftClone3API.Graphics;
using MinecraftClone3API.Util;
using OpenTK.Graphics.OpenGL4;

namespace MinecraftClone3.Graphics
{
    internal sealed class GeometryFramebuffer : Framebuffer
    {
        private readonly int _diffuse;
        private readonly int _normal;
        private readonly int _depth;

        public GeometryFramebuffer(int width, int height) : base(width, height)
        {
            Bind();

            _diffuse = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _diffuse);
            GL.TexStorage2D(TextureTarget2d.Texture2D, 1, SizedInternalFormat.Rgba8, width, height);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, _diffuse, 0);

            _normal = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _normal);
            GL.TexStorage2D(TextureTarget2d.Texture2D, 1, SizedInternalFormat.Rgba8, width, height);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, _normal, 0);

            _depth = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _depth);
            GL.TexStorage2D(TextureTarget2d.Texture2D, 1, (SizedInternalFormat)33190, width, height);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, _depth, 0);

            GL.DrawBuffers(2, new []{DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1});
            var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if(status != FramebufferErrorCode.FramebufferComplete)
                Logger.Error("Error creating geometry framebuffer!");

            Unbind(Program.Window.Width, Program.Window.Height);
        }

        public void BindTextures()
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _diffuse);
            Samplers.BindFramebufferTextureSampler(0);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, _normal);
            Samplers.BindFramebufferTextureSampler(1);

            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, _depth);
            Samplers.BindFramebufferTextureSampler(2);
        }

        public override void Dispose()
        {
            base.Dispose();
            GL.DeleteTexture(_diffuse);
            GL.DeleteTexture(_normal);
            GL.DeleteTexture(_depth);
        }
    }
}
