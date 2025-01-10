using ImGuiNET;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using YamlDotNet.Core.Tokens;

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
