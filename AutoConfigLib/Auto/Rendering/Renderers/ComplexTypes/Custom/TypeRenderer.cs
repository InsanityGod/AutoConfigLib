using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace AutoConfigLib.Auto.Rendering.Renderers.ComplexTypes.Custom
{
    public class TypeRenderer : ComplexRendererBase<Type>
    {
        public override bool ShouldBeInsideCollapseHeader => false;

        public IRenderer ValueRenderer { get; private set; }

        public override void Initialize()
        {
            base.Initialize();
            ValueRenderer = Renderer.GetOrCreateRenderForType(typeof(string));
        }

        public override Type RenderValue(Type instance, string id, FieldRenderDefinition fieldDefinition = null)
        {
            ValueRenderer.RenderObject(instance.FullName, $"{id}-name", fieldDefinition);
            
            return instance;
        }
    }
}
