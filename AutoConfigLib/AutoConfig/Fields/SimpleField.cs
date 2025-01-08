using HarmonyLib;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Vintagestory.API.Util;

namespace AutoConfigLib.AutoConfig.Fields
{
    public static class SimpleField
    {
        public static bool TryAdd(object instance, MemberInfo field, Type fieldType, string id)
        {
            if(field is PropertyInfo property && (!property.CanWrite || !property.CanRead)) return false;
            if(field is FieldInfo fieldInfo && fieldInfo.IsLiteral) return false;

            var method = AccessTools.Method(typeof(SimpleField), nameof(TryAddType))
                .MakeGenericMethod(fieldType);

            var arguments = new object[] { GetImGuiName(field.Name, id), field.GetValue<object>(instance), null };

            var result = method.Invoke(null, arguments);
            if ((bool)arguments[2])
            {
                field.SetValue(instance, result);
                return true;
            }
            return false;
        }

        internal static readonly HashSet<Type> SuppertedSimpleTypes = new()
        {
            typeof(bool),
            typeof(int),
            typeof(float),
            typeof(double),
            typeof(string),
        };

        //TODO Enabled/Disabled ending should be dropdown maybe?
        public static unsafe T TryAddType<T>(string id, T value, out bool success)
        {
            success = true;

            var notNullableType = Nullable.GetUnderlyingType(typeof(T));
            if (notNullableType != null)
            {
                //TODO: do same thing for strings maybe?
                if (SuppertedSimpleTypes.Contains(notNullableType))
                {
                    var method = AccessTools.Method(typeof(SimpleField), nameof(TryAddType))
                        .MakeGenericMethod(notNullableType);

                    if (ImGui.BeginTable($"##{id}-Nullable-Table", 2, ImGuiTableFlags.SizingStretchProp))
                    {

                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();

                        object valueOrDefault = value ?? Activator.CreateInstance(notNullableType);

                        var arguments = new object[] { id, valueOrDefault, null };

                        object result = method.Invoke(null, arguments);

                        ImGui.TableNextColumn();
                        bool isNull = value is null;
                        ImGui.Checkbox($"Is Null##{id}-IsNullCheckbox", ref isNull);


                        //If the value was null
                        if (value is null)
                        {
                            //But we filled in a value
                            if(!result.Equals(Activator.CreateInstance(notNullableType)))
                            {
                                //Assign new value
                                value = (T)result;
                            }
                        }
                        //If the value wasn't null but we checked the IsNullBox
                        else if(isNull)
                        {
                            value = (T)(object)null;
                        }

                        success = (bool)arguments[2];
                    }

                    ImGui.EndTable();
                    return value;
                }
                success = false;
                return value;
            }


            if (typeof(T) == typeof(bool))
            {
                var boolValue = (bool)(object)value;
                ImGui.Checkbox(id, ref boolValue);
                value = (T)(object)boolValue;
            }
            else if (typeof(T) == typeof(int))
            {
                var intValue = (int)(object)value;
                ImGui.InputInt(GetImGuiName(id, id), ref intValue);
                value = (T)(object)intValue;
            }
            else if (typeof(T) == typeof(float))
            {
                var floatValue = (float)(object)value;
                ImGui.InputFloat(id, ref floatValue);
                value = (T)(object)floatValue;
            }
            else if (typeof(T) == typeof(double))
            {
                var doubleValue = (double)(object)value;
                ImGui.InputDouble(id, ref doubleValue);
                value = (T)(object)doubleValue;
            }
            else if (typeof(T) == typeof(string))
            {
                var stringValue = (string)(object)value;
                bool wasNull = stringValue == null;
                if(wasNull) stringValue = string.Empty;

                ImGui.InputText(id, ref stringValue, (uint)AutoConfigLibModSystem.Config.MaxStringLength);
                value = (string.IsNullOrEmpty(stringValue) && wasNull) ?
                    (T)(object)null :
                    (T)(object)stringValue;
            }
            else if(typeof(T).BaseType == typeof(Enum))
            {
                var flags = typeof(T).GetCustomAttribute<FlagsAttribute>();
                if (flags == null)
                {
                    var values = (T[])Enum.GetValues(typeof(T));
                    var keys = values.Select(item => GetHumanReadable(Enum.GetName(typeof(T), item))).ToArray();
                    var currentIndex = keys.IndexOf(GetHumanReadable(Enum.GetName(typeof(T), value)));
                    ImGui.Combo(id, ref currentIndex, keys, keys.Length);

                    value = (T)(object)values[currentIndex];
                }
                else
                {
                    int intValue = Convert.ToInt32(value);
                    bool modified = false;

                    if (ImGui.BeginCombo(id, intValue == 0 ? "None" : value.ToString()))
                    {
                        // Iterate over each enum flag
                        foreach (T flag in Enum.GetValues(typeof(T)))
                        {
                            // Skip the "None" option (optional)
                            if (Convert.ToInt32(flag) == 0)
                                continue;

                            // Check if the current flag is set
                            bool isSelected = (intValue & Convert.ToInt32(flag)) != 0;

                            // Draw a selectable checkbox for this flag
                            if (ImGui.Selectable(flag.ToString(), isSelected))
                            {
                                // Toggle the flag
                                if (isSelected)
                                    intValue &= ~Convert.ToInt32(flag); // Remove the flag
                                else
                                    intValue |= Convert.ToInt32(flag); // Add the flag

                                modified = true;
                            }
                        }

                        ImGui.EndCombo();
                    }

                    // Update the enum value if it was modified
                    if (modified) value = (T)Enum.ToObject(typeof(T), intValue);

                }
            }
            else
            {
                success = false;
                return value;
            }
            //TODO: support for NatFloat

            return value;
        }


        public static string GetImGuiName(string name, string id) => $"{GetHumanReadable(name)}##{id}";

        public static string GetHumanReadable(string str)
        {
        if (string.IsNullOrWhiteSpace(str)) return string.Empty;

            StringBuilder newText = new(str.Length * 2);
            newText.Append(str[0]);

            for (int i = 1; i < str.Length; i++)
            {
                if (char.IsUpper(str[i]) && str[i - 1] != ' ')
                {
                    newText.Append(' ');
                }

                newText.Append(str[i]);
            }

            return newText.Replace('_',' ').ToString();
        }
    }
}
