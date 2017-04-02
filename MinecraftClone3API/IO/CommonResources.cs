using Newtonsoft.Json;

namespace MinecraftClone3API.IO
{
    public static class CommonResources
    {
        public static void Load()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings {Converters = {new CustomJsonConverter()}};
        }
    }
}
