using System.Collections.Generic;
using System.IO;
using System.Text;
using MinecraftClone3API.IO;
using MinecraftClone3API.Util;
using Newtonsoft.Json;

namespace MinecraftClone3API.Plugins
{
    public static class PluginManager
    {
        private const string PluginInfoFile = "PluginInfo.json";

        private static readonly List<PluginData> PluginDatas = new List<PluginData>();
        private static readonly Dictionary<string, PluginContext> Plugins = new Dictionary<string, PluginContext>();

        public static void AddPlugin(DirectoryInfo dir) => AddPlugin(new FileSystemRaw(dir));
        public static void AddPlugin(FileInfo file) => AddPlugin(new FileSystemCompressed(file));

        private static void AddPlugin(FileSystem fileSystem)
        {
            var files = fileSystem.ReadAllFiles();
            if (!files.TryGetValue(PluginInfoFile, out byte[] pluginInfoFileData))
            {
                Logger.Error($"Plugin \"{fileSystem.Name}\" does not have an info file and was ignored!");
                return;
            }

            var pluginInfo = JsonConvert.DeserializeObject<PluginInfo>(Encoding.Default.GetString(pluginInfoFileData));
        }
    }
}
