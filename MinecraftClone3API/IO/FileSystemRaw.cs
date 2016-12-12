using System.Collections.Generic;
using System.IO;

namespace MinecraftClone3API.IO
{
    internal class FileSystemRaw : FileSystem
    {
        private readonly DirectoryInfo _dir;

        public FileSystemRaw(DirectoryInfo dir) : base(dir.Name)
        {
            _dir = dir;
        }

        public override Dictionary<string, byte[]> ReadAllFiles()
        {
            var dic = new Dictionary<string, byte[]>();
            AddDir(dic, _dir, _dir);
            return dic;
        }

        private void AddDir(Dictionary<string, byte[]> dic, DirectoryInfo currentDir, DirectoryInfo rootDir)
        {
            foreach (var file in currentDir.EnumerateFiles())
            {
                var relativePath = file.FullName.Substring(rootDir.FullName.Length + 1).Replace("\\", "/");
                dic.Add(relativePath, File.ReadAllBytes(file.FullName));
            }

            foreach (var dir in currentDir.EnumerateDirectories())
                AddDir(dic, dir, rootDir);
        }
    }
}
