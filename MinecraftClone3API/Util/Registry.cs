using System.Collections.Generic;

namespace MinecraftClone3API.Util
{
    public class Registry<T> where T : RegistryEntry
    {
        private readonly Dictionary<string, T> _keysToEntries = new Dictionary<string, T>();

        public T this[string key] => _keysToEntries[key];

        public virtual void Register(string prefix, T entry)
        {
            entry.RegistryKey = $"{prefix}:{entry.Name}";
            _keysToEntries.Add(entry.RegistryKey, entry);
        }
    }
}