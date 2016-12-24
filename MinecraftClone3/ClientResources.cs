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
        public static TextureFramebuffer LightFramebuffer;

        public static Shader WorldGeometryShader;
        public static Shader CompositionShader;
        public static Shader PointLightShader;

        public static VertexArrayObject ScreenRectVao;

        public static void Load()
        {
            ResizeFrameBuffers();
            Program.Window.Resize += (sender, args) => ResizeFrameBuffers();

            WorldGeometryShader = ResourceReader.ReadShader(PluginDir + "Shaders/WorldGeometry");
            CompositionShader = ResourceReader.ReadShader(PluginDir + "Shaders/Composition");
            PointLightShader = ResourceReader.ReadShader(PluginDir + "Shaders/PointLight");

            ScreenRectVao = new VertexArrayObject();
            ScreenRectVao.Add(new Vector3(-1, +1, 0), Vector4.Zero, Vector4.Zero, Vector3.Zero, Vector4.Zero, Vector4.Zero);
            ScreenRectVao.Add(new Vector3(+1, +1, 0), Vector4.Zero, Vector4.Zero, Vector3.Zero, Vector4.Zero, Vector4.Zero);
            ScreenRectVao.Add(new Vector3(-1, -1, 0), Vector4.Zero, Vector4.Zero, Vector3.Zero, Vector4.Zero, Vector4.Zero);
            ScreenRectVao.Add(new Vector3(+1, -1, 0), Vector4.Zero, Vector4.Zero, Vector3.Zero, Vector4.Zero, Vector4.Zero);
            ScreenRectVao.AddIndices(new uint[] {0, 2, 1, 1, 2, 3});
            ScreenRectVao.Upload();

            Samplers.Load();
        }

        private static void ResizeFrameBuffers()
        {
            GeometryFramebuffer?.Dispose();
            GeometryFramebuffer = new GeometryFramebuffer(Program.Window.Width, Program.Window.Height);

            LightFramebuffer?.Dispose();
            LightFramebuffer = new TextureFramebuffer(Program.Window.Width, Program.Window.Height, false);
        }
    }
}
