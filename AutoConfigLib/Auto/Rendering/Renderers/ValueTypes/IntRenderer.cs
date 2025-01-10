using ImGuiNET;

namespace AutoConfigLib.Auto.Rendering.Renderers.ValueTypes
{
    public class IntRenderer : ValueRendererBase<int>
    {
        public override void RenderValue(ref int instance, string id, FieldRenderDefinition fieldDefinition = null)
        {
            //TODO attribute support
            ImGui.InputInt($"{fieldDefinition?.Name}##{id}", ref instance);
        }
    }
}