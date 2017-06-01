using System.IO;
using System.Threading;
using MinecraftClone3API.Client;
using MinecraftClone3API.Client.Graphics;
using MinecraftClone3API.Client.GUI;
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
        private static Texture _background;
        private static Texture _progressBar;
        private static Texture _progressBarFull;

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

            _background = ResourceReader.ReadTexture("System/Textures/Gui/ResourceLoadingBackground.png");
            _progressBar = ResourceReader.ReadTexture("System/Textures/Gui/Progressbar.png");
            _progressBarFull = ResourceReader.ReadTexture("System/Textures/Gui/ProgressbarFull.png");

            Start(false);
        }

        public GuiResourceLoading()
        {
            Start(true);
        }

        public override void Update()
        {
            if (_thread.IsAlive) return;
            //IsDead = true;
            //StateEngine.AddState(new StateMainMenu());
        }

        public override void Render()
        {
            GuiRenderer.DrawTexture(_background, new Rectangle(0, 0, 960, 540), null);
            GuiRenderer.DrawTexture(_progressBar, new Rectangle(100, 340, (int)ScaledResolution.GuiResolution.X - 100, 420), null);
            GuiRenderer.DrawTexture(_progressBarFull, Rectangle.FromSize(100, 340, 800 / 100 * _progress, 80), null);

            //GuiRenderer.DrawTexture(background, new Vector4(-1,-1,1,1), new Vector4(0,0,1,1));
        }

        private void Start(bool reload)
        {
            ClientResources.Window.Context.MakeCurrent(null);
            var contextReady = new EventWaitHandle(false, EventResetMode.AutoReset);
            _thread = new Thread(() =>
                {
                    var window = new NativeWindow();
                    var context = new GraphicsContext(GraphicsMode.Default, window.WindowInfo);
                    context.MakeCurrent(window.WindowInfo);
                    contextReady.Set();

                    Work(reload);

                    context.MakeCurrent(null);
                    context.Dispose();
                    window.Dispose();
                })
                {IsBackground = true};
            _thread.Start();

            contextReady.WaitOne();
            ClientResources.Window.MakeCurrent();
        }

        private void Work(bool reload)
        {
            if (!reload)
            {
                //Add plugins in "Plugins" dir
                var pluginsDir = new DirectoryInfo("Plugins");
                foreach (var dir in pluginsDir.EnumerateDirectories())
                    PluginManager.AddPlugin(new FileSystemRaw(dir));
                foreach (var file in pluginsDir.EnumerateFiles())
                    PluginManager.AddPlugin(new FileSystemCompressed(file));
            }

            //Load resources
            PluginManager.LoadResources(
                (total, state, plugin) =>
                {
                    _progress = (int) (total * 50);
                    _text = $"{I18N.Get(state)} {plugin} ({_progress}%)";
                    Logger.Debug($"{I18N.GetOrdinal(state)} {plugin} ({_progress}%)");
                });

            _progress = 50;
            _text = $"{I18N.Get("system.loading.resources.uploadTextures")} ({_progress}%)";
            Logger.Debug($"{I18N.GetOrdinal("system.loading.resources.uploadTextures")} ({_progress}%)");
            BlockTextureManager.Upload();

            if (!reload)
            {
                //Load plugins
                PluginManager.LoadPlugins(
                    (total, state, plugin) =>
                    {
                        _progress = (int) (total * 50) + 50;
                        _text = $"{I18N.Get(state)} {plugin} ({_progress}%)";
                        Logger.Debug($"{I18N.GetOrdinal(state)} {plugin} ({_progress}%)");
                    });
            }
        }
    }
}