using Newtonsoft.Json;

#pragma warning disable 0649

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

#pragma warning restore 0649