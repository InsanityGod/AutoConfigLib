using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
