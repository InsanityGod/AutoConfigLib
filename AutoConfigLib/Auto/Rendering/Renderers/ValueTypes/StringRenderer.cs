using ImGuiNET;

namespace AutoConfigLib.Auto.Rendering.Renderers.ValueTypes
{
    public class StringRenderer : ValueRendererBase<string>
    {
        public override void RenderValue(ref string instance, string id, FieldRenderDefinition fieldDefinition = null)
        {
            //TODO attribute support
            instance ??= string.Empty;
            ImGui.InputText($"{fieldDefinition?.Name}##{id}", ref instance, (uint)AutoConfigLibModSystem.Config.DefaultMaxStringLength);
        }
    }
}