using System;
using MinecraftClone3.Blocks;
using MinecraftClone3.Entities;
using MinecraftClone3.Graphics;
using MinecraftClone3.Utils;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace MinecraftClone3
{
    internal class Program
    {
        private static GameWindow _window;
        private static World _world;
        private static Shader shader;
        private static Matrix4 projection;
        private static TextureArray _textures;
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

            _world = new World();
            shader = new Shader("testShader");

            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60), (float)_window.Width/_window.Height, 0.01f, 256);

            _textures = new TextureArray(16, 16, 2);
            _textures.SetTexture(0, new TextureData("stone.png"));
            _textures.SetTexture(1, new TextureData("dirt.png"));
            _textures.GenerateMipmaps();

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
            _world.Update();

            PlayerController.Update(_window, _world);
        }

        private static void WindowOnRenderFrame(object sender, FrameEventArgs frameEventArgs)
        {
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);

            GL.ClearColor(Color4.DarkBlue);
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit | ClearBufferMask.ColorBufferBit);

            shader.Bind();
            GL.UniformMatrix4(4, false, ref PlayerController.Camera.View);
            GL.UniformMatrix4(8, false, ref projection);

            _textures.Bind(TextureUnit.Texture0);

            WorldRenderer.RenderWorld(_world);

            _window.SwapBuffers();

            _fpsCounter++;
            _fpsTimer += frameEventArgs.Time;
            if (_fpsTimer >= 1)
            {
                Logger.Debug("FPS: " + _fpsCounter / 1f);
                _fpsTimer -= 1;
                _fpsCounter = 0;

                Logger.Debug($"ChunksQueued: {_world.ChunksQueuedCount}, ChunksReady: {_world.ChunksReadyCount}, ChunksLoaded: {_world.ChunksLoadedCount}, ChunkThreads: {_world.ChunkThreadsCount}");
            }
        }
    }
}
