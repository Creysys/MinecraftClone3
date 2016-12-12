using System.Collections.Generic;

namespace MinecraftClone3API.IO
{
    internal abstract class FileSystem
    {
        public readonly string Name;

        protected FileSystem(string path)
        {
            Name = path;
        }

        public abstract Dictionary<string, byte[]> ReadAllFiles();
    }
}
