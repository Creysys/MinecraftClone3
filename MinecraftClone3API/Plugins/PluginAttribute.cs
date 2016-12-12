using System;

namespace MinecraftClone3API.Plugins
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PluginAttribute : Attribute
    {
        public readonly string Id;
        public readonly string Version;
        public readonly string Name;

        public PluginAttribute(string id, string version, string name)
        {
            Id = id;
            Version = version;
            Name = name;
        }
    }
}
