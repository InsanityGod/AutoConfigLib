using AutoConfigLib.Auto.Generators;
using ImGuiNET;
using System;
using System.Numerics;

namespace AutoConfigLib.Auto.Rendering.Renderers.ComplexTypes
{
    public abstract class ComplexRendererBase<T> : IRenderer<T>, ISupportGroupDisplacement
    {
        
        public bool CanBeInitialized { get; private set; }

        public virtual bool ShouldBeInsideCollapseHeader => true;

        public bool UseGroupDisplacement { get; set; }
        public float GroupDisplacementX { get; set; }
        public float GroupExtraSpaceX { get; set; }

        public virtual void Initialize() => CanBeInitialized = InstanceGenerator.CanGenerate<T>();

        public abstract T RenderValue(T instance, string id, FieldRenderDefinition fieldDefinition = null);

        public T Render(T instance, string id, FieldRenderDefinition fieldDefinition = null)
        {
            if (instance == null)
            {
                if (AutoConfigLibModSystem.Config.AutoInitializeNullFields)
                {
                    if (CanBeInitialized) return InstanceGenerator.Generate<T>();
                    ImGui.TextDisabled($"type of {fieldDefinition.Name ?? "unknown"} field cannot be initialized ({typeof(T)})");
                    return instance;
                }
                ImGui.BeginDisabled(!CanBeInitialized);
                if (ImGui.Button($"Initialize {fieldDefinition.Name}##{id}-initialize-button"))
                {
                    instance = InstanceGenerator.Generate<T>();
                }
                else if(ShouldBeInsideCollapseHeader) ImGuiHelper.TooltipIcon(fieldDefinition.Description);
                ImGuiHelper.SetExceptionToolTip(CanBeInitialized ? null : $"type of {fieldDefinition.Name ?? "unknown"} field cannot be initialized ({typeof(T)})");
                ImGui.EndDisabled();
                return instance;
            }

            if (ShouldBeInsideCollapseHeader && fieldDefinition != null)
            {
                if (!ImGui.CollapsingHeader($"{fieldDefinition.Name}##{id}-colapse"))
                {
                    ImGuiHelper.TooltipIcon(fieldDefinition.Description, true);
                    return instance;
                }
                ImGuiHelper.TooltipIcon(fieldDefinition.Description, true);

                ImGui.Indent();
            }

            //Localize for recursive rendering support
            var isUsingGroup = UseGroupDisplacement;
            UseGroupDisplacement = false;

            var cursorPos = ImGui.GetCursorPos();
            if (isUsingGroup)
            {
                ImGui.SetCursorPosX(cursorPos.X - GroupDisplacementX);
                ImGui.PopClipRect();
                
                var min = new Vector2(-1, -1);
                var max = new Vector2(float.MaxValue, float.MaxValue);
                ImGui.PushClipRect(min, max, true);
                ImGui.BeginGroup(); //TODO: maybe see if we can get this to use the sapce of the delete column as well
                ImGui.Indent();
            }

            var result = RenderValue(instance, id, fieldDefinition);

            if(ShouldBeInsideCollapseHeader) ImGui.Dummy(new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetTextLineHeight() * AutoConfigLibModSystem.Config.ComplexTypePaddingMultiplier));

            if (isUsingGroup)
            {
                ImGui.Unindent();
                ImGui.EndGroup();
                ImGui.SetCursorPosX(cursorPos.X);
            }

            if (ShouldBeInsideCollapseHeader && fieldDefinition != null) ImGui.Unindent();
            
            return result;
        }
    }
}