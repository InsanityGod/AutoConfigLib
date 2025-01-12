using ImGuiNET;

namespace AutoConfigLib.Auto.Rendering.Renderers.ValueTypes
{
    public class StringRenderer : ValueRendererBase<string>
    {
        public override void RenderValue(ref string instance, string id, FieldRenderDefinition fieldDefinition = null)
        {
            instance ??= string.Empty;
            ImGui.InputText($"{fieldDefinition?.Name}##{id}", ref instance, fieldDefinition?.MaxStringLength ?? (uint)AutoConfigLibModSystem.Config.DefaultMaxStringLength);
        }
    }
}