using ImGuiNET;
using System.Numerics;
using System.Reflection;

namespace AutoConfigLib.Auto.Rendering.Renderers
{
    public class ButtonRenderer : IRenderer<MethodInfo>
    {
        
        public MethodInfo Render(MethodInfo instance, string id, FieldRenderDefinition fieldDefinition = null)
        {
            if (ImGui.Button($"{fieldDefinition?.Name}##{id}-button", new Vector2(ImGui.GetContentRegionAvail().X, 0)))
            {
                try
                {
                    instance.Invoke(null, null);
                }
                catch
                {
                    //Failed while calling method
                }
            }
            ImGuiHelper.TooltipIcon(fieldDefinition?.Description);

            return instance;
        }
    }
}