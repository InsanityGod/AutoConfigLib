using HarmonyLib;
using ImGuiNET;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoConfigLib.Auto.Rendering.Renderers.ComplexTypes
{
    public class ClassRenderer<T> : ComplexRendererBase<T>
    {
        public bool IgnoreReadOnly => true;

        public Dictionary<string, List<FieldRenderDefinition>> FieldRenderInfoByGroup { get; private set; }

        public override void Initialize()
        {
            base.Initialize();

            var members = (AutoConfigLibModSystem.Config.IgnoreAccessModifier ?
                typeof(T).GetMembers(AccessTools.allDeclared) :
                typeof(T).GetMembers())
                .Where(member => !TypeHelper.IsBackingField(member))
                .ToList();

            var memberQuery = members
            //Filter out stuff originatin from object
            .Where(member => member.DeclaringType != typeof(object))
            //Filter out constructor
            .Where(member => member is not ConstructorInfo)
            //Filter out property get and set methods
            .Where(member => member is not MethodInfo method || !members.OfType<PropertyInfo>().Any(
                prop => prop.GetGetMethod() == method || prop.GetSetMethod() == method
            ));

            if (!AutoConfigLibModSystem.Config.ShowObsoleteMembers)
            {
                memberQuery = memberQuery.Where(member => member.GetCustomAttribute<ObsoleteAttribute>() == null);
            }

            if (!AutoConfigLibModSystem.Config.ShowNonProtoMembersInProtoClass)
            {
                var protoAttr = typeof(T).GetCustomAttribute<ProtoContractAttribute>();
                if (protoAttr != null && protoAttr.ImplicitFields == ImplicitFields.None) memberQuery = memberQuery.Where(member => member.GetCustomAttribute<ProtoMemberAttribute>() != null);
            }

            members = memberQuery.ToList();

            FieldRenderInfoByGroup = members.Select(FieldRenderDefinition.Create)
                .Where(item => item.IsVisible) //No point saving invisible items
                .GroupBy(x => x.Category)
                .ToDictionary(item => item.Key, item => item.ToList());

            if(AutoConfigLibModSystem.Config.AutoGroupByName && FieldRenderInfoByGroup.Count == 1 && FieldRenderInfoByGroup.TryGetValue(string.Empty, out var uncategorized))
            {
                FieldRenderInfoByGroup = Grouper.CategorizeByName(uncategorized);
            }
        }

        public bool AreMembersSorted { get; private set; } = false;
        public virtual void SortMembers()
        {
            //TODO: more sorting/ordering stuff

            foreach((var category, var collection) in FieldRenderInfoByGroup)
            {
                var sortQuery = collection
                    .OrderBy(item => AutoConfigLibModSystem.Config.ComplexTypeAllignment switch
                    {
                        Enums.EAlignment.Top => !item.ValueRenderer.ShouldBeInsideCollapseHeader,
                        Enums.EAlignment.Bottom => item.ValueRenderer.ShouldBeInsideCollapseHeader,
                        _ => true,
                    });

                FieldRenderInfoByGroup[category] = sortQuery.ToList();
            }
            
            AreMembersSorted = true;
        }

        public override T RenderValue(T instance, string id, FieldRenderDefinition fieldDefinition = null)
        {
            if(!AreMembersSorted) SortMembers();

            foreach ((var category, var fields) in FieldRenderInfoByGroup.OrderBy(item => !string.IsNullOrWhiteSpace(item.Key)))
            {
                if (!string.IsNullOrWhiteSpace(category)) ImGuiHelper.IndentedSeparatorText(category);

                foreach (var field in fields)
                {
                    if (field.ValueRenderer == null || !field.IsVisible) continue;
                    ImGui.BeginDisabled(field.IsReadOnly && !field.ValueRenderer.IgnoreReadOnly);
                    var value = field.GetValue(instance);
                    object result;
                    if (AutoConfigLibModSystem.Config.UseInstanceTypeOverFieldType && value != null && value is not MethodInfo)
                    {
                        result = Renderer.GetOrCreateRenderForType(value.GetType()).RenderObject(value, $"{id}-{field.SubId}", field);
                    }
                    else result = field.ValueRenderer.RenderObject(value, $"{id}-{field.SubId}", field);
                    
                    if(!field.ValueRenderer.ShouldBeInsideCollapseHeader) ImGuiHelper.TooltipIcon(field.Description);

                    if (!field.IsReadOnly && result != value) field.SetValue(instance, result);

                    ImGui.EndDisabled();
                }
            }
            return instance;
        }
    }
}