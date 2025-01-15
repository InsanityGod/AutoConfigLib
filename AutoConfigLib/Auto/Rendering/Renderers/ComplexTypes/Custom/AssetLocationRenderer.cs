using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace AutoConfigLib.Auto.Rendering.Renderers.ComplexTypes.Custom
{
    public class AssetLocationRenderer : ComplexRendererBase<AssetLocation>
    {
        public override bool ShouldBeInsideCollapseHeader => false;

        public IRenderer ValueRenderer { get; private set; }

        public override void Initialize()
        {
            base.Initialize();
            ValueRenderer = Renderer.GetOrCreateRenderForType(typeof(string));
        }

        public override AssetLocation RenderValue(AssetLocation instance, string id, FieldRenderDefinition fieldDefinition = null)
        {
            ImGui.BeginGroup();
            instance.Domain = (string)ValueRenderer.RenderObject(instance.Domain, $"{id}-domain");
            ImGui.SameLine();
            ImGui.Text(":");
            ImGui.SameLine();
            instance.Path = (string)ValueRenderer.RenderObject(instance.Path, $"{id}-path", fieldDefinition);
            ImGui.EndGroup();
            return instance;
            //TODO maybe use IsValid to show the user wether it's correct or not
        }
    }
}
