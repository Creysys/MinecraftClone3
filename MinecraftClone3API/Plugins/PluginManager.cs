using MinecraftClone3API.IO;
using MinecraftClone3API.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MinecraftClone3API.Plugins
{
    public static class PluginManager
    {
        private const string PluginInfoFile = "PluginInfo.json";

        private static readonly Dictionary<string, PluginContext> PluginDlls = new Dictionary<string, PluginContext>();

        internal static void LoadResources(Action<float, string> progress)
        {
            var part = 1f / PluginDlls.Count;
            var total = 0f;
            foreach (var plugin in PluginDlls)
            {
                progress(total, plugin.Value.PluginAttribute.Name);
                total += part;
                plugin.Value.Plugin.LoadResources(plugin.Value);
            }
        }

        public static void LoadPlugins(Action<float, string, string> progress)
        {
            var part = 0.3333333F / PluginDlls.Count;
            var total = 0f;
            foreach (var plugin in PluginDlls)
            {
                progress(total, "system.loading.preLoad", plugin.Value.PluginAttribute.Name);
                total += part;
                plugin.Value.Plugin.PreLoad(plugin.Value);
            }

            foreach (var plugin in PluginDlls)
            {
                progress(total, "system.loading.load", plugin.Value.PluginAttribute.Name);
                total += part;
                plugin.Value.Plugin.Load(plugin.Value);
            }

            foreach (var plugin in PluginDlls)
            {
                progress(total, "system.loading.postLoad", plugin.Value.PluginAttribute.Name);
                total += part;
                plugin.Value.Plugin.PostLoad(plugin.Value);
            }
        }

        public static void AddPlugin(FileSystem fileSystem)
        {
            var pluginFiles = fileSystem.GetFiles();
            ResourceManager.AddFileSystem(fileSystem, pluginFiles);
            if (!pluginFiles.Contains(PluginInfoFile))
            {
                Logger.Error($"Plugin \"{fileSystem.Name}\" does not have an info file and was ignored!");
                return;
            }

            var pluginInfoFileData = fileSystem.ReadFile(PluginInfoFile);
            var pluginInfo = JsonConvert.DeserializeObject<PluginInfo>(Encoding.UTF8.GetString(pluginInfoFileData));

            if (pluginInfo.PluginDlls == null) return;
            foreach (var dllPath in pluginInfo.PluginDlls)
            {
                if (!pluginFiles.Contains(dllPath))
                {
                    Logger.Error($"Plugin dll \"{dllPath}\" from \"{pluginInfo.PluginName}\" not found!");
                    continue;
                }

                try
                {
                    var dllData = fileSystem.ReadFile(dllPath);
                    var assembly = Assembly.Load(dllData);
                    foreach (var type in assembly.GetTypes().Where(t => typeof(IPlugin).IsAssignableFrom(t)))
                    {
                        var attributes = type.GetCustomAttributes(typeof(PluginAttribute), false);
                        if (attributes.Length != 1) continue;

                        var plugin = (IPlugin) Activator.CreateInstance(type);
                        var attribute = (PluginAttribute) attributes[0];
                        var pluginContext = new PluginContext(attribute, plugin);
                        PluginDlls.Add(attribute.Id, pluginContext);

                        Logger.Info($"Plugin dll \"{attribute.Id}\" added");
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Logger.Error($"There is a problem with plugin dll \"{dllPath}\" in \"{pluginInfo.PluginName}\":");
                    Logger.Exception(ex.LoaderExceptions[0]);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error loading plugin dll:\"{dllPath}\" in \"{pluginInfo.PluginName}\"");
                    Logger.Exception(ex);
                }
            }
        }
    }
}