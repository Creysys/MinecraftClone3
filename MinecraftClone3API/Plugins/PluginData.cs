using System.Collections.Generic;

namespace MinecraftClone3API.Plugins
{
    internal class PluginData
    {
        public readonly string PluginPath;

        public readonly Dictionary<string, byte[]> Files = new Dictionary<string, byte[]>();

        public PluginData(string pluginPath)
        {
            PluginPath = pluginPath;
        }
    }
}
