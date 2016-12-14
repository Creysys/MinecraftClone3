using MinecraftClone3.Graphics;
using MinecraftClone3API.Graphics;
using MinecraftClone3API.IO;
using OpenTK;

namespace MinecraftClone3
{
    internal static class ClientResources
    {
        private const string PluginDir = "Client/";

        public static GeometryFramebuffer GeometryFramebuffer;

        public static Shader WorldGeometryShader;
        public static Shader CompositionShader;

        public static VertexArrayObject ScreenRectVao;

        public static void Load()
        {
            ResizeGeometryBuffer();
            Program.Window.Resize += (sender, args) => ResizeGeometryBuffer();

            WorldGeometryShader = ResourceReader.ReadShader(PluginDir + "Shaders/WorldGeometry");
            CompositionShader = ResourceReader.ReadShader(PluginDir + "Shaders/Composition");

            ScreenRectVao = new VertexArrayObject();
            ScreenRectVao.Add(new Vector3(-1, +1, 0), Vector4.Zero, Vector3.Zero);
            ScreenRectVao.Add(new Vector3(+1, +1, 0), Vector4.Zero, Vector3.Zero);
            ScreenRectVao.Add(new Vector3(-1, -1, 0), Vector4.Zero, Vector3.Zero);
            ScreenRectVao.Add(new Vector3(+1, -1, 0), Vector4.Zero, Vector3.Zero);
            ScreenRectVao.AddIndices(new uint[] {0, 2, 1, 1, 2, 3});
            ScreenRectVao.Upload();

            Samplers.Load();
        }

        private static void ResizeGeometryBuffer()
        {
            GeometryFramebuffer?.Dispose();
            GeometryFramebuffer = new GeometryFramebuffer(Program.Window.Width, Program.Window.Height);
        }
    }
}
