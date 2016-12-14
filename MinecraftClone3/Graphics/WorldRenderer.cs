using System.Collections.Generic;
using MinecraftClone3API.Blocks;
using MinecraftClone3API.Graphics;
using MinecraftClone3API.Util;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace MinecraftClone3.Graphics
{
    internal static class WorldRenderer
    {
        public static void RenderWorld(World world, Camera camera, Matrix4 projection)
        {
            ClientResources.WorldGeometryShader.Bind();
            GL.UniformMatrix4(4, false, ref camera.View);
            GL.UniformMatrix4(8, false, ref projection);

            BlockTextureManager.Bind();
            Samplers.BindBlockTextureSampler();

            var viewFrustum = Frustum.FromViewProjection(camera.View * projection);

            var chunksToDraw = new List<Chunk>();
            foreach(var entry in world.LoadedChunks)
            {
                //Check if chunk is in player view frustum
                var chunkMiddle = (entry.Key * Chunk.Size + new Vector3i(Chunk.Size / 2)).ToVector3();
                if (!viewFrustum.SpehereIntersection(chunkMiddle, Chunk.Radius))
                    continue;

                chunksToDraw.Add(entry.Value);
            }

            //Draw chunks into geometry framebuffer
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);

            ClientResources.GeometryFramebuffer.Bind();
            ClientResources.GeometryFramebuffer.Clear(Color4.Transparent);
            foreach (var chunk in chunksToDraw)
            {
                var worldMat = Matrix4.CreateTranslation(chunk.Position.X * Chunk.Size, chunk.Position.Y * Chunk.Size,
                    chunk.Position.Z * Chunk.Size);
                GL.UniformMatrix4(0, false, ref worldMat);
                chunk.Draw();
            }
            ClientResources.GeometryFramebuffer.Unbind(Program.Window.Width, Program.Window.Height);

            //Draw composition
            GL.ClearColor(Color4.DarkBlue);
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit | ClearBufferMask.ColorBufferBit);

            ClientResources.GeometryFramebuffer.BindTextures();
            ClientResources.CompositionShader.Bind();
            ClientResources.ScreenRectVao.Draw();
        }
    }
}