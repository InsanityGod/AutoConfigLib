using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoConfigLib.Auto.Rendering.Renderers.ValueTypes
{
    public class DoubleRenderer : ValueRendererBase<double>
    {
        public override void RenderValue(ref double instance, string id, FieldRenderDefinition fieldDefinition = null)
        {
            //TODO range attribute support
            ImGui.InputDouble($"{fieldDefinition?.Name}##{id}", ref instance);
        }
    }
}
