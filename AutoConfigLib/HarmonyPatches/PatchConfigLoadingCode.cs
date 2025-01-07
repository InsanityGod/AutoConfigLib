using AutoConfigLib.AutoConfig;
using ConfigLib;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;
using Vintagestory.API.Common;

namespace AutoConfigLib.HarmonyPatches
{
    public static class PatchConfigLoadingCode
    {
        public static void PatchConfigStuff(ICoreAPI api, Harmony harmony)
        {
            var modAssemblies = api.ModLoader.Mods
                .Where(mod => mod.Systems.Count > 0)
                .Select(mod => mod.Systems.First().GetType().Assembly)
                .Where(assembly => !Array.Exists(assembly.GetReferencedAssemblies(), assembly => assembly.Name == typeof(ConfigLibModSystem).Assembly.GetName().Name))
                .Select(assembly => (assembly, AssemblyDefinition.ReadAssembly(assembly.Location)));

            foreach ((var assembly, var monoAssembly) in modAssemblies)
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
                                    if (instruction.OpCode == OpCodes.Callvirt && instruction.Operand is MethodReference methodRef)
                                    {
                                        if (methodRef.IsGenericInstance && methodRef.Name == nameof(ICoreAPICommon.LoadModConfig))
                                        {
                                            var realMethods = assembly.GetTypes()
                                                .First(realType => realType.Name == type.Name)
                                                .GetMethods(AccessTools.allDeclared)
                                                .Where(realMethod => realMethod.Name == method.Name)
                                                .ToList();
                                            var realMethod = realMethods.FirstOrDefault();
                                            if (realMethods.Count != 1)
                                            {
                                                api.Logger.Warning($"Failed to bind AutoConfig for {assembly.GetName()} in method '{method.FullName}'");
                                                break;
                                            }

                                            try
                                            {
                                                harmony.Patch(realMethod, transpiler: new HarmonyMethod(AccessTools.Method(typeof(ConfigGenerator), nameof(ConfigGenerator.Transpiler))));
                                            }
                                            catch (Exception ex)
                                            {
                                                api.Logger.Warning($"failed to inject auto config for {type.Name} in method {realMethod.Name}, exception: {ex}");
                                            }

                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}