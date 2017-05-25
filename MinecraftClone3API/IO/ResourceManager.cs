using MinecraftClone3API.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace MinecraftClone3API.IO
{
    internal static class ResourceManager
    {
        private const string ResourceSettingsFile = "ResourceSettings.json";

        private static Dictionary<string, FileSystem> _assetIndices = new Dictionary<string, FileSystem>();

        internal static void AddAssetsIndices(FileSystem fileSystem, List<string> pluginFiles)
        {
            if (!File.Exists(ResourceSettingsFile))
                File.WriteAllText(ResourceSettingsFile, JsonConvert.SerializeObject(new ResourceSettings()));
            var resourceSettings = JsonConvert.DeserializeObject<ResourceSettings>(File.ReadAllText(ResourceSettingsFile));

            var index = resourceSettings.IndexOf(fileSystem.Name);
            if (index == -1)
            {
                index = resourceSettings.Add(fileSystem.Name);
                File.WriteAllText(ResourceSettingsFile, JsonConvert.SerializeObject(resourceSettings, Formatting.Indented));
            }

            if (!resourceSettings.IsEnabled(index)) return;

            pluginFiles.ForEach(f =>
            {
                if (!f.StartsWith("assets/", StringComparison.OrdinalIgnoreCase)) return;
  
                f = f.Substring(7);
                if(_assetIndices.TryGetValue(f, out var fs))
                {
                    var otherIndex = resourceSettings.IndexOf(fs.Name);
                    if (otherIndex > index) return;
                }

                _assetIndices[f] = fileSystem;
            });
        }

        internal static byte[] Load(string path)
        {
            if (!_assetIndices.ContainsKey(path))
                throw new FileNotFoundException("File could not be found in Resources!", path);

            return _assetIndices[path].ReadFile(path);
        }
    }
}
