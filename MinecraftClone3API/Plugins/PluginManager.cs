using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using MinecraftClone3API.IO;
using MinecraftClone3API.Util;
using Newtonsoft.Json;

namespace MinecraftClone3API.Plugins
{
    public static class PluginManager
    {
        private const string PluginInfoFile = "PluginInfo.json";

        private static readonly Dictionary<string, PluginContext> PluginDlls = new Dictionary<string, PluginContext>();
        internal static readonly List<PluginData> PluginDatas = new List<PluginData>();

        public static void AddPlugin(DirectoryInfo dir) => AddPlugin(new FileSystemRaw(dir));
        public static void AddPlugin(FileInfo file) => AddPlugin(new FileSystemCompressed(file));

        public static void LoadPlugins()
        {
            foreach (var plugin in PluginDlls) plugin.Value.Plugin.PreLoad(plugin.Value);
            foreach (var plugin in PluginDlls) plugin.Value.Plugin.Load(plugin.Value);
            foreach (var plugin in PluginDlls) plugin.Value.Plugin.PostLoad(plugin.Value);
        }

        private static void AddPlugin(FileSystem fileSystem)
        {
            var pluginFiles = fileSystem.ReadAllFiles();
            if (!pluginFiles.TryGetValue(PluginInfoFile, out byte[] pluginInfoFileData))
            {
                Logger.Error($"Plugin \"{fileSystem.Name}\" does not have an info file and was ignored!");
                return;
            }

            var pluginInfo = JsonConvert.DeserializeObject<PluginInfo>(Encoding.Default.GetString(pluginInfoFileData));
            PluginDatas.Add(new PluginData(pluginInfo, pluginFiles));

            foreach (var dllPath in pluginInfo.PluginDlls)
            {
                if (!pluginFiles.TryGetValue(dllPath, out byte[] dllData))
                {
                    Logger.Error($"Plugin dll \"{dllPath}\" from \"{pluginInfo.PluginName}\" not found!");
                    continue;
                }

                var assembly = Assembly.Load(dllData);
                foreach (var type in assembly.GetTypes().Where(t => typeof(IPlugin).IsAssignableFrom(t)))
                {
                    var attributes = type.GetCustomAttributes(typeof(PluginAttribute), false);
                    if (attributes.Length != 1) continue;

                    var plugin = (IPlugin) Activator.CreateInstance(type);
                    var attribute = (PluginAttribute) attributes[0];
                    var pluginContext = new PluginContext(attribute, plugin);
                    PluginDlls.Add(attribute.Id, pluginContext);

                    Logger.Info($"Plugin dll \"{attribute.Id}\" loaded");
                }
            }
        }
    }
}
