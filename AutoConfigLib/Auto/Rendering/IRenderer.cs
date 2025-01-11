using ImGuiNET;
using System;

namespace AutoConfigLib.Auto.Rendering
{
    public interface IRenderer
    {
        /// <summary>
        /// Wether readonly should be ignored when deciding to disable object
        /// (This is usually only true for complex types as they don't need to be written to for changes)
        /// </summary>
        public bool IgnoreReadOnly => false;

        public bool IsComplex => false;

        public bool ShouldBeInsideCollapseHeader => false;

        public void Initialize()
        {
            //Optional
        }

        public object RenderObject(object instance, string id, FieldRenderDefinition fieldDefinition = null);
    }

    public interface IRenderer<T> : IRenderer
    {
        object IRenderer.RenderObject(object instance, string id, FieldRenderDefinition fieldDefinition)
        {
            try
            {
                return Render((T)instance, id, fieldDefinition);
            }
            catch (Exception e)
            {
                ImGui.TextWrapped($"Unexpected exception: {e}");
                return (T)instance;
            }
        }

        public T Render(T instance, string id, FieldRenderDefinition fieldDefinition = null);
    }
}