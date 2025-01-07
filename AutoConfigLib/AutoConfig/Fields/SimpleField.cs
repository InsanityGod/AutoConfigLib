using HarmonyLib;
using ImGuiNET;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
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

        //TODO Enabled ending should be dropdown maybe?
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

                        //TODO: maybe throw warning or exception if we have false here because it shouldn't be possible
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
                ImGui.InputText(id, ref stringValue, 128); //TODO: test and maybe warning if existing string is already longer?
                value = (string.IsNullOrEmpty(stringValue) && wasNull) ?
                    (T)(object)null :
                    (T)(object)stringValue;
            }
            else if(typeof(T).BaseType == typeof(Enum))
            {
                var values = (T[])Enum.GetValues(typeof(T));
                var keys = values.Select(item => GetHumanReadable(Enum.GetName(typeof(T), item))).ToArray();
                var currentIndex = keys.IndexOf(GetHumanReadable(Enum.GetName(typeof(T), value)));
                ImGui.Combo(id, ref currentIndex, keys, keys.Length);

                value = (T)(object)values[currentIndex];
            }
            else
            {
                success = false;
                return value;
            }
            //TODO: find out why IMGui is so weirdly responsive

            //TODO:
            //else if (typeof(V) == typeof(NatFloat))
            //{
            //    
            //    //NatFloat customValue = value as NatFloat;
            //    //customValue.avg = OnInputFloat($"##avg-{row}" + key, customValue.avg, "avg");
            //    //customValue.var = OnInputFloat($"##var-{row}" + key, customValue.var, "var");
            //    //value = (V)Convert.ChangeType(customValue, typeof(NatFloat));
            //}
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

            return newText.ToString();
        }
    }
}
