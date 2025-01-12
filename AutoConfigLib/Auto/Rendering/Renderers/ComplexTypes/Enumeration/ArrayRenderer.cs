using AutoConfigLib.Auto.Generators;
using ImGuiNET;
using System;
using System.Linq;
using System.Numerics;
using System.Text;
using Vintagestory.API.Util;

namespace AutoConfigLib.Auto.Rendering.Renderers.ComplexTypes.Enumeration
{
    public class ArrayRenderer<V> : ComplexRendererBase<V[]>
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

            if (!UniqueGenerator.CanGenerate<V>())
            {
                addFailureReasonBuilder.AppendLine($"Cannot initialize array item of type '{typeof(V)}'");
            }
            AddButtonFailureReason = addFailureReasonBuilder.ToString();
            if (string.IsNullOrEmpty(AddButtonFailureReason)) AddButtonFailureReason = null;

            UseCollapseHeaderForValues = ValueRenderer.ShouldBeInsideCollapseHeader;
        }

        public override V[] RenderValue(V[] instance, string id, FieldRenderDefinition fieldDefinition = null)
        {
            ImGui.BeginTable($"##{id}-array", 2, ImGuiTableFlags.NoPadInnerX);

            ImGui.TableSetupColumn($"##{id}-array-val-col", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn($"##{id}-array-del-col", ImGuiTableColumnFlags.WidthFixed);

            for (int row = 0; row < instance.Length; row++)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(-1);

                if (!UseCollapseHeaderForValues || ImGui.CollapsingHeader($"Content##{id}-array-colapse-{row}"))
                {
                    if(UseCollapseHeaderForValues) ImGui.Indent();
                    instance[row] = (V)ValueRenderer.RenderObject(instance[row], $"{id}-array-value-{row}");
                    if(UseCollapseHeaderForValues) ImGui.Unindent();
                }

                ImGui.TableNextColumn();

                ImGui.BeginDisabled(DeleteButtonFailureReason != null);

                if (ImGui.Button($"Remove##{id}-array-remove-item-{row}"))
                {
                    try
                    {
                        instance = instance.RemoveAt(row);
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

            if (ImGui.Button($"Add##{id}-array-add-item", new Vector2(ImGui.GetContentRegionAvail().X, 0)))
            {
                try
                {
                    instance = instance.Append(UniqueGenerator.Generate(Array.Empty<V>(), out _));
                }
                catch (Exception ex)
                {
                    AddButtonFailureReason = $"Unexpected Exception: {ex}";
                }
            }

            ImGuiHelper.SetExceptionToolTip(AddButtonFailureReason);
            ImGui.EndDisabled();

            return instance;
        }
    }
}