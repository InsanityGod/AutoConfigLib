using AutoConfigLib.AutoConfig.Fields;
using ConfigLib;
using HarmonyLib;
using ImGuiNET;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Vintagestory.API.Common;

namespace AutoConfigLib.AutoConfig
{
    public static class ConfigGenerator
    {
        public static Dictionary<Type, Config> Configs { get; internal set; }

        public static T GetConfigOverride<T>(ICoreAPICommon api, string fileName)
        {
            var result = api.LoadModConfig<T>(fileName);
            if (!Configs.ContainsKey(typeof(T)))
            {
                Configs[typeof(T)] = new Config
                {
                    Filename = fileName,
                    Type = typeof(T),
                    Value = result
                };
            }
            else
            {
                //Something might be wrong here
            }
            return result;
        }

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

                    var method = AccessTools.Method(typeof(ConfigGenerator), nameof(GetConfigOverride))
                        .MakeGenericMethod(methodInfo.GetGenericArguments());
                    code.opcode = OpCodes.Call;
                    code.operand = method;
                }
            }
            //TODO
            return codes;
        }

        public static void GenerateDefaultConfigLib(ICoreAPI api)
        {
            var configLib = api.ModLoader.GetModSystem<ConfigLibModSystem>();
            foreach ((var type, var config) in Configs)
            {
                config.Mod = api.ModLoader.Mods.First(mod => mod.Systems.First().GetType().Assembly == type.Assembly);
                configLib.RegisterCustomConfig(config.Mod.Info.ModID + " (auto generated)", (string id, ControlButtons buttons) => HandleConfigButtons(api, config, id, buttons));
            }
        }

        public static void HandleConfigButtons(ICoreAPI api, Config config, string id, ControlButtons buttons)
        {
            if (buttons.Save)
            {
                config.Save(api);
            }
            else if (buttons.Defaults)
            {
                //TODO
            }
            else if (buttons.Reload)
            {
                //TODO
            }
            else if (buttons.Restore)
            {
                //TODO
            }

            EditConfig(api, config, id);
        }

        public static void EditConfig(ICoreAPI api, Config config, string id) => AddType(config.Type, config.Value, id);

        public static void AddType(Type type, object instance, string id)
        {
            //TODO group by keywords!
            foreach (var member in type.GetMembers())
            {
                Type fieldType = null;
                if (member is PropertyInfo property)
                {
                    fieldType = property.PropertyType;
                }
                else if (member is FieldInfo field)
                {
                    fieldType = field.FieldType;
                }

                if (fieldType == null) continue;
                if (GenericEnumerableField.TryAdd(instance, member, fieldType, id)) continue;
                if (SimpleField.TryAdd(instance, member, fieldType, id)) continue;
                if(ComplexField.TryAdd(instance, member, fieldType, id)) continue;
                ImGui.Text($"Unsopported object type '{fieldType}'");
            }
        }
    }
}