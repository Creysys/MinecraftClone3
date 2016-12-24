using System.Collections.Generic;
using System.Linq;
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
            var viewProjection = camera.View * projection;
            var viewFrustum = Frustum.FromViewProjection(viewProjection);

            DrawGeometryFramebuffer(world, camera, projection, viewFrustum);
            DrawLightFramebuffer(world, viewProjection.Inverted(), viewFrustum);
            DrawComposition();
        }

        private static void DrawGeometryFramebuffer(World world, Camera camera, Matrix4 projection, Frustum viewFrustum)
        {
            var chunksToDraw = new List<Chunk>();
            foreach (var entry in world.LoadedChunks)
            {
                //Check if chunk is in player view frustum
                var chunkMiddle = (entry.Key * Chunk.Size + new Vector3i(Chunk.Size / 2)).ToVector3();
                if (!viewFrustum.SpehereIntersection(chunkMiddle, Chunk.Radius))
                    continue;

                chunksToDraw.Add(entry.Value);
            }


            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);

            ClientResources.GeometryFramebuffer.Bind();
            ClientResources.GeometryFramebuffer.Clear(Color4.Transparent);

            ClientResources.WorldGeometryShader.Bind();
            GL.UniformMatrix4(4, false, ref camera.View);
            GL.UniformMatrix4(8, false, ref projection);

            BlockTextureManager.Bind();
            Samplers.BindBlockTextureSampler();
            
            foreach (var chunk in chunksToDraw)
            {
                var worldMat = Matrix4.CreateTranslation(chunk.Position.X * Chunk.Size, chunk.Position.Y * Chunk.Size,
                    chunk.Position.Z * Chunk.Size);
                GL.UniformMatrix4(0, false, ref worldMat);
                chunk.Draw();
            }
            ClientResources.GeometryFramebuffer.Unbind(Program.Window.Width, Program.Window.Height);
        }

        private static void DrawLightFramebuffer(World world, Matrix4 viewProjectionInv, Frustum viewFrustum)
        {
            //TODO: Lighting?

            /*
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);

            ClientResources.LightFramebuffer.Bind();
            ClientResources.LightFramebuffer.Clear(Color4.Black);

            ClientResources.PointLightShader.Bind();
            GL.UniformMatrix4(0, false, ref viewProjectionInv);

            
            foreach (var light in world.Lights.Select(light => light as PointLight))
            {
                if (light == null || !viewFrustum.SpehereIntersection(light.Position, light.Range)) continue;
                GL.Uniform3(4, light.Position);
                GL.Uniform3(5, light.Color);
                GL.Uniform1(6, light.Range);
                ClientResources.ScreenRectVao.Draw();
            }
            
            ClientResources.LightFramebuffer.Unbind(Program.Window.Width, Program.Window.Height);

            GL.Disable(EnableCap.Blend);
            */
        }

        private static void DrawComposition()
        {
            GL.ClearColor(Color4.DarkBlue);
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit | ClearBufferMask.ColorBufferBit);

            ClientResources.GeometryFramebuffer.BindTexturesAndSamplers();
            ClientResources.LightFramebuffer.Texture.Bind(TextureUnit.Texture3);
            ClientResources.CompositionShader.Bind();
            ClientResources.ScreenRectVao.Draw();
        }
    }
}