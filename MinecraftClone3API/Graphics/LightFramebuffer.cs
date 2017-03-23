using MinecraftClone3API.Util;
using OpenTK.Graphics.OpenGL4;

namespace MinecraftClone3API.Graphics
{
    public class TextureFramebuffer : Framebuffer
    {
        public readonly Texture Texture;

        private readonly int _depthId = -1;

        public TextureFramebuffer(int width, int height, bool depthBuffer) : base(width, height)
        {
            Bind();

            Texture = Texture.FromId(GL.GenTexture(), width, height);
            Texture.Bind(TextureUnit.Texture0);
            GL.TexStorage2D(TextureTarget2d.Texture2D, 1, SizedInternalFormat.Rgba8, width, height);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, Texture.Id, 0);

            if (depthBuffer)
            {
                _depthId = GL.GenRenderbuffer();
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _depthId);
                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, width, height);
                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment,
                    RenderbufferTarget.Renderbuffer, _depthId);
            }

            GL.DrawBuffers(1, new[] { DrawBuffersEnum.ColorAttachment0 });
            CheckFramebufferStatus();

            Unbind(ClientResources.Window.Width, ClientResources.Window.Height);
        }
    }
}
