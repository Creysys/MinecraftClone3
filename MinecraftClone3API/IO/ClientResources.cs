using System;
using System.Collections.Generic;
using System.IO;
using MinecraftClone3API.Graphics;
using MinecraftClone3API.Util;
using OpenTK;
using OpenTK.Input;

namespace MinecraftClone3API.IO
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

        public static readonly Dictionary<Key, string> Keybindings = new Dictionary<Key, string>();

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

            //TODO: Remove

            var lines = File.ReadAllLines("Keybindings.txt");
            foreach (var line in lines)
            {
                var splits = line.Split('=');

                if (splits.Length != 2) continue;

                if (Enum.TryParse(splits[0], true, out Key key))
                {
                    Keybindings.Add(key, splits[1]);
                }
            }

            var blockModel = ResourceReader.ReadBlockModel("Vanilla/Models/Stairs.json");
            Logger.Debug("lod");
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
