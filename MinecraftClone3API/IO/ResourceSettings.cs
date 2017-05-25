using System.Collections.Generic;

namespace MinecraftClone3API.IO
{
    internal class ResourceSettings
    {
        public class Entry
        {
            public string Name;
            public bool Enabled = true;
        }

        public List<Entry> Entries = new List<Entry>();

        public int IndexOf(string name) => Entries.FindIndex(s => s.Name.Equals(name, System.StringComparison.Ordinal));
        public int Add(string name)
        {
            Entries.Add(new Entry() { Name = name });
            return Entries.Count - 1;
        }
        public bool IsEnabled(int index) => Entries[index].Enabled;
    }
}
