using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoConfigLib.Auto.Rendering.Renderers.ValueTypes
{
    public class NullableValueRenderer<T> : IRenderer<T>
    {
        public IRenderer NotNullableRenderer { get; private set; }

        public object DefaultValue { get; private set; }
        public void Initialize()
        {
            var notNullableType = Nullable.GetUnderlyingType(typeof(T));

            NotNullableRenderer = Renderer.GetOrCreateRenderForType(notNullableType);
            DefaultValue = Activator.CreateInstance(notNullableType);
        }

        public T Render(T instance, string id, FieldRenderDefinition fieldDefinition = null)
        {
            bool isNull = instance == null;
            if(isNull)
            {
                ImGui.BeginDisabled(true);
                NotNullableRenderer.RenderObject(DefaultValue, id, fieldDefinition);
                ImGui.EndDisabled();
            }
            else instance = (T)NotNullableRenderer.RenderObject(instance, id, fieldDefinition);

            ImGui.SameLine();
            if (ImGui.Checkbox($"Is Null##{id}-nullable-checkbox", ref isNull)) return isNull ? (T)(object)null : (T)DefaultValue;
            return instance;
        }
    }
}
