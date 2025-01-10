using ImGuiNET;

namespace AutoConfigLib.Auto.Rendering.Renderers.ValueTypes
{
    public class BoolRenderer : IRenderer<bool>
    {
        

        public bool Render(bool instance, string id, FieldRenderDefinition fieldDefinition = null)
        {
            ImGui.Checkbox($"{fieldDefinition?.Name}##{id}", ref instance);
            return instance;
        }
    }
}