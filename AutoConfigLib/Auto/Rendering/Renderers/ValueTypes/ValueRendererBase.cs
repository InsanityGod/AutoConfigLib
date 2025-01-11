using ImGuiNET;

namespace AutoConfigLib.Auto.Rendering.Renderers.ValueTypes
{
    public abstract class ValueRendererBase<T> : IRenderer<T>
    {
        
        public abstract void RenderValue(ref T instance, string id, FieldRenderDefinition fieldDefinition = null);

        public T Render(T instance, string id, FieldRenderDefinition fieldDefinition = null)
        {
            var original = ImGuiHelper.ResetValueButton(instance, id, fieldDefinition);

            RenderValue(ref instance, id, fieldDefinition);
            if (!ImGui.IsItemDeactivated())
            {
                return original; //Only sends new value when update is complete
            }
            return instance;
        }
    }
}