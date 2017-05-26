using System.IO;
using System.Threading;
using MinecraftClone3API.Client;
using MinecraftClone3API.Client.Graphics;
using MinecraftClone3API.Client.GUI;
using MinecraftClone3API.Client.StateSystem;
using MinecraftClone3API.Graphics;
using MinecraftClone3API.IO;
using MinecraftClone3API.Plugins;
using MinecraftClone3API.Util;
using OpenTK;
using OpenTK.Graphics;

namespace MinecraftClone3.States
{
    internal class GuiResourceLoading : GuiBase
    {
        private static Texture background;

        private Thread _thread;
        private int _progress;
        private string _text;

        public GuiResourceLoading(GameWindow window)
        {
            CommonResources.Load();
            PluginManager.AddPlugin(new FileSystemRaw(new DirectoryInfo("Plugins\\System")));
            ResourceReader.ClearCache();
            ClientResources.Load(window);
            BoundingBoxRenderer.Load();

            background = ResourceReader.ReadTexture("System/Textures/Gui/ResourceLoadingBackground.png");

            Start();
        }

        public GuiResourceLoading()
        {
            Start();
        }

        public override void Update()
        {
            if (_thread.IsAlive) return;
            //IsDead = true;
            //StateEngine.AddState(new StateMainMenu());
        }

        public override void Render()
        {
            GuiRenderer.DrawTexture(background, new Rectangle(10, 10, 960-10, 540-10), Rectangle.FromSize(201, 137, 393, 231), true);
            //GuiRenderer.DrawTexture(background, new Vector4(-1,-1,1,1), new Vector4(0,0,1,1));
        }

        private void Start()
        {
            ClientResources.Window.Context.MakeCurrent(null);
            var contextReady = new EventWaitHandle(false, EventResetMode.AutoReset);
            _thread = new Thread(() =>
                {
                    var window = new NativeWindow();
                    var context = new GraphicsContext(GraphicsMode.Default, window.WindowInfo);
                    context.MakeCurrent(window.WindowInfo);
                    contextReady.Set();

                    Work();

                    context.MakeCurrent(null);
                    context.Dispose();
                    window.Dispose();
                })
                {IsBackground = true};
            _thread.Start();

            contextReady.WaitOne();
            ClientResources.Window.MakeCurrent();
        }

        private void Work()
        {
            //Load plugins in "Plugins" dir
            var pluginsDir = new DirectoryInfo("Plugins");
            foreach (var dir in pluginsDir.EnumerateDirectories())
                PluginManager.AddPlugin(new FileSystemRaw(dir));
            foreach (var file in pluginsDir.EnumerateFiles())
                PluginManager.AddPlugin(new FileSystemCompressed(file));

            ResourceManager.Load(
                (total, state, plugin) =>
                {
                    _progress = (int) (total * 50);
                    _text = $"{I18N.Get(state)} {plugin} ({_progress}%)";
                    Logger.Debug($"{I18N.GetOrdinal(state)} {plugin} ({_progress}%)");
                });

            PluginManager.LoadPlugins(
                (total, state, plugin) =>
                {
                    _progress = (int) (total * 50) + 50;
                    _text = $"{I18N.Get(state)} {plugin} ({_progress}%)";
                    Logger.Debug($"{I18N.GetOrdinal(state)} {plugin} ({_progress})");
                });

            BlockTextureManager.Upload();
        }
    }
}