using ConfigLib;
using ImGuiNET;
using InsanityLib.Util;
using System;
using System.Linq;
using Vintagestory.API.Common;

namespace AutoConfigLib;

public static class WorldConfigEditor
{
    public static void LoadWorldConfig(ICoreAPI api)
    {
        var configLib = api.ModLoader.GetModSystem<ConfigLibModSystem>();
        var name = "World Config (auto)";

        if (AutoConfigLibModSystem.CoreServerAPI == null) name += " (readonly)";
        configLib.RegisterCustomConfig(name, EditWorldConfig);
    }

    public static void EditWorldConfig(string id, ControlButtons buttons)
    {
        var api = (ICoreAPI)AutoConfigLibModSystem.CoreServerAPI ?? AutoConfigLibModSystem.CoreClientAPI;

        ImGui.BeginDisabled(AutoConfigLibModSystem.CoreServerAPI == null);
        ImGui.Text("Remember to reload the world after making changes to this");
        foreach (var mod in api.ModLoader.Mods.Where(mod => mod.WorldConfig != null))
        {
            if (mod.WorldConfig.WorldConfigAttributes == null || mod.WorldConfig.WorldConfigAttributes.Length == 0) continue;
            if (!ImGui.CollapsingHeader($"{mod.Info.Name}##{id}-{mod.Info.ModID}-collapse")) continue;
            ImGui.Indent();
            var attributesByCategory = mod.WorldConfig.WorldConfigAttributes
                .GroupBy(config => config.Category);

            foreach (var attributeGroup in attributesByCategory)
            {
                if (ImGui.CollapsingHeader($"{attributeGroup.Key}##{id}-{mod.Info.ModID}-{attributeGroup.Key}-collapse"))
                {
                    ImGui.Indent();

                    foreach (var attribute in attributeGroup)
                    {
                        AddWorldConfigAttribute(api, attribute, $"{id}-{mod.Info.ModID}-{attributeGroup.Key}-{attribute.Code}");
                    }

                    ImGui.Unindent();
                }
            }
            ImGui.Unindent();
        }

        ImGui.EndDisabled();
    }

    public static void AddWorldConfigAttribute(ICoreAPI api, WorldConfigurationAttribute attribute, string id)
    {
        if (attribute.OnlyDuringWorldCreate) ImGui.BeginDisabled();
        var config = api.World.Config;
        switch (attribute.DataType)
        {
            case EnumDataType.Bool:
                var boolValue = config.GetAsBool(attribute.Code);
                var oldBoolValue = boolValue;
                ImGui.Checkbox($"{attribute.Code.ToHumanReadable()}##{id}", ref boolValue);
                if (oldBoolValue != boolValue)
                {
                    config.SetBool(attribute.Code, boolValue);
                }
                break;

            case EnumDataType.IntInput:
                var intValue = config.GetInt(attribute.Code);
                var oldIntValue = intValue;
                ImGui.InputInt($"{attribute.Code.ToHumanReadable()}##{id}", ref intValue);
                if (oldIntValue != intValue)
                {
                    config.SetInt(attribute.Code, intValue);
                }
                break;

            case EnumDataType.DoubleInput:
                var doubleValue = config.GetInt(attribute.Code);
                var oldDoubleValue = doubleValue;
                ImGui.InputInt($"{attribute.Code.ToHumanReadable()}##{id}", ref doubleValue);
                if (oldDoubleValue != doubleValue)
                {
                    config.SetDouble(attribute.Code, doubleValue);
                }
                break;

            case EnumDataType.IntRange:
                var intRangeValue = config.GetInt(attribute.Code);
                var oldIntRangeValue = intRangeValue;
                ImGui.DragInt($"{attribute.Code.ToHumanReadable()}##{id}", ref intRangeValue, Math.Min((int)attribute.Step, 1), (int)attribute.Min, (int)attribute.Max);
                if (oldIntRangeValue != intRangeValue)
                {
                    config.SetInt(attribute.Code, intRangeValue);
                }
                break;

            case EnumDataType.String:
                var stringValue = config.GetAsString(attribute.Code);
                var oldStringValue = stringValue;
                stringValue ??= string.Empty;
                ImGui.InputText($"{attribute.Code.ToHumanReadable()}##{id}", ref stringValue, 128);
                if (oldStringValue != null && oldStringValue != stringValue || oldStringValue == null && !string.IsNullOrEmpty(stringValue))
                {
                    config.SetString(attribute.Code, stringValue);
                }
                break;

            case EnumDataType.DropDown:
                var dropDownValue = config.GetAsString(attribute.Code);
                var currentIndex = Array.IndexOf(attribute.Values, dropDownValue);
                var oldIndex = currentIndex;
                ImGui.Combo($"{attribute.Code.ToHumanReadable()}##{id}", ref currentIndex, attribute.Names ?? attribute.Values, attribute.Values.Length);
                if (oldIndex != currentIndex)
                {
                    config.SetString(attribute.Code, attribute.Values[currentIndex]);
                }
                break;
        }
        if (attribute.OnlyDuringWorldCreate) ImGui.EndDisabled();
    }
}