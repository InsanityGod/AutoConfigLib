using AutoConfigLib.Auto.Generators;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace AutoConfigLib.Auto.Rendering.Renderers.ComplexTypes.Enumeration
{
    public class ReadonlyCollectionRenderer<T, V> : ComplexRendererBase<T> where T : IReadOnlyCollection<V>
    {
        public IRenderer ValueRenderer { get; private set; }

        public bool UseDisabledBlockForValues { get; private set;}
        public bool UseCollapseHeaderForValues { get; private set; }

        public override void Initialize()
        {
            base.Initialize();
            ValueRenderer = Renderer.GetOrCreateRenderForType(typeof(V));
            UseCollapseHeaderForValues = ValueRenderer.ShouldBeInsideCollapseHeader;
            UseDisabledBlockForValues = !ValueRenderer.IsComplex;
        }

        public override T RenderValue(T instance, string id, FieldRenderDefinition fieldDefinition = null)
        {
            ImGui.BeginDisabled(UseDisabledBlockForValues);
            ImGui.BeginTable($"##{id}-collection", 2, ImGuiTableFlags.NoPadInnerX);

            ImGui.TableSetupColumn($"##{id}-collection-val-col", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn($"##{id}-collection-del-col", ImGuiTableColumnFlags.WidthFixed);

            for (int row = 0; row < instance.Count; row++)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(-1);
                var item = instance.ElementAt(row);

                if (!UseCollapseHeaderForValues || ImGui.CollapsingHeader($"Content##{id}-collection-colapse-{row}"))
                {
                    if(UseCollapseHeaderForValues) ImGui.Indent();
                    ValueRenderer.RenderObject(item, $"{id}-collection-value-{row}");
                    if(UseCollapseHeaderForValues) ImGui.Unindent();
                }

                ImGui.TableNextColumn();
            }

            ImGui.EndTable();
            ImGui.EndDisabled();
            return instance;
        }
    }
}