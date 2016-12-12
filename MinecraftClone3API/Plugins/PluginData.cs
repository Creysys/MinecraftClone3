using System.Collections.Generic;
using System.Linq;

namespace MinecraftClone3API.Plugins
{
    internal class PluginData
    {
        public const string AssetsDir = "Assets";

        public readonly PluginInfo PluginInfo;

        public readonly Dictionary<string, byte[]> Files = new Dictionary<string, byte[]>();

        public PluginData(PluginInfo pluginInfo, Dictionary<string, byte[]> files)
        {
            PluginInfo = pluginInfo;

            foreach (var entry in files.Where(entry => entry.Key.StartsWith(AssetsDir + "/")))
                Files.Add(entry.Key.Substring(AssetsDir.Length + 1), entry.Value);
        }
    }
}
