using Newtonsoft.Json;

namespace MinecraftClone3API.Plugins
{
    internal class PluginInfo
    {
        [JsonProperty(nameof(PluginName))]
        public string PluginName;

        [JsonProperty(nameof(PluginDlls))]
        public string[] PluginDlls;
    }
}
