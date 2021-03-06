﻿using System.CodeDom;
using MinecraftClone3API.Blocks;
using MinecraftClone3API.Util;

namespace MinecraftClone3API.Plugins
{
    public class PluginContext
    {
        public readonly PluginAttribute PluginAttribute;

        internal readonly IPlugin Plugin;

        internal PluginContext(PluginAttribute pluginAttribute, IPlugin plugin)
        {
            PluginAttribute = pluginAttribute;
            Plugin = plugin;
        }

        public void Register(Block block) => GameRegistry.BlockRegistry.Register(PluginAttribute.Id, block);
        public void Register<T>() where T : BlockData => GameRegistry.BlockDataRegistry.Register(PluginAttribute.Id, new BlockDataRegistryEntry(typeof(T)));
    }
}
