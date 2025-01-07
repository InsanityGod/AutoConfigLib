using HarmonyLib;
using ImGuiNET;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;

namespace AutoConfigLib.AutoConfig.Fields
{
    public static class ComplexField
    {
        public static bool TryAdd(object instance, MemberInfo field, Type fieldType, string id)
        {
            if(instance is null) return false;

            var arguments = new object[] { $"{id}-{field.Name}", field.Name, field.GetValue<object>(instance), null };

            var result = AccessTools.Method(typeof(ComplexField), nameof(TryAddType))
                .MakeGenericMethod(fieldType)
                .Invoke(null, arguments);

            field.SetValue(instance, result);

            return (bool)arguments[3];
        }

        public static T TryAddType<T>(string id, string name, T value, out bool success)
        {
            success = false;

            if(value is null) return value;
            if(typeof(T).IsValueType) return value;
            
            success = true;

            if(ImGui.CollapsingHeader(SimpleField.GetImGuiName(name, $"{id}-collapse")))
            {
                ImGui.Indent();
            
                ConfigGenerator.AddType(typeof(T), value, id);
                
                ImGui.Unindent();
            }

            return value;
        }
    }
}
