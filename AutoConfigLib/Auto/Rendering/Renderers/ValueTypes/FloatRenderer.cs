using ImGuiNET;
using System;

namespace AutoConfigLib.Auto.Rendering.Renderers.ValueTypes
{
    public class FloatRenderer : ValueRendererBase<float>
    {
        public override void RenderValue(ref float instance, string id, FieldRenderDefinition fieldDefinition = null)
        {
            id = $"{fieldDefinition?.Name}##{id}";

            if(fieldDefinition?.UseSlider == true)
            {
                if (fieldDefinition.IsPercentage)
                {
                    instance *= 100;
                    ImGui.SliderFloat(id, ref instance, (float)fieldDefinition.RangeMin * 100, (float)fieldDefinition.RangeMax * 100, fieldDefinition.FormatString);
                    instance /= 100;
                }
                else ImGui.SliderFloat(id, ref instance, (float)fieldDefinition.RangeMin, (float)fieldDefinition.RangeMax, fieldDefinition.FormatString);
            }
            else
            {
                if (fieldDefinition?.IsPercentage == true)
                {
                    instance *= 100;
                    ImGui.InputFloat(id, ref instance, 0, 0, fieldDefinition?.FormatString);
                    instance /= 100;
                }
                else ImGui.InputFloat($"{fieldDefinition?.Name}##{id}", ref instance, 0, 0, fieldDefinition?.FormatString);
            }

            if(fieldDefinition?.RangeMin != null) instance = Math.Max(instance, (float)fieldDefinition.RangeMin);
            if(fieldDefinition?.RangeMax != null) instance = Math.Min(instance, (float)fieldDefinition.RangeMax);
        }
    }
}