using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoConfigLib.Auto.Rendering.Renderers.ComplexTypes.Custom
{
    public class ConfigDefinitionRenderer : IRenderer<ConfigDefinition>
    {
        private IRenderer<ConfigDefinition> _classRenderer;

        public void Initialize()
        {
            _classRenderer = new ClassRenderer<ConfigDefinition>();
            _classRenderer.Initialize();
        }

        public ConfigDefinition Render(ConfigDefinition instance, string id, FieldRenderDefinition fieldDefinition = null)
        {
            if (AutoConfigLibModSystem.Config.ShowConfigDefinition)
            {
                _classRenderer.RenderObject(instance, $"{id}-configdefinition");

                ImGuiHelper.IndentedSeparator();
            }
            if(instance.PrimaryValue == null)
            {
                ImGui.Text("Could not intercept config because it was only just created. Please reload world first.");
            }
            else Renderer.GetOrCreateRenderForType(instance.Type).RenderObject(instance.PrimaryValue, $"{id}-config");
            return instance;
        }
    }
}
