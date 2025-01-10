using AutoConfigLib.Auto.Generators;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace AutoConfigLib.Auto.Rendering.Renderers.ComplexTypes.Enumeration
{
    public class ListRenderer<T, V> : ComplexRendererBase<T> where T : IList<V>
    {
        public IRenderer ValueRenderer { get; private set; }

        public bool UseCollapseHeaderForValues { get; private set; }

        public string AddButtonFailureReason { get; private set; }
        public string DeleteButtonFailureReason { get; private set; }

        public override void Initialize()
        {
            base.Initialize();
            ValueRenderer = Renderer.GetOrCreateRenderForType(typeof(V));

            var addFailureReasonBuilder = new StringBuilder();

            if (!UniqueGenerator.CanGenerateUnique<V>())
            {
                addFailureReasonBuilder.AppendLine($"Cannot initialize list item of type '{typeof(V)}'");
            }
            AddButtonFailureReason = addFailureReasonBuilder.ToString();
            if (string.IsNullOrEmpty(AddButtonFailureReason)) AddButtonFailureReason = null;

            UseCollapseHeaderForValues = ValueRenderer.ShouldBeInsideCollapseHeader;
        }

        public override T RenderValue(T instance, string id, FieldRenderDefinition fieldDefinition = null)
        {
            if (fieldDefinition != null)
            {
                if (!ImGui.CollapsingHeader($"{fieldDefinition.Name}##{id}-colapse")) return instance;
                ImGui.Indent();
            }

            ImGui.BeginTable($"##{id}-list", 2, ImGuiTableFlags.BordersOuter | ImGuiTableFlags.NoPadInnerX);

            ImGui.TableSetupColumn($"##{id}-list-val-col", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn($"##{id}-list-del-col", ImGuiTableColumnFlags.WidthFixed);

            for (int row = 0; row < instance.Count; row++)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(-1);

                if (!UseCollapseHeaderForValues || ImGui.CollapsingHeader($"Content##{id}-list-colapse-{row}"))
                {
                    instance[row] = (V)ValueRenderer.RenderObject(instance[row], $"{id}-list-value-{row}");
                }

                ImGui.TableNextColumn();

                ImGui.BeginDisabled(DeleteButtonFailureReason != null);

                if (ImGui.Button($"Remove##{id}-list-remove-item-{row}"))
                {
                    try
                    {
                        instance.RemoveAt(row);
                    }
                    catch (Exception ex)
                    {
                        DeleteButtonFailureReason = $"Unexpected Exception: {ex}";
                    }
                }

                ImGuiHelper.SetExceptionToolTip(DeleteButtonFailureReason);
                ImGui.EndDisabled();
            }

            ImGui.EndTable();

            ImGui.BeginDisabled(AddButtonFailureReason != null);

            if (ImGui.Button($"Add##{id}-list-add-item", new Vector2(ImGui.GetContentRegionAvail().X, 0)))
            {
                try
                {
                    instance.Add(UniqueGenerator.GenerateUnique(Array.Empty<V>(), out _));
                }
                catch (Exception ex)
                {
                    AddButtonFailureReason = $"Unexpected Exception: {ex}";
                }
            }

            ImGuiHelper.SetExceptionToolTip(AddButtonFailureReason);
            ImGui.EndDisabled();

            if (fieldDefinition != null) ImGui.Unindent();
            return instance;
        }
    }
}