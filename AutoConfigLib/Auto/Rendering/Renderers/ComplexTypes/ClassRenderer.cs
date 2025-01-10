using HarmonyLib;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AutoConfigLib.Auto.Rendering.Renderers.ComplexTypes
{
    public class ClassRenderer<T> : ComplexRendererBase<T>
    {
        public bool IgnoreReadOnly => true;

        public Dictionary<string, FieldRenderDefinition[]> FieldRenderInfoByGroup { get; private set; }

        public override void Initialize()
        {
            base.Initialize();
            var members = AutoConfigLibModSystem.Config.IgnoreAccessModifier ?
                typeof(T).GetMembers(AccessTools.allDeclared) :
                typeof(T).GetMembers();

            FieldRenderInfoByGroup = members.Select(FieldRenderDefinition.Create)
                .GroupBy(x => x.Category)
                .ToDictionary(item => item.Key, item => item.ToArray());
        }

        public override T RenderValue(T instance, string id, FieldRenderDefinition fieldDefinition = null)
        {
            if (fieldDefinition != null)
            {
                if (!ImGui.CollapsingHeader($"{fieldDefinition.Name}##{id}-colapse")) return instance;
                ImGui.Indent();
            }

            foreach ((var category, var fields) in FieldRenderInfoByGroup.OrderBy(item => string.IsNullOrWhiteSpace(item.Key)))
            {
                if (!string.IsNullOrWhiteSpace(category)) ImGuiHelper.IndentedSeparatorText(category);

                foreach (var field in fields)
                {
                    if (field.ValueRenderer == null || !field.IsVisible) continue;
                    ImGui.BeginDisabled(field.IsReadOnly && !field.ValueRenderer.IgnoreReadOnly);
                    var value = field.GetValue(instance);
                    var result = field.ValueRenderer.RenderObject(value, $"{id}-{field.SubId}", field);
                    ImGuiHelper.TooltipIcon(field.Description);

                    if (!field.IsReadOnly && result != value) field.SetValue(instance, result);

                    ImGui.EndDisabled();
                }
            }
            if (fieldDefinition != null) ImGui.Unindent();
            return instance;
        }
    }
}
