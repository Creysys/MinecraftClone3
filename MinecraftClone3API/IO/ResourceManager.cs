﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MinecraftClone3API.Plugins;
using MinecraftClone3API.Util;

namespace MinecraftClone3API.IO
{
    public static class ResourceManager
    {
        internal struct LangLine
        {
            public string Lang;
            public string Line;
            public int Index;

            public LangLine(string name, string line, int index)
            {
                Lang = name;
                Line = line;
                Index = index;
            }
        }

        private const string ResourceSettingsFile = "ResourceSettings.json";
        private const string AssetsDir = "Assets/";
        private const string LangDir = "Lang/";
        private const string LangExt = ".lang";

        private static readonly Dictionary<string, FileSystem> AssetIndices = new Dictionary<string, FileSystem>();

        internal static readonly List<LangLine> LangEntries = new List<LangLine>();

        internal static void AddFileSystem(FileSystem fileSystem, List<string> pluginFiles)
        {
            if (!File.Exists(ResourceSettingsFile))
                File.WriteAllText(ResourceSettingsFile, JsonConvert.SerializeObject(new ResourceSettings()));
            var resourceSettings =
                JsonConvert.DeserializeObject<ResourceSettings>(File.ReadAllText(ResourceSettingsFile));

            var index = resourceSettings.IndexOf(fileSystem.Name);
            if (index == -1)
            {
                index = resourceSettings.Add(fileSystem.Name);
                File.WriteAllText(ResourceSettingsFile,
                    JsonConvert.SerializeObject(resourceSettings, Formatting.Indented));
            }

            if (!resourceSettings.IsEnabled(index)) return;

            pluginFiles.ForEach(f =>
            {
                //Add asset index
                if (f.StartsWith(AssetsDir, StringComparison.OrdinalIgnoreCase))
                {
                    f = f.Substring(AssetsDir.Length);
                    if (AssetIndices.TryGetValue(f, out var fs))
                    {
                        var otherIndex = resourceSettings.IndexOf(fs.Name);
                        if (otherIndex > index) return;
                    }

                    AssetIndices[f] = fileSystem;
                }
                //Import language
                else if (f.StartsWith(LangDir, StringComparison.OrdinalIgnoreCase) &&
                         f.EndsWith(LangExt, StringComparison.OrdinalIgnoreCase))
                {
                    var name = GetLangName(f);
                    var data = fileSystem.ReadFile(f);
                    using (var reader = new StringReader(Encoding.UTF8.GetString(data)))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                            LangEntries.Add(new LangLine(name, line, index));
                    }
                }
            });
        }

        internal static byte[] LoadAsset(string path)
        {
            if (!AssetIndices.ContainsKey(path))
                throw new FileNotFoundException("File could not be found in Resources!", path);

            return AssetIndices[path].ReadFile(AssetsDir + path);
        }

        internal static bool ExistsAsset(string path) => AssetIndices.ContainsKey(path);

        private static string GetLangName(string path)
        {
            var slashIndex = path.LastIndexOf("/", StringComparison.Ordinal);
            slashIndex = slashIndex == -1 ? 0 : slashIndex + 1;

            return path.Substring(slashIndex, path.Length - slashIndex - LangExt.Length);
        }
    }
}