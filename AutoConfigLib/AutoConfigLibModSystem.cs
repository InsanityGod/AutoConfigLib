﻿using AutoConfigLib.Auto;
using AutoConfigLib.Auto.Rendering;
using AutoConfigLib.Auto.Rendering.Attributes;
using AutoConfigLib.Config;
using AutoConfigLib.HarmonyPatches;
using ConfigLib;
using HarmonyLib;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace AutoConfigLib
{
    public class AutoConfigLibModSystem : ModSystem
    {
        private static Harmony harmony = new("autoconfiglib");

        public const string ConfigName = "AutoConfigLibConfig.json";

        public override double ExecuteOrder() => double.MinValue;

        public AutoConfigLibModSystem()
        {
            AutoConfigGenerator.FoundConfigsByPath ??= new();
            Renderer.CachedRenderesByType ??= new();
            AttributeHelper.RegisterDefaultCustomProviders();
            if (!Harmony.HasAnyPatches("autoconfiglib"))
            {
                harmony.PatchAllUncategorized();

                PatchConfigLoadingCode.FindAndPatchMethods(harmony);
            }
        }

        public static ModConfig Config { get; private set; }

        public static ICoreClientAPI CoreClientAPI { get; set; }
        public static ICoreServerAPI CoreServerAPI { get; set; }

        public static void EnsureApiCache(ICoreAPI api)
        {
            if (api.Side == EnumAppSide.Client) CoreClientAPI ??= api as ICoreClientAPI;
            else CoreServerAPI ??= api as ICoreServerAPI;
        }

        public override void StartPre(ICoreAPI api)
        {
            base.StartPre(api);
            EnsureApiCache(api);

            if (api.ModLoader.IsModEnabled("configureeverything")) harmony?.PatchCategory("configureeverything");

            if (Config == null)
            {
                try
                {
                    Config = api.LoadModConfig<ModConfig>(ConfigName) ?? new();
                    api.StoreModConfig(Config, ConfigName); //Save just in case new fields where initialized
                    Config = AutoConfigGenerator.RegisterOrCollectConfigFile(api, ConfigName, Config);
                }
                catch (Exception ex)
                {
                    api.Logger.Error($"Failed to load {ConfigName} using default, exception: {ex}");
                    Config = new();
                }
            }
            if(api.Side == EnumAppSide.Client && Config.ConfigLibWindowImprovements) harmony?.PatchCategory("ConfigLibWindowImprovements");

            //TODO: maybe allow for localizing config into world settings
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            var configLib = api.ModLoader.GetModSystem<ConfigLibModSystem>();
            if (configLib == null) return;
            if (Config.RegisterWorldConfig) configLib.RegisterCustomConfig("World Config", WorldConfigEditor.EditWorldConfig);
            configLib.ConfigWindowClosed += Renderer.ClearCache;
        }

        /// <summary>
        /// Register a config manually
        /// </summary>
        /// <returns></returns>
        public static T RegisterOrCollectConfigFile<T>(ICoreAPI api, string configPath, T configValue) => AutoConfigGenerator.RegisterOrCollectConfigFile(api, configPath, configValue);

        /// <summary>
        /// Adds an object to configlib for editing (using auto generated UI)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        public static void AddForEditing<T>(T obj, string name) where T : class =>
            CoreClientAPI.ModLoader.GetModSystem<ConfigLibModSystem>().RegisterCustomConfig(name, (string id, ControlButtons buttons) => Renderer.GetOrCreateRenderForType(typeof(T)).RenderObject(obj, id));

        public override void AssetsFinalize(ICoreAPI api)
        {
            base.AssetsFinalize(api);
            if (api.Side == EnumAppSide.Server) return;

            AutoConfigGenerator.RegisterFoundConfigsInConfigLib(api);
        }

        public override void Dispose()
        {
            harmony?.UnpatchAll();
            AutoConfigGenerator.FoundConfigsByPath = null;
            Renderer.CachedRenderesByType = null;
            AttributeHelper.CustomProviders.Clear();
            Config = null;

            DoNotTouchThis.Touched_1 = false;
            base.Dispose();
        }
    }
}