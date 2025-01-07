using AutoConfigLib.AutoConfig;
using AutoConfigLib.Config;
using AutoConfigLib.HarmonyPatches;
using ConfigLib;
using HarmonyLib;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace AutoConfigLib
{
    public class AutoConfigLibModSystem : ModSystem
    {
        private Harmony harmony;

        public const string ConfigName = "AutoConfigLibConfig.json";

        public static ModConfig Config { get; private set; }

        public static ICoreClientAPI CoreClientAPI { get; set; }
        public static ICoreServerAPI CoreServerAPI { get; set; }
         
        public override void StartPre(ICoreAPI api)
        {
            if(api.Side == EnumAppSide.Client)
            {
                CoreClientAPI = api as ICoreClientAPI;
            }
            else
            {
                CoreServerAPI = api as ICoreServerAPI;
            }

            base.StartPre(api);
            if(Config == null)
            {
                try
                {
                    Config = api.LoadModConfig<ModConfig>(ConfigName) ?? new();
                    api.StoreModConfig(Config, ConfigName);
                }
                catch(Exception ex)
                {
                    api.Logger.Error($"Failed to load {ConfigName} using default, exception: {ex}");
                    Config = new();
                }
            }

            ConfigGenerator.Configs ??= new Dictionary<string, AutoConfig.Config>()
            {
                { 
                    ConfigName,
                    new AutoConfig.Config
                    {
                        Filename = ConfigName,
                        ClientValue = Config,
                        ServerValue = Config,
                        Mod = Mod,
                        Type = typeof(ModConfig)
                    }
                }
            };

            if (!Harmony.HasAnyPatches(Mod.Info.ModID))
            {
                harmony = new Harmony(Mod.Info.ModID);

                harmony.PatchAllUncategorized();
                PatchConfigLoadingCode.PatchConfigStuff(api, harmony);
            }
            
            //TODO: Check if there is a way to safeguard order somewhat
            //TODO: allow for localizing config into world settings
            //TODO: allow for edition world settings
        }

        public override void AssetsFinalize(ICoreAPI api)
        {
            base.AssetsFinalize(api);
            if(api.Side == EnumAppSide.Server) return;
            ConfigGenerator.GenerateDefaultConfigLib(api);
        }

        public override void Dispose()
        {
            harmony?.UnpatchAll();
            ConfigGenerator.Configs = null;
            base.Dispose();
        }
    }
}