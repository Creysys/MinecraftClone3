using MinecraftClone3API.Client;
using MinecraftClone3API.IO;
using MinecraftClone3API.Util;
using OpenTK.Graphics.OpenGL4;

namespace MinecraftClone3API.Graphics
{
    public sealed class GeometryFramebuffer : Framebuffer
    {
        private readonly int _diffuse;
        private readonly int _normal;
        private readonly int _light;
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

            _light = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _light);
            GL.TexStorage2D(TextureTarget2d.Texture2D, 1, (SizedInternalFormat)32849, width, height); //GL_RGB8
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment2, _light, 0);

            _depth = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _depth);
            GL.TexStorage2D(TextureTarget2d.Texture2D, 1, (SizedInternalFormat)33190, width, height); //GL_DEPTH_COMPONENT24
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, _depth, 0);

            GL.DrawBuffers(2, new []{DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1});
            CheckFramebufferStatus();

            Unbind(ClientResources.Window.Width, ClientResources.Window.Height);
        }

        public void BindTexturesAndSamplers()
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
