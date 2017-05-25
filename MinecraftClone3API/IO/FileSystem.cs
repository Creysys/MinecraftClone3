using System.Collections.Generic;

namespace MinecraftClone3API.IO
{
    internal abstract class FileSystem
    {
        public readonly string Name;

        protected FileSystem(string name)
        {
            Name = name;
        }

        public abstract List<string> GetFiles();
        public abstract byte[] ReadFile(string path);
    }
}
