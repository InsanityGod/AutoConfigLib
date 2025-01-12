using ImGuiNET;
using System;
using System.ComponentModel.DataAnnotations;

namespace AutoConfigLib.Auto.Rendering.Renderers.ValueTypes
{
    public class IntRenderer : ValueRendererBase<int>
    {
        //TODO maybe format support for stuff other then float percentages
        public override void RenderValue(ref int instance, string id, FieldRenderDefinition fieldDefinition = null)
        {
            id = $"{fieldDefinition?.Name}##{id}";
 
            if(fieldDefinition?.UseSlider == true)
            {
                ImGui.SliderInt(id, ref instance, (int)fieldDefinition.RangeMin, (int)fieldDefinition.RangeMax);
            }
            else
            {
                ImGui.InputInt($"{fieldDefinition?.Name}##{id}", ref instance);
            }

            if(fieldDefinition?.RangeMin != null) instance = Math.Max(instance, (int)fieldDefinition.RangeMin);
            if(fieldDefinition?.RangeMax != null) instance = Math.Min(instance, (int)fieldDefinition.RangeMax);
        }
    }
}