using System.Collections.Generic;
using System.Drawing;
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
        private static Dictionary<string, BlockTexture> _cachedTextures = new Dictionary<string, BlockTexture>();
        private static Dictionary<string, BlockModel> _cachedModels = new Dictionary<string, BlockModel>();

        internal static void ClearCache()
        {
            _cachedModels = null;
            _cachedTextures = null;
        }

        public static byte[] ReadBytes(string path)
        {/*
            foreach (var pluginData in PluginManager.PluginDatas)
                if (pluginData.Files.TryGetValue(path, out var data))
                    return data;

            Logger.Error($"\"{path}\" could not be found!");*/
            return null;
        }

        public static bool Exists(string path)
            => false;// PluginManager.PluginDatas.Any(pluginData => pluginData.Files.ContainsKey(path));

        public static string ReadString(string path) => Encoding.Default.GetString(ReadBytes(path));

        public static TextureData ReadTextureData(string path)
            => new TextureData((Bitmap) Image.FromStream(new MemoryStream(ReadBytes(path))));

        public static BlockTexture ReadBlockTexture(string path)
        {
            if(_cachedTextures.TryGetValue(path, out var tex)) return tex;
            tex = BlockTextureManager.LoadTexture(ReadTextureData(path));
            _cachedTextures.Add(path, tex);
            return tex;
        }

        public static Shader ReadShader(string path) => new Shader(path);

        public static BlockModel ReadBlockModel(string path)
        {
            if (!Exists(path))
            {
                Logger.Error($"Block model \"{path}\" could not be found!");
                return ClientResources.MissingModel;
            }

            if (_cachedModels.TryGetValue(path, out var model)) return model;
            model = BlockModel.Parse(ReadString(path), path);
            _cachedModels.Add(path, model);
            return model;
        }
    }
}
