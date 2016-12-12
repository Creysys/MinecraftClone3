namespace MinecraftClone3API.Plugins
{
    public interface IPlugin
    {
        void PreLoad(PluginContext context);
        void Load(PluginContext context);
        void PostLoad(PluginContext context);
        void Unload(PluginContext context);
    }
}
