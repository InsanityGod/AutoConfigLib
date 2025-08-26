using ConfigLib;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Common;

namespace AutoConfigLib.HarmonyPatches;

public static class PatchConfigLoadingCode
{
    public static void FindAndPatchMethods(Harmony harmony)
    {
        var assembliesToScan = AppDomain.CurrentDomain.GetAssemblies()
            //Skip any dynamic assembly (cause I don't know how to read the assembly definition of these)
            .Where(assembly => !string.IsNullOrEmpty(assembly.Location))
            //Skip anything that does not reference vintage story
            .Where(assembly => Array.Exists(assembly.GetReferencedAssemblies(), assembly => assembly.Name == typeof(ModSystem).Assembly.GetName().Name))
            //Skip anything that references ConfigLib (As they should have a manual implementation)
            .Where(assembly => !Array.Exists(assembly.GetReferencedAssemblies(), assembly => assembly.Name == typeof(ConfigLibModSystem).Assembly.GetName().Name))
            .Select(assembly => (assembly, AssemblyDefinition.ReadAssembly(assembly.Location)));

        foreach ((var assembly, var monoAssembly) in assembliesToScan)
        {
            try
            {
                ScanAndPatchAssembly(harmony, assembly, monoAssembly);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unnexpected exception occured during AutoConfigLib Patching of '{assembly.FullName}', exception: {ex}");
            }
        }
    }

    private static void ScanAndPatchAssembly(Harmony harmony, Assembly assembly, AssemblyDefinition monoAssembly)
    {
        foreach (var module in monoAssembly.Modules)
        {
            foreach (var type in module.Types)
            {
                foreach (var method in type.Methods)
                {
                    if (method.HasBody)
                    {
                        // Access and print the method body
                        foreach (var instruction in method.Body.Instructions)
                        {
                            if (instruction.OpCode != OpCodes.Callvirt || instruction.Operand is not MethodReference methodRef || !methodRef.IsGenericInstance || methodRef.Name != nameof(ICoreAPICommon.LoadModConfig)) continue;

                            var realMethods = assembly.GetTypes()
                                .First(realType => realType.Name == type.Name)
                                .GetMethods(AccessTools.allDeclared)
                                .Where(realMethod => realMethod.Name == method.Name)
                                .ToList();
                            var realMethod = realMethods.FirstOrDefault();
                            if (realMethods.Count != 1)
                            {
                                Console.WriteLine($"AutoConfig: Failed to find real method for {assembly.FullName} {method.FullName}");
                                break;
                            }

                            try
                            {
                                harmony.Patch(realMethod, transpiler: new HarmonyMethod(AccessTools.Method(typeof(ConfigInterception), nameof(ConfigInterception.Transpiler))));
                            }
                            catch
                            {
                                Console.WriteLine($"AutoConfig failed to inject auto config for {assembly.FullName} {type.Name} in method {realMethod.Name}");
                            }

                            break;
                        }
                    }
                }
            }
        }
    }
}