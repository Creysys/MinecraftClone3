using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace MinecraftClone3API.IO
{
    internal class FileSystemCompressed : FileSystem
    {
        private readonly ZipArchive _archive;

        public FileSystemCompressed(FileInfo file) : base(file.Name)
        {
            _archive = new ZipArchive(file.OpenRead());
        }

        public override Dictionary<string, byte[]> ReadAllFiles() => _archive.Entries.ToDictionary(entry => entry.FullName, ReadEntry);

        private byte[] ReadEntry(ZipArchiveEntry entry)
        {
            using (var memoryStream = new MemoryStream())
            {
                entry.Open().CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
