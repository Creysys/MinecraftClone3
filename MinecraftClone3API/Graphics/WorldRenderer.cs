using System.Collections.Generic;
using MinecraftClone3API.Blocks;
using MinecraftClone3API.Entities;
using MinecraftClone3API.Util;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace MinecraftClone3API.Graphics
{
    public static class WorldRenderer
    {
        public const float RenderDistance = 256;
        public const float RenderDistanceSq = RenderDistance * RenderDistance;

        public const float SortDistance = 32;
        public const float SortDistanceSq = SortDistance * SortDistance;

        public static void RenderWorld(World world, Matrix4 projection)
        {
            var viewProjection = PlayerController.Camera.View * projection;
            var viewFrustum = Frustum.FromViewProjection(viewProjection);

            //Wireframe
            //GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);

            DrawGeometryFramebuffer(world, PlayerController.Camera, projection, viewFrustum);
            //DrawLightFramebuffer(world, viewProjection.Inverted(), viewFrustum);

            //GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);

            DrawComposition();
        }

        private static void DrawGeometryFramebuffer(World world, Camera camera, Matrix4 projection, Frustum viewFrustum)
        {
            var chunksToDraw = new List<Chunk>();
            var chunksToSort = new List<Chunk>();

            foreach (var entry in world.LoadedChunks)
            {
                //Check if chunk is in player view frustum
                var chunkMiddle = (entry.Key * Chunk.Size + new Vector3i(Chunk.Size / 2)).ToVector3();

                if (!viewFrustum.SpehereIntersection(chunkMiddle, Chunk.Radius))
                    continue;

                var lengthSq = (camera.Position - chunkMiddle).LengthSquared;
                if (lengthSq > RenderDistanceSq) continue;

                if(entry.Value.HasTransparency && lengthSq < SortDistanceSq)
                    chunksToSort.Add(entry.Value);
                else
                    chunksToDraw.Add(entry.Value);
            }

            var cameraPos = camera.Position;
            chunksToSort.Sort((chunk1, chunk2)
                => (int) ((cameraPos - chunk1.Middle).LengthSquared * 1000 -
                          (cameraPos - chunk2.Middle).LengthSquared * 1000));

            chunksToSort.AddRange(chunksToDraw);
            chunksToDraw = chunksToSort;

            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);

            ClientResources.GeometryFramebuffer.Bind();
            ClientResources.GeometryFramebuffer.Clear(Color4.Transparent);

            ClientResources.WorldGeometryShader.Bind();
            GL.UniformMatrix4(4, false, ref camera.View);
            GL.UniformMatrix4(8, false, ref projection);

            BlockTextureManager.Bind();
            Samplers.BindBlockTextureSampler();
            
            //Draw opaque blocks front to back
            foreach (var chunk in chunksToDraw)
            {
                var worldMat = Matrix4.CreateTranslation(chunk.Position.X * Chunk.Size, chunk.Position.Y * Chunk.Size,
                    chunk.Position.Z * Chunk.Size);
                GL.UniformMatrix4(0, false, ref worldMat);
                chunk.Draw();
            }

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            //Draw transparent block back to front
            for (var i = chunksToDraw.Count - 1; i >= 0; i--)
            {
                var chunk = chunksToDraw[i];
                var worldMat = Matrix4.CreateTranslation(chunk.Position.X * Chunk.Size, chunk.Position.Y * Chunk.Size,
                    chunk.Position.Z * Chunk.Size);
                GL.UniformMatrix4(0, false, ref worldMat);
                chunk.DrawTransparent();
            }

            //TODO: Entities
            PlayerController.Render(camera, projection);

            ClientResources.GeometryFramebuffer.Unbind(ClientResources.Window.Width, ClientResources.Window.Height);
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