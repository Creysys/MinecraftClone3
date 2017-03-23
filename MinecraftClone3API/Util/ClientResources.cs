using MinecraftClone3API.Graphics;
using MinecraftClone3API.IO;
using OpenTK;

namespace MinecraftClone3API.Util
{
    public static class ClientResources
    {
        private const string PluginDir = "Client/";

        public static GameWindow Window;

        public static GeometryFramebuffer GeometryFramebuffer;
        public static TextureFramebuffer LightFramebuffer;

        public static Shader WorldGeometryShader;
        public static Shader CompositionShader;
        public static Shader PointLightShader;
        public static Shader BlockOutlineShader;

        public static VertexArrayObject ScreenRectVao;

        public static void Load(GameWindow window)
        {
            Window = window;

            ResizeFrameBuffers();
            Window.Resize += (sender, args) => ResizeFrameBuffers();

            WorldGeometryShader = ResourceReader.ReadShader(PluginDir + "Shaders/WorldGeometry");
            CompositionShader = ResourceReader.ReadShader(PluginDir + "Shaders/Composition");
            PointLightShader = ResourceReader.ReadShader(PluginDir + "Shaders/PointLight");
            BlockOutlineShader = ResourceReader.ReadShader(PluginDir + "Shaders/BlockOutline");

            ScreenRectVao = new VertexArrayObject();
            ScreenRectVao.Add(new Vector3(-1, +1, 0), Vector4.Zero, Vector4.Zero, Vector4.Zero, Vector4.Zero, Vector4.Zero);
            ScreenRectVao.Add(new Vector3(+1, +1, 0), Vector4.Zero, Vector4.Zero, Vector4.Zero, Vector4.Zero, Vector4.Zero);
            ScreenRectVao.Add(new Vector3(-1, -1, 0), Vector4.Zero, Vector4.Zero, Vector4.Zero, Vector4.Zero, Vector4.Zero);
            ScreenRectVao.Add(new Vector3(+1, -1, 0), Vector4.Zero, Vector4.Zero, Vector4.Zero, Vector4.Zero, Vector4.Zero);
            ScreenRectVao.AddFace(new uint[] {0, 2, 1, 1, 2, 3}, Vector3.Zero);
            ScreenRectVao.Upload();

            Samplers.Load();
        }

        private static void ResizeFrameBuffers()
        {
            GeometryFramebuffer?.Dispose();
            GeometryFramebuffer = new GeometryFramebuffer(Window.Width, Window.Height);

            LightFramebuffer?.Dispose();
            LightFramebuffer = new TextureFramebuffer(Window.Width, Window.Height, false);
        }
    }
}
