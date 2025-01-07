using Newtonsoft.Json.Linq;
using System;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace AutoConfigLib.AutoConfig
{
    public class Config
    {
        public Type Type { get; internal set; }

        public string Filename { get; internal set; }

        public object ClientValue { get; internal set; }

        public object ServerValue { get; internal set; }

        public object PrimaryValue { get; internal set; }

        public Mod Mod { get; internal set; }

        public void Save(ICoreAPI api) => api.StoreModConfig(new JsonObject(JToken.FromObject(PrimaryValue)), Filename);
    }
}