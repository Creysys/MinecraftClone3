namespace MinecraftClone3API.Util
{
    public abstract class RegistryEntry
    {
        public readonly string Name;

        protected RegistryEntry(string name)
        {
            Name = name;
        }

        public string RegistryKey { get; internal set; }
    }
}