using ImGuiNET;

namespace AutoConfigLib.Auto.Rendering.Renderers.ValueTypes
{
    public class BoolRenderer : ValueRendererBase<bool>
    {
        public override void RenderValue(ref bool instance, string id, FieldRenderDefinition fieldDefinition = null)
        {
            ImGui.Checkbox($"{fieldDefinition?.Name}##{id}", ref instance);
        }
    }
}