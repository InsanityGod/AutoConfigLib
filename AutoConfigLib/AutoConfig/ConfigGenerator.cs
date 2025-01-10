﻿using AutoConfigLib.AutoConfig.Fields;
using ConfigLib;
using HarmonyLib;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Vintagestory.API.Common;

namespace AutoConfigLib.AutoConfig
{
    public static class ConfigGenerator
    {
        public static Dictionary<string, Config> Configs { get; internal set; }

        [Obsolete]
        public static T GetConfigOverride<T>(ICoreAPICommon api, string fileName)
        {
            var result = api.LoadModConfig<T>(fileName);
            if(api is not ICoreAPI coreApi) return result; //shouldn't happen but just in case

            if (!Configs.ContainsKey(fileName))
            {
                var config = new Config
                {
                    configPath = fileName,
                    Type = typeof(T),
                };

                if(coreApi.Side == EnumAppSide.Client) config.ClientValue = result;
                if(coreApi.Side == EnumAppSide.Server) config.ServerValue = result;

                Configs[fileName] = config;
            }
            else
            {
                var config = Configs[fileName];

                if(AutoConfigLibModSystem.Config.AutoMergeClientServerConfig) return (T)(config.ServerValue ?? config.ClientValue);

                if(coreApi.Side == EnumAppSide.Client) config.ClientValue ??= result;
                if(coreApi.Side == EnumAppSide.Server) config.ServerValue ??= result;
            }
            return result;
        }

        public static void GenerateDefaultConfigLib(ICoreAPI api)
        {
            var configLib = api.ModLoader.GetModSystem<ConfigLibModSystem>();
            foreach (var config in Configs.Values)
            {
                config.Mod = AutoConfigLibModSystem.CoreServerAPI?.ModLoader.Mods.FirstOrDefault(mod => mod.Systems.FirstOrDefault()?.GetType().Assembly == config.Type.Assembly);
                config.Mod ??= AutoConfigLibModSystem.CoreClientAPI?.ModLoader.Mods.FirstOrDefault(mod => mod.Systems.FirstOrDefault()?.GetType().Assembly == config.Type.Assembly);
            }

            foreach (var config in Configs.Values)
            {
                if(config.Mod == null)
                {
                    api.Logger.Error($"Could not find mod for '{config.Type?.FullName}'");
                    continue;
                }

                var value = config.ClientValue;
                value ??= config.ServerValue;

                config.PrimaryValue = value;
                if(config.ServerValue != null && !ReferenceEquals(config.ServerValue, value))
                {
                    api.Logger.Warning($"{config.Mod.Info.Name}', has a requested config '{config.Type}' on both local client and server, skipping auto config ('ClientServerConfigAutoMerge' can be enabled to prevent this)");
                    continue;
                }

                var extraStr = string.Empty;
                if(Configs.Values.Count(conf => conf.Mod?.Info.ModID == config.Mod.Info.ModID) > 1)
                {
                    extraStr = " - " + config.configPath.Split('.')[0];
                }

                configLib.RegisterCustomConfig($"{config.Mod.Info.ModID}{extraStr} (auto)", (string id, ControlButtons buttons) => HandleConfigButtons(api, config, id, buttons));
            }

            if(AutoConfigLibModSystem.Config.LoadWorldConfig) WorldConfigEditor.LoadWorldConfig(api);
        }

        public static void HandleConfigButtons(ICoreAPI api, Config config, string id, ControlButtons buttons)
        {
            if (buttons.Save)
            {
                config.Save(api);
            }
            else if (buttons.Defaults || buttons.Restore)
            {
                try
                {
                    object newInstance = null;

                    if (buttons.Defaults)
                    {
                        newInstance = Activator.CreateInstance(config.Type);
                    }
                    else
                    {
                        var method = typeof(ICoreAPICommon)
                            .GetMethods()
                            .First(method => method.Name == nameof(ICoreAPICommon.LoadModConfig) && method.IsGenericMethod)
                            .MakeGenericMethod(config.Type);

                        newInstance = method.Invoke(api, new object[] { config.configPath });
                    }

                    foreach (var member in config.Type.GetMembers())
                    {
                        if (member is PropertyInfo property && property.CanRead && property.CanWrite)
                        {
                            property.SetValue(config.PrimaryValue, property.GetValue(newInstance));
                        }
                        else if (member is FieldInfo field && !field.IsInitOnly)
                        {
                            field.SetValue(config.PrimaryValue, field.GetValue(newInstance));
                        }
                    }
                }
                catch(Exception ex)
                {
                    api.Logger.Error($"Failed to apply default for '{config.Type}' exception:{ex}");
                }
            }
            else if (buttons.Reload)
            {
                //TODO not sure what to do here
            }

            EditConfig(api, config, id);
        }

        public static void EditConfig(ICoreAPI api, Config config, string id)
        {
            try
            {
                AddType(config.Type, config.PrimaryValue, id);
            }
            catch (Exception ex) 
            {
                ImGui.Text($"Unexpected Error, please report on mod page: {ex}");
            }
        }

        public static void AddType(Type type, object instance, string id)
        {
            if(instance is null) return;
            //TODO group by keywords
            foreach (var member in type.GetMembers())
            {
                Type fieldType = null;
                if (member is PropertyInfo property)
                {
                    fieldType = property.PropertyType;
                    var getter = property.GetGetMethod();
                    if(getter == null || getter.IsStatic) continue;
                }
                else if (member is FieldInfo field)
                {
                    if(field.IsStatic) continue;
                    fieldType = field.FieldType;
                }

                if (fieldType == null) continue;
                if (GenericEnumerableField.TryAdd(instance, member, fieldType, id)) continue;
                if (SimpleField.TryAdd(instance, member, fieldType, id)) continue;
                if(SimpleField.SuppertedSimpleTypes.Contains(fieldType) || SimpleField.SuppertedSimpleTypes.Contains(fieldType.BaseType)) continue; //If we have a simple type that can't be read or writen to
                
                if(ComplexField.TryAdd(instance, member, fieldType, id)) continue;
                if(AutoConfigLibModSystem.Config.ShowPresenceOfUnsupportedTypes) ImGui.TextWrapped($"Unsupported field of type '{fieldType}'");
            }
        }
    }
}