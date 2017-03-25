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
        //Changable in settings
        public const float RenderDistance = 256;
        public const float RenderDistanceSq = RenderDistance * RenderDistance;

        //Changeable in settings
        public const float SortDistance = 128;
        public const float SortDistanceSq = SortDistance * SortDistance;

        public static void RenderWorld(World world, Matrix4 projection)
        {
            var viewProjection = PlayerController.Camera.View * projection;
            var viewFrustum = Frustum.FromViewProjection(viewProjection);

            var wireframe = false;
            if(wireframe) GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);

            DrawGeometryFramebuffer(world, PlayerController.Camera, projection, viewFrustum);
            //DrawLightFramebuffer(world, viewProjection.Inverted(), viewFrustum);

            if(wireframe) GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);

            DrawComposition();
        }

        private static void DrawGeometryFramebuffer(World world, Camera camera, Matrix4 projection, Frustum viewFrustum)
        {
            var chunksToDraw = new List<Chunk>(1024);
            var transparentSortedChunks = new List<Chunk>(1024);
            var transparentChunks = new List<Chunk>(1024);

            foreach (var entry in world.LoadedChunks)
            {
                //Check if chunk is in player view frustum
                var chunkMiddle = (entry.Key * Chunk.Size + new Vector3i(Chunk.Size / 2)).ToVector3();

                if (!viewFrustum.SpehereIntersection(chunkMiddle, Chunk.Radius))
                    continue;

                var lengthSq = (camera.Position - chunkMiddle).LengthSquared;
                if (lengthSq > RenderDistanceSq) continue;

                if (entry.Value.HasTransparency)
                {
                    if (lengthSq < SortDistanceSq)
                    {
                        entry.Value.SortTransparentFaces();
                        transparentSortedChunks.Add(entry.Value);
                    }
                    else
                    {
                        transparentChunks.Add(entry.Value);
                    }
                }
                else chunksToDraw.Add(entry.Value);
            }

            //Sort transparent chunks and append to draw list
            var cameraPos = camera.Position;
            transparentSortedChunks.Sort((chunk1, chunk2)
                => (int) ((cameraPos - chunk2.Middle).LengthSquared * 1000 -
                          (cameraPos - chunk1.Middle).LengthSquared * 1000));

            transparentChunks.AddRange(transparentSortedChunks);
            chunksToDraw.AddRange(transparentChunks);

            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);

            ClientResources.GeometryFramebuffer.Bind();
            ClientResources.GeometryFramebuffer.Clear(Color4.DarkBlue);
            //ClientResources.GeometryFramebuffer.Clear(Color4.Transparent);    Breaks transparency

            ClientResources.WorldGeometryShader.Bind();
            GL.UniformMatrix4(4, false, ref camera.View);
            GL.UniformMatrix4(8, false, ref projection);
            GL.Uniform1(12, 1);

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
            GL.Uniform1(12, 0);

            //Draw transparent blocks back to front
            foreach (var chunk in transparentChunks)
            {
                var worldMat = Matrix4.CreateTranslation(chunk.Position.X * Chunk.Size, chunk.Position.Y * Chunk.Size,
                    chunk.Position.Z * Chunk.Size);
                GL.UniformMatrix4(0, false, ref worldMat);
                chunk.DrawTransparent();
            }

            GL.Disable(EnableCap.Blend);

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

            //GL.Enable(EnableCap.Blend);

            ClientResources.GeometryFramebuffer.BindTexturesAndSamplers();
            ClientResources.LightFramebuffer.Texture.Bind(TextureUnit.Texture3);
            ClientResources.CompositionShader.Bind();
            ClientResources.ScreenRectVao.Draw();

            //GL.Disable(EnableCap.Blend);
        }
    }
}