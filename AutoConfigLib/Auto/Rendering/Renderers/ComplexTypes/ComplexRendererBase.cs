using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoConfigLib.Auto.Rendering.Renderers.ComplexTypes
{
    public abstract class ComplexRendererBase<T> : IRenderer<T>
    {
        public bool CanBeInitialized { get; private set; }

        public bool ShouldBeInsideCollapseHeader => true;

        public virtual void Initialize()
        {
            try
            {
                Activator.CreateInstance<T>();
                CanBeInitialized = true;
            }
            catch
            {
                CanBeInitialized = false;
            }
        }

        public abstract T RenderValue(T instance, string id, FieldRenderDefinition fieldDefinition = null);

        public T Render(T instance, string id, FieldRenderDefinition fieldDefinition = null)
        {
            if (instance == null)
            {
                if (AutoConfigLibModSystem.Config.AutoInitializeNullFields)
                {
                    if (CanBeInitialized) return Activator.CreateInstance<T>();
                    ImGui.TextDisabled($"type of {fieldDefinition.Name ?? "unknown"} field cannot be initialized ({typeof(T)})");
                    return instance;
                }
                ImGui.BeginDisabled(!CanBeInitialized);
                if (ImGui.Button($"Initialize {fieldDefinition.Name}##{id}-initialize-button"))
                {
                    instance = Activator.CreateInstance<T>();
                }
                ImGuiHelper.SetExceptionToolTip(CanBeInitialized ? null : $"type of {fieldDefinition.Name ?? "unknown"} field cannot be initialized ({typeof(T)})");
                ImGui.EndDisabled();
                return instance;
            }



            return RenderValue(instance, id, fieldDefinition);
        }
    }
}
