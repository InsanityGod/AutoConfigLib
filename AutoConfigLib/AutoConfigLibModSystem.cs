using AutoConfigLib.Auto;
using AutoConfigLib.Config;
using AutoConfigLib.HarmonyPatches;
using ConfigLib;
using HarmonyLib;
using InsanityLib.Attributes.Auto.Config;
using InsanityLib.Util.AutoRegistry;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace AutoConfigLib;

public class AutoConfigLibModSystem : ModSystem
{
    public const string harmonyId = "autoconfiglib";
    private static Harmony Harmony { get; set; }

    public override double ExecuteOrder() => double.MinValue;

    public AutoConfigLibModSystem()
    {
        AutoConfigGenerator.FoundConfigsByPath ??= new();
        if(Harmony is null)
        {
            Harmony = new(harmonyId);
            Harmony.PatchAllUncategorized();

            PatchConfigLoadingCode.FindAndPatchMethods(Harmony);
        }
    }

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

        AutoConfigUtil.EnsureInsanityLibConfigPreLoaded(api);
        AutoConfigUtil.LoadMember(api, AccessTools.Property(typeof(ModConfig), nameof(ModConfig.Instance)), new AutoConfigAttribute("AutoConfigLibConfig.json") { ServerSync = false });

        if (api.ModLoader.IsModEnabled("configureeverything")) Harmony?.PatchCategory("configureeverything");


        if(api.Side == EnumAppSide.Client && ModConfig.Instance.ConfigLibWindowImprovements) Harmony?.PatchCategory("ConfigLibWindowImprovements");

        //TODO: maybe allow for localizing config into world settings
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        var configLib = api.ModLoader.GetModSystem<ConfigLibModSystem>();
        if (configLib == null) return;
        if (ModConfig.Instance.RegisterWorldConfig) configLib.RegisterCustomConfig("World Config", WorldConfigEditor.EditWorldConfig);
    }

    /// <summary>
    /// Register a config manually
    /// </summary>
    /// <returns></returns>
    public static T RegisterOrCollectConfigFile<T>(ICoreAPI api, string configPath, T configValue) => AutoConfigGenerator.RegisterOrCollectConfigFile(api, configPath, configValue);


    public override void AssetsFinalize(ICoreAPI api)
    {
        base.AssetsFinalize(api);
        if (api.Side == EnumAppSide.Server) return;

        AutoConfigGenerator.RegisterFoundConfigsInConfigLib(api);
    }

    public override void Dispose()
    {
        Harmony?.UnpatchAll(harmonyId);
        Harmony = null;

        AutoConfigGenerator.FoundConfigsByPath = null;

        base.Dispose();
    }
}