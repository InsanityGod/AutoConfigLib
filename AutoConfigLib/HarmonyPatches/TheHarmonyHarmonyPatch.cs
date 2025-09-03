using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AutoConfigLib.HarmonyPatches
{

    [HarmonyPatch]
    public static class TheHarmonyHarmonyPatch
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(Harmony), nameof(Harmony.PatchAll), new Type[] { });
            yield return AccessTools.Method(typeof(Harmony), nameof(Harmony.PatchCategory), new Type[] { typeof(string) });
            yield return AccessTools.Method(typeof(Harmony), nameof(Harmony.PatchAllUncategorized), new Type[] { });
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var getFrameMethod = AccessTools.Method(typeof(StackFrame), nameof(StackFrame.GetMethod));

            for ( var i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                if (code.Calls(getFrameMethod))
                {
                    code.opcode = OpCodes.Call;
                    code.operand = AccessTools.Method(typeof(TheHarmonyHarmonyPatch), nameof(GetNonPatchedMethodFromFrame));
                }
            }

            return codes;
        }

        public static MethodBase GetNonPatchedMethodFromFrame(StackFrame frame)
        {
            var normalResult = frame.GetMethod();
            if(normalResult.ReflectedType == null)
            {
                var nonHarmonyResult = Harmony.GetOriginalMethodFromStackframe(frame);
                return nonHarmonyResult;
            }
            return normalResult;
        }
    }
}
