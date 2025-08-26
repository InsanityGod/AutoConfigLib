using AutoConfigLib.Auto;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Vintagestory.API.Common;

namespace AutoConfigLib.HarmonyPatches;

public static class ConfigInterception
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = instructions.ToList();

        var targetMethod = typeof(ICoreAPICommon).GetMethods().Single(method => method.Name == nameof(ICoreAPICommon.LoadModConfig) && method.IsGenericMethodDefinition);

        for (var i = 0; i < codes.Count; i++)
        {
            var code = codes[i];
            if (code.opcode == OpCodes.Callvirt && code.operand is MethodInfo methodInfo && methodInfo.IsGenericMethod)
            {
                var genericMethod = methodInfo.GetGenericMethodDefinition();
                if (genericMethod != targetMethod) continue;

                var method = AccessTools.Method(typeof(ConfigInterception), nameof(GetConfigOverride))
                    .MakeGenericMethod(methodInfo.GetGenericArguments());
                code.opcode = OpCodes.Call;
                code.operand = method;
            }
        }
        return codes;
    }

    public static T GetConfigOverride<T>(ICoreAPICommon api, string fileName)
    {
        var result = api.LoadModConfig<T>(fileName);
        if (api is not ICoreAPI coreApi)
        {
            //shouldn't happen but just in case
            Console.WriteLine("AutoConfigLib: Failed to register config because LoadModConfig was not called on 'ICoreApi' but a different implementation of 'ICoreCommonApi'");
            return result;
        }

        AutoConfigLibModSystem.EnsureApiCache(coreApi);
        return AutoConfigGenerator.RegisterOrCollectConfigFile(coreApi, fileName, result);
    }
}