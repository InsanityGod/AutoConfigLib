using Newtonsoft.Json.Linq;
using System;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace AutoConfigLib.AutoConfig
{
    public class Config
    {
        public string ConfigDisplayName => $"{Mod?.Info.Name ?? Filename} (AutoConfig)";

        public Type Type { get; internal set; }

        public string Filename { get; internal set; }

        public object Value { get; internal set; }

        public Mod Mod { get; internal set; }

        public void Save(ICoreAPI api) => api.StoreModConfig(new JsonObject(JToken.FromObject(Value)), Filename);
    }
}