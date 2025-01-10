using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoConfigLib.Auto.Rendering.Renderers
{
    public class UnsupportedTypeRenderer<T> : IRenderer
    {
        public object RenderObject(object instance, string id, FieldRenderDefinition fieldDefinition = null)
        {
            if (AutoConfigLibModSystem.Config.ShowPresenceOfUnsupportedTypes)
            {
                if (string.IsNullOrEmpty(fieldDefinition?.Name))
                {
                    ImGui.TextWrapped($"Unsupported type: {typeof(T)}");
                }
                else
                {
                    ImGui.TextWrapped($"Member '{fieldDefinition.Name}' is an unsupported type: {typeof(T)}");
                }
            }
            return instance;
        }
    }
}
