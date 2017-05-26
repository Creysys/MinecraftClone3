using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace MinecraftClone3API.IO
{
    public class FileSystemCompressed : FileSystem
    {
        private readonly ZipArchive _archive;

        public FileSystemCompressed(FileInfo file) : base(file.Name)
        {
            _archive = new ZipArchive(file.OpenRead());
        }

        public override List<string> GetFiles() => _archive.Entries.Select(e => e.FullName).ToList();
        

        public override byte[] ReadFile(string path)
        {
            var entry = _archive.GetEntry(path);
            if (entry == null)
                throw new FileNotFoundException("File could not be found in the compressed file system!", path);
            return ReadEntry(entry);
        }


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
