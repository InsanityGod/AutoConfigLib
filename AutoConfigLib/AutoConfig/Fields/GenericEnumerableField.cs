using AutoConfigLib.AutoConfig.Generators;
using HarmonyLib;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Vintagestory.API.Util;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AutoConfigLib.AutoConfig.Fields
{
    public static class GenericEnumerableField
    {

        public static bool TryAdd(object instance, MemberInfo field, Type fieldType, string id)
        {
            var arguments = new object[] { $"{id}-{field.Name}", field.Name, field.GetValue<object>(instance), null };

            var result = AccessTools.Method(typeof(GenericEnumerableField), nameof(TryAddType))
                .MakeGenericMethod(fieldType)
                .Invoke(null, arguments);

            field.SetValue(instance, result);
            return (bool)arguments[3];
        }

        public static T TryAddType<T>(string id, string name, T value, out bool success)
        {
            Type fieldType = typeof(T);
            success = false;
            if(fieldType.IsArray && fieldType.BaseType == typeof(Array))
            {
                success = true;
                if(ImGui.CollapsingHeader(SimpleField.GetImGuiName(name, $"{id}-collapse")))
                {
                    ImGui.Indent();
                    value = (T)AccessTools.Method(typeof(GenericEnumerableField), nameof(AddArray))
                        .MakeGenericMethod(fieldType.GetElementType())
                        .Invoke(null, parameters: new object[] { value, id });
                    ImGui.Unindent();
                }
                return value;
            }
            
            if (!fieldType.IsGenericType) return value;

            var genericFieldType = fieldType.GetGenericTypeDefinition();
            if(genericFieldType == typeof(Dictionary<,>))
            {
                success = true;
                
                try
                {
                    value ??= Activator.CreateInstance<T>();
                }
                catch
                {
                    ImGui.Text($"#Could not init '{typeof(T)}'#{id}-collapse-error");
                    return value;
                }

                if(ImGui.CollapsingHeader(SimpleField.GetImGuiName(name, $"{id}-collapse")))
                {
                    ImGui.Indent();
                    AccessTools.Method(typeof(GenericEnumerableField), nameof(AddDictionary))
                        .MakeGenericMethod(fieldType.GenericTypeArguments)
                        .Invoke(null, parameters: new object[] { value, id });
                    ImGui.Unindent();
                }
                return value;
            }

            if(genericFieldType == typeof(List<>))
            {
                success = true;
                
                try
                {
                    value ??= Activator.CreateInstance<T>();
                }
                catch
                {
                    ImGui.Text($"#Could not init '{typeof(T)}'#{id}-collapse-error");
                    return value;
                }

                if(ImGui.CollapsingHeader(SimpleField.GetImGuiName(name, $"{id}-collapse")))
                {
                    ImGui.Indent();
                    AccessTools.Method(typeof(GenericEnumerableField), nameof(AddList))
                        .MakeGenericMethod(fieldType.GenericTypeArguments)
                        .Invoke(null, parameters: new object[] { value, id });
                    ImGui.Unindent();
                }
                return value;
            }

            if (typeof(T).IsICollection())
            {
                success = true;

                if(!AutoConfigLibModSystem.Config.UseDefaultImplementationForCollections) return value;

                try
                {
                    value ??= Activator.CreateInstance<T>();
                }
                catch
                {
                    ImGui.Text($"#Could not init '{typeof(T)}'#{id}-collapse-error");
                    return value;
                }

                if(ImGui.CollapsingHeader(SimpleField.GetImGuiName(name, $"{id}-collapse")))
                {
                    ImGui.Indent();
                    AccessTools.Method(typeof(GenericEnumerableField), nameof(AddGenericCollection))
                        .MakeGenericMethod(fieldType.GenericTypeArguments)
                        .Invoke(null, parameters: new object[] { value, id });
                    ImGui.Unindent();
                }
                return value;
            }

            return value;
        }

        public static T[] AddArray<T>(T[] array, string id)
        {
            array ??= Array.Empty<T>();
            T[] newArray = null;
            if (ImGui.BeginTable($"##{id}-array", 2, ImGuiTableFlags.BordersOuter | ImGuiTableFlags.SizingStretchProp ))
            {
                ImGui.TableSetupColumn($"##{id}-array-item-col", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn($"##{id}-array-del-col", ImGuiTableColumnFlags.WidthFixed);
                for(int i = 0; i < array.Length; i++)
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    var itemId = $"{id}-array-{i}";

                    ImGui.SetNextItemWidth(-1);
                    var result = TryAddType(itemId, "Value", array[i], out bool arrayItemSuccess);
                    if(!arrayItemSuccess) result = SimpleField.TryAddType(itemId, array[i], out arrayItemSuccess);
                    if(!arrayItemSuccess) result = ComplexField.TryAddType(itemId, "Value", array[i], out arrayItemSuccess);
                    
                    if (arrayItemSuccess)
                    {
                        array[i] = result;
                    }
                    else
                    {
                        ImGui.Text($"Unsupported array element of type '{typeof(T)}'");
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

        public static unsafe void AddList<T>(List<T> list, string id)
        {
            if(list == null) return;

            if (ImGui.BeginTable($"##{id}-list", 2, ImGuiTableFlags.BordersOuter | ImGuiTableFlags.SizingStretchProp ))
            {
                ImGui.TableSetupColumn($"##{id}-list-item-col", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn($"##{id}-list-del-col", ImGuiTableColumnFlags.WidthFixed);
                for(int i = 0; i < list.Count; i++)
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    var itemId = $"{id}-list-{i}";

                    ImGui.SetNextItemWidth(-1);
                    var result = TryAddType(itemId, "Value", list[i], out bool listItemSuccess);
                    if(!listItemSuccess) result = SimpleField.TryAddType(itemId, list[i], out listItemSuccess);
                    if(!listItemSuccess) result = ComplexField.TryAddType(itemId, "Value", list[i], out listItemSuccess);
                    
                    if (listItemSuccess)
                    {
                        list[i] = result;
                    }
                    else
                    {
                        ImGui.Text($"Unsupported list element of type '{typeof(T)}'");
                        //This list is a not supported type
                        ImGui.EndTable();
                        return;
                    }


                    ImGui.TableNextColumn();

                    if (ImGui.Button($"Remove##{id}-RemoveListItem-{i}"))
                    {
                        list.RemoveAt(i);
                    }
                }
            }
            ImGui.EndTable();

            if (ImGui.Button($"Add##{id}-ListAdd", new Vector2(ImGui.GetContentRegionAvail().X, 0)))
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
                list.Add(newInstance);
            }
        }

        public static unsafe void AddGenericCollection<T>(ICollection<T> collection, string id)
        {
            if(collection == null) return;

            if (ImGui.BeginTable($"##{id}-array", 2, ImGuiTableFlags.BordersOuter | ImGuiTableFlags.SizingStretchProp ))
            {
                ImGui.TableSetupColumn($"##{id}-array-item-col", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn($"##{id}-array-del-col", ImGuiTableColumnFlags.WidthFixed);

                var i = 0;
                foreach (T item in collection.ToList())
                {

                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    var itemId = $"{id}-array-{item.GetHashCode()}";

                    ImGui.SetNextItemWidth(-1);
                    var newItem = TryAddType(itemId, "Value", item, out bool arrayItemSuccess);
                    if(!arrayItemSuccess) newItem = SimpleField.TryAddType(itemId, item, out arrayItemSuccess);
                    if(!arrayItemSuccess) newItem = ComplexField.TryAddType(itemId, "Value", item, out arrayItemSuccess);
                    
                    if (arrayItemSuccess)
                    {
                        if (!item.Equals(newItem))
                        {
                            collection.Remove(item);
                            collection.Add(newItem);
                            //TODO see if we can make sure focus isn't lost when we modify strings
                        }
                    }
                    else
                    {
                        ImGui.Text($"Unsupported array element of type '{typeof(T)}'");
                        //This array is a not supported type
                        ImGui.EndTable();
                        break;
                    }


                    ImGui.TableNextColumn();

                    if (ImGui.Button($"Remove##{id}-RemoveArrayItem-{i}"))
                    {
                        collection.Remove(item);
                    }

                    i++;
                }

            }
            ImGui.EndTable();

            if (ImGui.Button($"Add##{id}-ArrayAdd", new Vector2(ImGui.GetContentRegionAvail().X, 0)))
            {
                T newInstance;

                try
                {
                    if (collection is ISet<T>)
                    {
                        newInstance = UniqueGenerator.GenerateUnique(collection);
                    }
                    else newInstance = Activator.CreateInstance<T>();
                }
                catch
                {
                    newInstance = default;
                    //Can't initialize this type so use default
                }

                try
                {
                    collection.Add(newInstance);
                }
                catch
                {
                    //should only happen if someone messed up their collection type
                }

            }
        }

        public static void AddDictionary<K, V>(Dictionary<K, V> dict, string id)
        {
            if (ImGui.BeginTable($"{id}-dict", 3, ImGuiTableFlags.BordersOuter | ImGuiTableFlags.NoPadInnerX))
            {
                ImGui.TableSetupColumn($"##{id}-dict-key-col", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn($"##{id}-dict-val-col", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn($"##{id}-dict-del-col", ImGuiTableColumnFlags.WidthFixed);
                for (int row = 0; row < dict.Count; row++)
                {
                    ImGui.TableNextRow();

                    K key = dict.Keys.ElementAt(row);
                    V value = dict.Values.ElementAt(row);
                    ImGui.TableNextColumn();

                    ImGui.SetNextItemWidth(-1);
                    var newKey = SimpleField.TryAddType($"{id}-DictKey-{row}", key, out bool successKey);

                    if (!successKey) ImGui.Text($"Unsupported dict value type '{typeof(K)}'");

                    if (successKey && !key.Equals(newKey))
                    {
                        if (!dict.ContainsKey(newKey))
                        {
                            //Ensuring uniqueness of key
                            dict.Remove(key);
                            dict.TryAdd(newKey, value);
                            value = dict.Values.ElementAt(row);

                            key = newKey;
                        }
                    }
                    ImGui.TableNextColumn();
                    
                    ImGui.SetNextItemWidth(-1);

                    value = TryAddType($"{id}-DictValue-{row}-{key}", "Value", value, out bool successValue);
                    if (!successValue) value = SimpleField.TryAddType($"{id}-DictValue-{row}-{key}", value, out successValue);
                    if (!successValue) value = ComplexField.TryAddType($"{id}-DictValue-{row}-{key}", "Value", value, out successValue);
                    //TODO: see if we can have the complex field use all that empty space on the left
                    if (!successValue) ImGui.Text($"Unsupported dict value type '{typeof(V)}'");

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
                try
                {
                    var newKey = UniqueGenerator.GenerateUnique(dict.Keys);
                    V newInstance;

                    try
                    {
                        newInstance = Activator.CreateInstance<V>();
                    }
                    catch
                    {
                        newInstance = default;
                        //Can't initialize this type so use default
                    }
                    dict.TryAdd(newKey, newInstance);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                    //failed to add key
                }
            }
        }
    }
}
