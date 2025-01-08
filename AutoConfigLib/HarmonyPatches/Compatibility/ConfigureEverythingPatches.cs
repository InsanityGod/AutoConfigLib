using AutoConfigLib.AutoConfig;
using ConfigureEverything;
using ConfigureEverything.Configuration;
using ConfigureEverything.HarmonyPatches;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace AutoConfigLib.HarmonyPatches.Compatibility
{
    [HarmonyPatchCategory("configureeverything")]
    [HarmonyPatch]
    public static class ConfigureEverythingPatches
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            //var api = (ICoreAPI)AutoConfigLibModSystem.CoreServerAPI ?? AutoConfigLibModSystem.CoreClientAPI;
            //var coreSystem = api.ModLoader.GetMod("configureeverything").Systems.FirstOrDefault(system => system.GetType().Name == "Core");
            //var harmonySystem = api.ModLoader.GetMod("configureeverything").Systems.FirstOrDefault(system => system.GetType().Name == "HarmonyPatches");
            //
            //if(coreSystem == null || harmonySystem == null)
            //{
            //    api.Logger.Error("AutoConfigLib <-> ConfigureEverything compatibility failed");
            //}
            //else
            //{
            //    yield return coreSystem.GetType().GetMethod("AssetsFinalize");
            //    yield return coreSystem.GetType().GetMethod("StartPre");
            //    yield return AccessTools.Method(harmonySystem.GetType(), "PatchAll");
            //}
            yield return AccessTools.Method(typeof(Core), nameof(Core.StartPre));
            yield return AccessTools.Method(typeof(Core), nameof(Core.AssetsFinalize));
            yield return AccessTools.Method(typeof(ConfigureEverything.HarmonyPatches.HarmonyPatches), "PatchAll");
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var targetMethod = AccessTools.Method(typeof(ModConfig), nameof(ModConfig.ReadConfig));
            var replacementMethod = AccessTools.Method(typeof(ConfigureEverythingPatches), nameof(ReadConfigIntercept));

            for(var i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                if(code.opcode == OpCodes.Call && code.operand is MethodInfo method && method.IsGenericMethod && method.GetGenericMethodDefinition() == targetMethod)
                {
                    code.operand = replacementMethod.MakeGenericMethod(method.GetGenericArguments());
                }
            }

            return codes;
        }

        public static T ReadConfigIntercept<T>(ICoreAPI api, string jsonConfig)
        {
            var targetMethod = AccessTools.Method(typeof(ModConfig), nameof(ModConfig.ReadConfig));
            var result = (T)targetMethod.MakeGenericMethod(typeof(T))
                .Invoke(null, new object[] { api, jsonConfig });

            //TODO extract this duplicate code in to a seperate method
            if (!ConfigGenerator.Configs.ContainsKey(jsonConfig))
            {
                var config = new AutoConfig.Config
                {
                    Filename = jsonConfig,
                    Type = typeof(T),
                };

                if(api.Side == EnumAppSide.Client) config.ClientValue = result;
                if(api.Side == EnumAppSide.Server) config.ServerValue = result;

                ConfigGenerator.Configs[jsonConfig] = config;
            }
            else
            {
                var config = ConfigGenerator.Configs[jsonConfig];

                if(AutoConfigLibModSystem.Config.ClientServerConfigAutoMerge) return (T)(config.ServerValue ?? config.ClientValue);

                if(api.Side == EnumAppSide.Client) config.ClientValue ??= result;
                if(api.Side == EnumAppSide.Server) config.ServerValue ??= result;
            }

            return result;
        }
    }
}
