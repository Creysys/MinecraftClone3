using System;
using System.Collections.Generic;
using MinecraftClone3API.IO;

namespace MinecraftClone3API.Util
{
    public static class I18N
    {
        private static readonly Dictionary<string, string> Entries = new Dictionary<string, string>();

        private static string _currentLang = "en-US";

        public static void SetCurrentLanguage(string lang) => _currentLang = lang;

        public static void Load(Action<float> progress)
        {
            Entries.Clear();

            var indices = new Dictionary<string, int>();
            var part = 1f / ResourceManager.LangEntries.Count;
            var total = 0f;
            ResourceManager.LangEntries.ForEach(entry =>
            {
                progress(total);
                total += part;
                
                var splits = entry.Line.Split('=');
                if (splits.Length != 2) return;
                var key = splits[0];
                var value = splits[1];

                var globalKey = MakeKey(entry.Lang, key);

                if (indices.TryGetValue(globalKey, out var index))
                    if (entry.Index < index) return;

                indices[globalKey] = entry.Index;
                Entries[globalKey] = value;
            });
        }

        public static string GetLang(string lang, string key) => Entries.TryGetValue(MakeKey(lang, key), out var value)
            ? value
            : key;
        public static string Get(string key) => GetLang(_currentLang, key);

        private static string MakeKey(string lang, string key) => $"{lang}:{key}";
    }
}
