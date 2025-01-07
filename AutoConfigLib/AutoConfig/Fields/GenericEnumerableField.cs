using HarmonyLib;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using YamlDotNet.Core.Tokens;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AutoConfigLib.AutoConfig.Fields
{
    public static class GenericEnumerableField
    {

        public static bool TryAdd(object instance, MemberInfo field, Type fieldType, string id)
        {
            if(fieldType.IsArray && fieldType.BaseType == typeof(Array))
            {
                if(ImGui.CollapsingHeader(SimpleField.GetImGuiName(field.Name, $"{id}-collapse-{field.Name}")))
                {
                    var result = AccessTools.Method(typeof(GenericEnumerableField), nameof(AddArray))
                        .MakeGenericMethod(fieldType.GetElementType())
                        .Invoke(null, parameters: new object[] { field.GetValue<object>(instance), field, fieldType, $"{id}-{field.Name}" });

                    field.SetValue(instance, result);
                }
                return true;
            }

            if (!fieldType.IsGenericType) return false;

            var genericFieldType = fieldType.GetGenericTypeDefinition();
            if(genericFieldType == typeof(Dictionary<,>))
            {
                if(fieldType.GenericTypeArguments[0] != typeof(string)) return true; //TODO create support for enum (and maybe integer) keys 
                if(ImGui.CollapsingHeader(SimpleField.GetImGuiName(field.Name, $"{id}-collapse-{field.Name}")))
                {
                    AccessTools.Method(typeof(GenericEnumerableField), nameof(AddDictionary))
                        .MakeGenericMethod(fieldType.GenericTypeArguments[1])
                        .Invoke(null, parameters: new object[] { instance, field, fieldType, $"{id}-{field.Name}" });
                }
                return true;
            }
            return false;
        }

        public static T[] AddArray<T>(object instance, MemberInfo member, Type fieldType, string id)
        {
            var array = (T[])instance;
            T[] newArray = null;
            if (ImGui.BeginTable($"##{id}-array-{member.Name}", 2, ImGuiTableFlags.BordersOuter | ImGuiTableFlags.SizingStretchProp ))
            {
                ImGui.TableSetupColumn($"##{id}-array-item-col", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn($"##{id}-array-del-col", ImGuiTableColumnFlags.WidthFixed);
                for(int i = 0; i < array.Length; i++)
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    var itemId = $"##{id}-array-{member.Name}-{i}";

                    ImGui.SetNextItemWidth(-1);
                    var result = SimpleField.TryAddType(itemId, array[i], out bool success);
                    //TODO: complex types
                    
                    if (success)
                    {
                        array[i] = result;
                    }
                    else
                    {
                        ImGui.Text($"Unsopported array element of type '{typeof(T)}'");
                        //This array is a not supported type
                        ImGui.EndTable();
                        return array;
                    }


                    ImGui.TableNextColumn();

                    if (ImGui.Button($"Remove##{id}-RemoveArrayItem-{i}"))
                    {
                        newArray = (newArray ?? array).RemoveAt(i);
                    }
                }
            }
            ImGui.EndTable();

            if (ImGui.Button($"Add##{id}-ArrayAdd", new Vector2(ImGui.GetContentRegionAvail().X, 0)))
            {
                T newInstance;

                try
                {
                    newInstance = Activator.CreateInstance<T>();
                }
                catch
                {
                    newInstance = default;
                    //Can't initialize this type so use default
                }
                newArray = (newArray ?? array).Append(newInstance);
            }

            return newArray ?? array;
        }

        public static void AddDictionary<V>(object instance, MemberInfo field, Type fieldType, string id)
        {
            var dict = field.GetValue<Dictionary<string, V>>(instance);
            if (ImGui.BeginTable($"{id}-dict-{field.Name}", 3, ImGuiTableFlags.BordersOuter | ImGuiTableFlags.NoPadInnerX))
            {
                ImGui.TableSetupColumn($"##{id}-dict-key-col", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn($"##{id}-dict-val-col", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn($"##{id}-dict-del-col", ImGuiTableColumnFlags.WidthFixed);
                for (int row = 0; row < dict.Count; row++)
                {
                    ImGui.TableNextRow();
                    string key = dict.Keys.ElementAt(row);
                    string prevKey = (string)key.Clone();
                    V value = dict.Values.ElementAt(row);

                    ImGui.TableNextColumn();

                    ImGui.SetNextItemWidth(-1);
                    ImGui.InputText($"##{id}-DictKey-{row}", ref key, 300);
                    if (prevKey != key)
                    {
                        dict.Remove(prevKey);
                        dict.TryAdd(key, value);
                        value = dict.Values.ElementAt(row);
                    }
                    ImGui.TableNextColumn();
                    //TODO: ensure keys are unique

                    ImGui.SetNextItemWidth(-1);
                    value = SimpleField.TryAddType($"##{id}-DictValue-{row}-{key}", value, out bool success);

                    if (!success)
                    {
                        ImGui.Text($"Unsopported dict value type '{typeof(V)}'");
                        //This dict key is a not supported type
                    }

                    dict[key] = value;
                    ImGui.TableNextColumn();
                    if (ImGui.Button($"Remove##{id}-RemoveDictKey-{row}-{key}"))
                    {
                        dict.Remove(key);
                    }
                }
                ImGui.EndTable();
            }

            if (ImGui.Button($"Add##{id}-DictAdd", new Vector2(ImGui.GetContentRegionAvail().X, 0)))
            {
                int rowId = dict.Count;
                string newKey = $"row {rowId}";
                while (dict.ContainsKey(newKey)) newKey = $"row {++rowId}";
                dict.TryAdd(newKey, default);
            }
        }
    }
}
