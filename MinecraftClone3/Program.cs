﻿using System;
using System.Globalization;
using System.IO;
using MinecraftClone3API.Blocks;
using MinecraftClone3API.Entities;
using MinecraftClone3API.Graphics;
using MinecraftClone3API.IO;
using MinecraftClone3API.Plugins;
using MinecraftClone3API.Util;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace MinecraftClone3
{
    internal class Program
    {

        private static readonly EntityPlayer _playerEntity = new EntityPlayer {Position = new Vector3(0, 2, 0)};

        public static GameWindow Window;

        private static WorldServer _world;
        private static Matrix4 _projection;
        private static int _fpsCounter;
        private static double _fpsTimer;

        private static void Main(string[] args)
        {
            //Make exceptions be english (wtf microsoft???)
            System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

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

            CommonResources.Load();

            //Load plugins in "Plugins" dir
            var pluginsDir = new DirectoryInfo("Plugins");
            foreach (var dir in pluginsDir.EnumerateDirectories())
                PluginManager.AddPlugin(new FileSystemRaw(dir));
            foreach (var file in pluginsDir.EnumerateFiles())
                PluginManager.AddPlugin(new FileSystemCompressed(file));
            ResourceManager.Load(
                (total, state, plugin) => Logger.Debug($"{I18N.Get(state)} {plugin} ({(int) (total * 100)})"));

            PluginManager.LoadPlugins(
                (total, state, plugin) => Logger.Debug($"{I18N.Get(state)} {plugin} ({(int) (total * 100)})"));

            ClientResources.Load(Window);

            PlayerController.SetEntity(_playerEntity);
            PluginManager.LoadPlugins((total, state, plugin) => { });

            BoundingBoxRenderer.Load();

            BlockTextureManager.Upload();

            _world = new WorldServer();
            _world.PlayerEntities.Add(_playerEntity);

            _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60), (float)Window.Width/Window.Height, 0.01f, 512);

            Window.Run();
        }

        private static void WindowOnClosed(object sender, EventArgs eventArgs)
        {
            _world.Unload();
        }

        private static void WindowOnResize(object sender, EventArgs eventArgs)
        {
            _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60), (float)Window.Width / Window.Height, 0.01f, 512);
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
        }

        private static void WindowOnRenderFrame(object sender, FrameEventArgs frameEventArgs)
        {
            WorldRenderer.RenderWorld(_world, _projection);

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
