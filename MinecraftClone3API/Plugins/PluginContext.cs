using MinecraftClone3API.Blocks;
using MinecraftClone3API.Util;

namespace MinecraftClone3API.Plugins
{
    public class PluginContext
    {
        public readonly string PluginName;

        internal readonly IPlugin Plugin;

        internal PluginContext(string pluginName, IPlugin plugin)
        {
            PluginName = pluginName;
            Plugin = plugin;
        }

        public void Register(Block block) => GameRegistry.BlockRegistry.Register(PluginName, block);
    }
}
