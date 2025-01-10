using AutoConfigLib.Auto.Generators;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace AutoConfigLib.Auto.Rendering.Renderers.ComplexTypes.Enumeration
{
    public class DictionaryRenderer<T, K, V> : ComplexRendererBase<T> where T : IDictionary<K, V>
    {
        public IRenderer KeyRenderer { get; private set; }

        public IRenderer ValueRenderer { get; private set; }

        public string AddButtonFailureReason { get; private set; }

        public bool UseCollapseHeaderForValues { get; private set; }

        public override void Initialize()
        {
            base.Initialize();
            KeyRenderer = Renderer.GetOrCreateRenderForType(typeof(K));
            ValueRenderer = Renderer.GetOrCreateRenderForType(typeof(V));

            var addFailureReasonBuilder = new StringBuilder();
            if (!UniqueGenerator.CanGenerateUnique<K>())
            {
                addFailureReasonBuilder.AppendLine($"Cannot initialize dictionary key of type '{typeof(K)}'");
            }
            if (!UniqueGenerator.CanGenerateUnique<V>())
            {
                addFailureReasonBuilder.AppendLine($"Cannot initialize dictionary item of type '{typeof(V)}'");
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

            ImGui.BeginTable($"##{id}-dict", 3, ImGuiTableFlags.BordersOuter | ImGuiTableFlags.NoPadInnerX);

            ImGui.TableSetupColumn($"##{id}-dict-key-col", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn($"##{id}-dict-val-col", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn($"##{id}-dict-del-col", ImGuiTableColumnFlags.WidthFixed);

            for (int row = 0; row < instance.Count; row++)
            {
                var entry = instance.ElementAt(row);
                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(-1);
                var key = KeyRenderer.RenderObject(entry.Key, $"{id}-dict-key-{row}");

                if (!key.Equals(entry.Key))
                {
                    if (instance.ContainsKey((K)key))
                    {
                        key = entry.Key; //Ignore new key if this key already existed
                    }
                    else
                    {
                        instance.Remove(entry.Key);
                        instance.Add((K)key, entry.Value);
                    }
                }

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(-1);
                if (!UseCollapseHeaderForValues || ImGui.CollapsingHeader($"Content##{id}-dict-colapse-{row}"))
                {
                    var value = ValueRenderer.RenderObject(entry.Value, $"{id}-dict-value-{row}");

                    if (!value.Equals(entry.Value)) instance[(K)key] = (V)value;
                }

                ImGui.TableNextColumn();
                if (ImGui.Button($"Remove##{id}-dict-remove-item-{row}")) instance.Remove(entry.Key);
            }

            ImGui.EndTable();

            ImGui.BeginDisabled(AddButtonFailureReason != null);

            if (ImGui.Button($"Add##{id}-dict-add-item", new Vector2(ImGui.GetContentRegionAvail().X, 0)))
            {
                try
                {
                    var uniqueKey = UniqueGenerator.GenerateUnique(instance.Keys, out var uniqueKeyLeft);
                    if (uniqueKeyLeft) instance.Add(uniqueKey, UniqueGenerator.GenerateUnique(Array.Empty<V>(), out _));
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