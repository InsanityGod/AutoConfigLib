using ImGuiNET;

namespace AutoConfigLib.Auto.Rendering.Renderers.ValueTypes
{
    public class FloatRenderer : ValueRendererBase<float>
    {
        public override void RenderValue(ref float instance, string id, FieldRenderDefinition fieldDefinition = null)
        {
            //TODO range attribute support
            ImGui.InputFloat($"{fieldDefinition?.Name}##{id}", ref instance);
        }
    }
}