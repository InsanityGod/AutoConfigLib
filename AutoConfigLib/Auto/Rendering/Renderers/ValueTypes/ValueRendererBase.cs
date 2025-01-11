using ImGuiNET;

namespace AutoConfigLib.Auto.Rendering.Renderers.ValueTypes
{
    public abstract class ValueRendererBase<T> : IRenderer<T>
    {
        public virtual bool OnlyUpdateOnItemDeactivation => true;

        public abstract void RenderValue(ref T instance, string id, FieldRenderDefinition fieldDefinition = null);

        public T Render(T instance, string id, FieldRenderDefinition fieldDefinition = null)
        {
            var original = fieldDefinition?.IsNullableValueType == true ? instance : ImGuiHelper.ResetValueButton(instance, id, fieldDefinition);

            RenderValue(ref instance, id, fieldDefinition);
            if (OnlyUpdateOnItemDeactivation && !ImGui.IsItemDeactivated())
            {
                return original; //Only sends new value when update is complete
            }
            return instance;
        }
    }
}