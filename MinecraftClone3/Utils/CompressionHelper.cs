using System.IO;
using System.IO.Compression;

namespace MinecraftClone3.Utils
{
    internal static class CompressionHelper
    {
        public static byte[] CompressBytes(byte[] bytes)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var compressionStream = new GZipStream(memoryStream, CompressionMode.Compress))
                {
                    compressionStream.Write(bytes, 0, bytes.Length);
                }

                return memoryStream.ToArray();
            }
        }

        public static byte[] DecompressBytes(byte[] bytes)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var decompressionStream = new GZipStream(new MemoryStream(bytes), CompressionMode.Decompress))
                {
                    decompressionStream.CopyTo(memoryStream);
                }

                return memoryStream.ToArray();
            }
        }
    }
}