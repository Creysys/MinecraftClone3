using System;
using System.IO;
using MinecraftClone3.Entities;
using MinecraftClone3.Graphics;
using MinecraftClone3API.Blocks;
using MinecraftClone3API.Entities;
using MinecraftClone3API.Graphics;
using MinecraftClone3API.Plugins;
using MinecraftClone3API.Util;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace MinecraftClone3
{
    internal class Program
    {
        private static readonly EntityPlayer _playerEntity = new EntityPlayer(){Position = new Vector3(0, 2, 0)};
        private static readonly Camera _camera = new Camera(_playerEntity);

        private static GameWindow _window;
        private static World _world;
        private static Shader shader;
        private static Matrix4 projection;
        private static int _fpsCounter;
        private static double _fpsTimer;

        private static void Main(string[] args)
        {
            _window = new GameWindow(1280, 720, GraphicsMode.Default, "MinecraftClone3")
            {
                CursorVisible = false,
                TargetUpdateFrequency = 120,
                VSync = VSyncMode.On
            };
            _window.Closed += WindowOnClosed;
            _window.Resize += WindowOnResize;
            _window.FocusedChanged += WindowOnFocusedChanged;
            _window.UpdateFrame += WindowOnUpdateFrame;
            _window.RenderFrame += WindowOnRenderFrame;

            //Load plugins in "Plugins" dir
            var pluginsDir = new DirectoryInfo("Plugins");
            foreach (var dir in pluginsDir.EnumerateDirectories())
                PluginManager.AddPlugin(dir);
            foreach (var file in pluginsDir.EnumerateFiles())
                PluginManager.AddPlugin(file);

            PlayerController.SetEntity(_playerEntity);
            PluginManager.LoadPlugins();
            BlockTextureManager.Upload();

            _world = new World();
            _world.PlayerEntities.Add(_playerEntity);
            shader = new Shader("testShader");

            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60), (float)_window.Width/_window.Height, 0.01f, 256);

            _window.Run();
        }

        private static void WindowOnClosed(object sender, EventArgs eventArgs)
        {
            _world.Unload();
        }

        private static void WindowOnResize(object sender, EventArgs eventArgs)
        {
            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60), (float)_window.Width / _window.Height, 0.01f, 256);
            GL.Viewport(_window.ClientSize);
        }

        private static void WindowOnFocusedChanged(object sender, EventArgs eventArgs)
        {
            if (_window.Focused)
                PlayerController.ResetMouse();
        }

        private static void WindowOnUpdateFrame(object sender, FrameEventArgs frameEventArgs)
        {
            PlayerController.Update(_window, _world);

            _world.Update();
            _camera.Update();
        }

        private static void WindowOnRenderFrame(object sender, FrameEventArgs frameEventArgs)
        {
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);

            GL.ClearColor(Color4.DarkBlue);
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit | ClearBufferMask.ColorBufferBit);

            shader.Bind();
            GL.UniformMatrix4(4, false, ref _camera.View);
            GL.UniformMatrix4(8, false, ref projection);

            WorldRenderer.RenderWorld(_world);

            _window.SwapBuffers();

            _fpsCounter++;
            _fpsTimer += frameEventArgs.Time;
            if (_fpsTimer >= 1)
            {
                Logger.Debug("FPS: " + _fpsCounter / 1f);
                _fpsTimer -= 1;
                _fpsCounter = 0;

                Logger.Debug($"ChunksQueued: {_world.ChunksQueuedCount}, ChunksReady: {_world.ChunksReadyCount}, ChunksLoaded: {_world.ChunksLoadedCount}");
            }
        }
    }
}
