using AutoConfigLib.Auto.Generators;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace AutoConfigLib.Auto.Rendering.Renderers.ComplexTypes.Enumeration
{
    public class CollectionRenderer<T, V> : ComplexRendererBase<T> where T : ICollection<V>
    {
        public IRenderer ValueRenderer { get; private set; }

        public string AddButtonFailureReason { get; private set; }
        public string DeleteButtonFailureReason { get; private set; }
        public bool UseCollapseHeaderForValues { get; private set; }

        public bool AreItemsUnique { get; private set; }

        public override void Initialize()
        {
            base.Initialize();
            ValueRenderer = Renderer.GetOrCreateRenderForType(typeof(V));

            var addFailureReasonBuilder = new StringBuilder();

            if (!UniqueGenerator.CanGenerate<V>())
            {
                addFailureReasonBuilder.AppendLine($"Cannot initialize collection item of type '{typeof(V)}'");
            }
            AddButtonFailureReason = addFailureReasonBuilder.ToString();
            if (string.IsNullOrEmpty(AddButtonFailureReason)) AddButtonFailureReason = null;

            UseCollapseHeaderForValues = ValueRenderer.ShouldBeInsideCollapseHeader;

            AreItemsUnique = typeof(ISet<V>).IsAssignableFrom(typeof(T));
        }

        public override T RenderValue(T instance, string id, FieldRenderDefinition fieldDefinition = null)
        {
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
                    var newItem = (V)ValueRenderer.RenderObject(item, $"{id}-collection-value-{row}");

                    if (!newItem.Equals(item))
                    {
                        instance.Remove(item);
                        instance.Add(newItem);
                    }
                    if(UseCollapseHeaderForValues) ImGui.Unindent();
                }

                ImGui.TableNextColumn();

                ImGui.BeginDisabled(DeleteButtonFailureReason != null);

                if (ImGui.Button($"Remove##{id}-collection-remove-item-{row}"))
                {
                    try
                    {
                        instance.Remove(item);
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

            if (ImGui.Button($"Add##{id}-collection-add-item", new Vector2(ImGui.GetContentRegionAvail().X, 0)))
            {
                try
                {
                    instance.Add(UniqueGenerator.Generate<V>(AreItemsUnique ? instance : Array.Empty<V>(), out _));
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