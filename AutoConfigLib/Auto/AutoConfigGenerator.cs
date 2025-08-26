using ConfigLib;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vintagestory.API.Common;

namespace AutoConfigLib.Auto
{
    public static class AutoConfigGenerator
    {
        public static Dictionary<string, ConfigDefinition> FoundConfigsByPath { get; internal set; }

        public static T RegisterOrCollectConfigFile<T>(ICoreAPI api, string configPath, T configValue)
        {
            if(AutoConfigLibModSystem.Config == null)
            {
                //Safety check, if this is called before AutoConfigLib is started, just return the value (should not be needed since I adjusted execution order but better safe then sorry)
                api.Logger.Error($"AutoConfigLib could not create auto config for `{configPath}` because they requested config before AutoConfigLib was started");
                return configValue;
            }

            var autoConfigLibTree = api.World.Config.GetOrAddTreeAttribute("autoconfiglib");
            var isLocalized = autoConfigLibTree.TryGetAttribute(configPath, out var localizedConfigStr);
            if (isLocalized) configValue = JsonConvert.DeserializeObject<T>((string)localizedConfigStr.GetValue());

            if (FoundConfigsByPath.TryGetValue(configPath, out var config))
            {
            if(config.Type != typeof(T))
            {
                api.Logger.Warning("[AutoConfigLib] A mod attempted to load the same config file but with a different type, config might not work propperly: '{0}' -> {1} != {2}", configPath, config.Type.FullName, typeof(T).FullName);
                return configValue;
            }

                //TODO: see if there is a better way to get matching mod
                config.Mod ??= api.ModLoader.Mods.FirstOrDefault(mod => mod.Systems.FirstOrDefault()?.GetType().Assembly == config.Type.Assembly);
                if (AutoConfigLibModSystem.Config.AutoMergeClientServerConfig) return (T)(config.ServerValue ?? config.ClientValue);

                if (api.Side == EnumAppSide.Client) config.ClientValue ??= configValue;
                if (api.Side == EnumAppSide.Server) config.ServerValue ??= configValue;
            }
            else
            {
                config = new ConfigDefinition
                {
                    ConfigPath = configPath,
                    IsLocalized_Internal = isLocalized,
                    Type = typeof(T),
                    Mod = api.ModLoader.Mods.FirstOrDefault(mod => mod.Systems.FirstOrDefault()?.GetType().Assembly == typeof(T).Assembly)
                };

                if (api.Side == EnumAppSide.Client) config.ClientValue = configValue;
                if (api.Side == EnumAppSide.Server) config.ServerValue = configValue;
                FoundConfigsByPath.Add(configPath, config);
            }
            return configValue;
        }

        internal static void RegisterFoundConfigsInConfigLib(ICoreAPI api)
        {
            if (api.Side == EnumAppSide.Server) return; //No need to register on server

            var configLib = api.ModLoader.GetModSystem<ConfigLibModSystem>();
            foreach (var config in FoundConfigsByPath.Values)
            {
                try
                {
                    var domain = config.Mod?.Info?.ModID;
                    if(!string.IsNullOrEmpty(domain) && configLib.Domains.Contains(domain)) continue; //In this case they have config support through json api
                    RegisterConfigInConfigLib(api, config);
                }
                catch
                {
                    //TODO: centralize logging for debug/display purposes
                    api.Logger.Warning($"Failed to load AutoConfig for '{config.ConfigPath}' ({config.Mod?.Info?.Name ?? "Unknown Mod"})");
                }
            }
        }

        public static void RegisterConfigInConfigLib(ICoreAPI api, ConfigDefinition config)
        {
            var value = config.ClientValue;
            value ??= config.ServerValue;

            config.PrimaryValue ??= value;
            if (config.ServerValue != null && !ReferenceEquals(config.ServerValue, value))
            {
                api.Logger.Warning($"{config.Mod?.Info?.Name}', has requested config '{config.Type}' on both local client and server, skipping auto config ('ClientServerConfigAutoMerge' can be enabled to prevent this)");
                return;
            }

            var pathWithoutExtension = Path.Combine(Path.GetDirectoryName(config.ConfigPath), Path.GetFileNameWithoutExtension(config.ConfigPath));
            api.ModLoader.GetModSystem<ConfigLibModSystem>()
                .RegisterCustomConfig(
                    $"{config.Mod?.Info?.Name ?? "Unknown Mod"} - {pathWithoutExtension} (auto)",
                    (string id, ControlButtons buttons) => config.Edit(api, id, buttons)
                );
        }
    }
}