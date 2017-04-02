using System;
using System.Collections.Generic;
using System.Linq;
using MinecraftClone3API.Blocks;
using MinecraftClone3API.IO;
using MinecraftClone3API.Util;
using Newtonsoft.Json;
using OpenTK;
// ReSharper disable InconsistentNaming

namespace MinecraftClone3API.Graphics
{
    public class BlockModel
    {
        private const string FileExtension = ".json";

        public static BlockModel Parse(string source, string path)
        {
            var m = JsonConvert.DeserializeObject<BlockModelParent>(source);
            var sources = new List<string>();

            while (!string.IsNullOrEmpty(m.Parent))
            {
                if (sources.Count > 100)
                    throw new Exception($"\"{m.Parent}\" has either more than 100 parents or is an endless loop!");

                //Find parent file relatively
                var paths = new List<string>();
                paths.Add(m.Parent);
                paths.Add(m.Parent + FileExtension);

                var i = path.LastIndexOf("/", StringComparison.Ordinal) + 1;
                paths.Add(path.Substring(0, i) + m.Parent);
                paths.Add(path.Substring(0, i) + m.Parent + FileExtension);

                i = path.IndexOf("/", StringComparison.Ordinal) + 1;
                paths.Add(path.Substring(0, i) + m.Parent);
                paths.Add(path.Substring(0, i) + m.Parent + FileExtension);

                var filename = paths.FirstOrDefault(ResourceReader.Exists);

                if (filename == null)
                {
                    Logger.Error($"{m.Parent} could not be found in {path}!");
                    m.Parent = null;
                    continue;
                }

                //Find first existing source, add it to the sources and find its parent
                var parentSource = ResourceReader.ReadString(filename);
                m = JsonConvert.DeserializeObject<BlockModelParent>(parentSource);
                sources.Add(parentSource);
            }

            sources.Reverse();
            sources.Add(source);
            var model = new BlockModel();
            sources.ForEach(s => JsonConvert.PopulateObject(s, model));
            return model;
        }

        private class BlockModelParent
        {
            public string Parent;
        }

        public class DisplayEntry
        {
            public Vector3 Rotation;
            public Vector3 Translation;
            public Vector3 Scale;
        }

        public class Element
        {
            public Vector3 From;
            public Vector3 To;
            public Dictionary<BlockFace, FaceData> Faces;
        }

        public class FaceData
        {
            public Vector4 UV;
            public string Texture;
            public BlockFace Cullface;
        }
        
        public string Parent;
        public bool AmbientOcclusion;
        public Dictionary<string, DisplayEntry> Display;
        public Dictionary<string, string> Textures;
        public Element[] Elements;

        public BlockModel()
        {
            //Default json constructor
        }
    }
}
