using MinecraftClone3API.Plugins;

namespace VanillaPlugin
{
    [Plugin("Vanilla", "1.0", "Vanilla")]
    public class VanillaPlugin : IPlugin
    {
        public void PreLoad(PluginContext context)
        {
        }

        public void Load(PluginContext context)
        {
            context.Register(new BlockBasic("Stone", "Vanilla/Textures/Blocks/Stone.png"));
            context.Register(new BlockBasic("Dirt", "Vanilla/Textures/Blocks/Dirt.png"));
            context.Register(new BlockGrass());
        }

        public void PostLoad(PluginContext context)
        {
        }

        public void Unload(PluginContext context)
        {
        }
    }
}
