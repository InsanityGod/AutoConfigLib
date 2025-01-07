using AutoConfigLib.AutoConfig;
using AutoConfigLib.HarmonyPatches;
using HarmonyLib;
using Vintagestory.API.Common;

namespace AutoConfigLib
{
    public class AutoConfigLibModSystem : ModSystem
    {
        private Harmony harmony;

        public override void StartPre(ICoreAPI api)
        {
            ConfigGenerator.Configs = new();
            if (!Harmony.HasAnyPatches(Mod.Info.ModID))
            {
                harmony = new Harmony(Mod.Info.ModID);

                harmony.PatchAllUncategorized();
                PatchConfigLoadingCode.PatchConfigStuff(api, harmony);
            }
            base.StartPre(api);
        }

        public override void AssetsFinalize(ICoreAPI api)
        {
            base.AssetsFinalize(api);
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