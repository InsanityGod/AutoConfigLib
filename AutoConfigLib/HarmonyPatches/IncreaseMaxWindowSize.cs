using HarmonyLib;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;

namespace AutoConfigLib.HarmonyPatches;

[HarmonyPatch("ConfigLib.ConfigWindow","Draw")]
[HarmonyPatchCategory("ConfigLibWindowImprovements")]
public static class IncreaseMaxWindowSize
{
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = instructions.ToList();
        var targetMethod = AccessTools.Method(typeof(ImGui), nameof(ImGui.SetNextWindowSizeConstraints), new Type[] { typeof(Vector2), typeof(Vector2) });
        for(var i = 0; i < codes.Count; i++)
        {
            var code = codes[i];
            if(code.opcode == OpCodes.Call && code.operand == targetMethod)
            {
                var sizeX = codes[i -3];

                if(sizeX.opcode == OpCodes.Ldc_R4 && sizeX.operand is float currentSizeX && currentSizeX < 2000f)
                {
                    sizeX.operand = 2000f;
                }

                break;
            }
        }

        return codes;
    }
}
