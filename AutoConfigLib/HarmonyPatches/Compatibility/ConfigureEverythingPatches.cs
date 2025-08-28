using AutoConfigLib.Auto;
using ConfigureEverything;
using ConfigureEverything.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
            yield return AccessTools.Method(typeof(Core), nameof(Core.StartPre));
            yield return AccessTools.Method(typeof(Core), nameof(Core.AssetsFinalize));
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var targetMethod = AccessTools.Method(typeof(ModConfig), nameof(ModConfig.ReadConfig));
            var replacementMethod = AccessTools.Method(typeof(ConfigureEverythingPatches), nameof(ReadConfigIntercept));

            for (var i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                if (code.opcode == OpCodes.Call && code.operand is MethodInfo method && method.IsGenericMethod && method.GetGenericMethodDefinition() == targetMethod)
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

            return AutoConfigGenerator.RegisterOrCollectConfigFile(api, jsonConfig, result);
        }
    }
}