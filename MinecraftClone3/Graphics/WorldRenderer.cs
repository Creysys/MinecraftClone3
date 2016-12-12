using MinecraftClone3API.Blocks;
using MinecraftClone3API.Graphics;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace MinecraftClone3.Graphics
{
    internal static class WorldRenderer
    {
        public static void RenderWorld(World world)
        {
            BlockTextureManager.Bind();

            foreach (var entry in world.LoadedChunks)
            {
                var worldMat = Matrix4.CreateTranslation(entry.Key.X * Chunk.Size, entry.Key.Y * Chunk.Size,
                    entry.Key.Z * Chunk.Size);
                GL.UniformMatrix4(0, false, ref worldMat);
                entry.Value.Draw();
            }
        }
    }
}