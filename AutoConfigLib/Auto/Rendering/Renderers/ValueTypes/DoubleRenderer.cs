using ImGuiNET;
using System;

namespace AutoConfigLib.Auto.Rendering.Renderers.ValueTypes
{
    public class DoubleRenderer : ValueRendererBase<double>
    {
        public override void RenderValue(ref double instance, string id, FieldRenderDefinition fieldDefinition = null)
        {
            id = $"{fieldDefinition?.Name}##{id}";
            ImGui.InputDouble($"{fieldDefinition?.Name}##{id}", ref instance);
            //TODO: maybe an option to use float sliders on double fields

            if(fieldDefinition?.RangeMin != null) instance = Math.Max(instance, (double)fieldDefinition.RangeMin);
            if(fieldDefinition?.RangeMax != null) instance = Math.Min(instance, (double)fieldDefinition.RangeMax);
        }
    }
}