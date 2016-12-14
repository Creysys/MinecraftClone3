﻿using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using MinecraftClone3API.Graphics;
using MinecraftClone3API.Plugins;
using MinecraftClone3API.Util;

namespace MinecraftClone3API.IO
{
    public static class ResourceReader
    {
        public static byte[] ReadBytes(string path)
        {
            foreach (var pluginData in PluginManager.PluginDatas)
                if (pluginData.Files.TryGetValue(path, out byte[] data))
                    return data;

            Logger.Error($"\"{path}\" could not be found!");
            return null;
        }

        public static bool Exists(string path)
            => PluginManager.PluginDatas.Any(pluginData => pluginData.Files.ContainsKey(path));

        public static string ReadString(string path) => Encoding.Default.GetString(ReadBytes(path));

        public static TextureData ReadTextureData(string path)
            => new TextureData((Bitmap) Image.FromStream(new MemoryStream(ReadBytes(path))));

        public static BlockTexture ReadBlockTexture(string path)
            => BlockTextureManager.LoadTexture(ReadTextureData(path));

        public static Shader ReadShader(string path) => new Shader(path);
    }
}
