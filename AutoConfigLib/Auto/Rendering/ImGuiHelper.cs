using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.MathTools;
using static System.Net.Mime.MediaTypeNames;

namespace AutoConfigLib.Auto.Rendering
{
    public static class ImGuiHelper
    {
        public static void IndentedSeparator() {
            float indentX = ImGui.GetCursorPosX();
            float separatorWidth = ImGui.GetContentRegionAvail().X;
        
            var cursorPos = ImGui.GetCursorScreenPos();
            ImGui.GetWindowDrawList().AddLine(
                new Vector2(cursorPos.X + indentX, cursorPos.Y),
                new Vector2(cursorPos.X + indentX + separatorWidth, cursorPos.Y),
                ImGui.GetColorU32(ImGuiCol.Separator),
                1.0f
            );
        
            ImGui.Dummy(new Vector2(0, ImGui.GetStyle().ItemSpacing.Y)); // Add vertical spacing
        }

        public static void IndentedSeparatorText(string text) 
        {
            // Get style and dimensions
            var style = ImGui.GetStyle();
            float contentWidth = ImGui.GetContentRegionAvail().X;
            float textWidth = ImGui.CalcTextSize(text).X;
        
            // Calculate space for separators on both sides
            float separatorWidth = (contentWidth - textWidth) / 2.0f;
            if (separatorWidth < 0.0f) separatorWidth = 0.0f; // Handle narrow widths gracefully
        
            // Get cursor position for drawing
            var cursorPos = ImGui.GetCursorScreenPos();
            float lineHeight = ImGui.GetTextLineHeight();
        
            // Draw left separator
            ImGui.GetWindowDrawList().AddLine(
                new Vector2(cursorPos.X, cursorPos.Y + lineHeight / 2.0f), // Start position
                new Vector2(cursorPos.X + separatorWidth - style.ItemSpacing.X, cursorPos.Y + lineHeight / 2.0f), // End position
                ImGui.GetColorU32(ImGuiCol.Separator),
                1.0f // Line thickness
            );
        
            // Render the text
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + separatorWidth); // Move cursor for the text
            ImGui.TextUnformatted(text);
        
            // Draw right separator
            var textEndPos = ImGui.GetCursorScreenPos(); // Position after text
            ImGui.GetWindowDrawList().AddLine(
                new Vector2(textEndPos.X + style.ItemSpacing.X, cursorPos.Y + lineHeight / 2.0f), // Start position
                new Vector2(textEndPos.X + separatorWidth - style.ItemSpacing.X, cursorPos.Y + lineHeight / 2.0f), // End position
                ImGui.GetColorU32(ImGuiCol.Separator),
                1.0f // Line thickness
            );
        
            // Add spacing below the separator text
            ImGui.Dummy(new Vector2(0, style.ItemSpacing.Y));
        }

        public static void TooltipIcon(string comment)
		{
            if(string.IsNullOrEmpty(comment)) return;

			ImGui.SameLine();
			ImGui.TextDisabled("(?)");
			Tooltip(comment);
		}

        public static void Tooltip(string comment)
        {
            if(string.IsNullOrEmpty(comment)) return;
            if (ImGui.BeginItemTooltip())
			{
				ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35f);
				ImGui.TextUnformatted(comment);
				ImGui.PopTextWrapPos();
				ImGui.EndTooltip();
			}
        }

        public static unsafe void SetExceptionToolTip(string comment)
        {
            if(string.IsNullOrEmpty(comment) || !ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) return;
            var originalColorPtr = ImGui.GetStyleColorVec4(ImGuiCol.PopupBg);

            if (originalColorPtr == null) return; // Safety check in case the pointer is null

            var originalColor = *originalColorPtr;
            ImGui.PushStyleColor(ImGuiCol.PopupBg, new Vector4(originalColor.X, originalColor.Y, originalColor.Z, 255));
            ImGui.SetTooltip(comment);
            ImGui.PopStyleColor();
            
        }
    }
}
