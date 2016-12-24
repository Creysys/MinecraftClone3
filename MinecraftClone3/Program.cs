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

        public static GameWindow Window;

        private static World _world;
        private static Matrix4 projection;
        private static int _fpsCounter;
        private static double _fpsTimer;

        private static void Main(string[] args)
        {
            Window = new GameWindow(1280, 720, GraphicsMode.Default, "MinecraftClone3")
            {
                CursorVisible = false,
                TargetUpdateFrequency = 120,
                VSync = VSyncMode.On
            };
            Window.Closed += WindowOnClosed;
            Window.Resize += WindowOnResize;
            Window.FocusedChanged += WindowOnFocusedChanged;
            Window.UpdateFrame += WindowOnUpdateFrame;
            Window.RenderFrame += WindowOnRenderFrame;

            //Load plugins in "Plugins" dir
            var pluginsDir = new DirectoryInfo("Plugins");
            foreach (var dir in pluginsDir.EnumerateDirectories())
                PluginManager.AddPlugin(dir);
            foreach (var file in pluginsDir.EnumerateFiles())
                PluginManager.AddPlugin(file);

            PlayerController.SetEntity(_playerEntity);
            PluginManager.LoadPlugins();

            ClientResources.Load();

            BlockTextureManager.Upload();

            _world = new World();
            _world.PlayerEntities.Add(_playerEntity);

            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60), (float)Window.Width/Window.Height, 0.01f, 512);

            Window.Run();
        }

        private static void WindowOnClosed(object sender, EventArgs eventArgs)
        {
            _world.Unload();
        }

        private static void WindowOnResize(object sender, EventArgs eventArgs)
        {
            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60), (float)Window.Width / Window.Height, 0.01f, 512);
            GL.Viewport(Window.ClientSize);
        }

        private static void WindowOnFocusedChanged(object sender, EventArgs eventArgs)
        {
            if (Window.Focused)
                PlayerController.ResetMouse();
        }

        private static void WindowOnUpdateFrame(object sender, FrameEventArgs frameEventArgs)
        {
            PlayerController.Update(Window, _world);

            _world.Update();
            _camera.Update();
        }

        private static void WindowOnRenderFrame(object sender, FrameEventArgs frameEventArgs)
        {
            WorldRenderer.RenderWorld(_world, _camera, projection);

            Window.SwapBuffers();

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
