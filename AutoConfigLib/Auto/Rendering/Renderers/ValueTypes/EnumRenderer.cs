using ImGuiNET;
using System;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Util;

namespace AutoConfigLib.Auto.Rendering.Renderers.ValueTypes
{
    public class EnumRenderer<T> : IRenderer<T> where T : struct, Enum
    {
        
        public bool IsEnumFlag { get; private set; }

        public T[] ValidValues { get; private set; }

        public string[] ValidStrValues { get; private set; }

        public void Initialize()
        {
            IsEnumFlag = typeof(T).GetCustomAttribute<FlagsAttribute>() != null;

            ValidValues = Enum.GetValues<T>();
            ValidStrValues = ValidValues.Select(val => val.ToString()).ToArray();
        }

        public T Render(T instance, string id, FieldRenderDefinition fieldDefinition = null)
        {
            //TODO atribute support

            if (!IsEnumFlag)
            {
                var currentIndex = ValidValues.IndexOf(instance);
                ImGui.Combo($"{fieldDefinition?.Name}##{id}", ref currentIndex, ValidStrValues, ValidStrValues.Length);
                return ValidValues[currentIndex];
            }
            else
            {
                int intValue = Convert.ToInt32(instance);
                bool modified = false;

                if (ImGui.BeginCombo($"{fieldDefinition?.Name}##{id}", intValue == 0 ? "None" : instance.ToString()))
                {
                    for (int i = 0; i < ValidValues.Length; i++)
                    {
                        var flag = ValidValues[i];
                        if (Convert.ToInt32(flag) == 0)
                            continue;

                        // Check if the current flag is set
                        bool isSelected = (intValue & Convert.ToInt32(flag)) != 0;

                        // Draw a selectable checkbox for this flag
                        if (ImGui.Selectable(ValidStrValues[i], isSelected))
                        {
                            // Toggle the flag
                            if (isSelected)
                                intValue &= ~Convert.ToInt32(flag); // Remove the flag
                            else
                                intValue |= Convert.ToInt32(flag); // Add the flag

                            modified = true;
                        }
                    }
                    ImGui.EndCombo();
                }

                // Update the enum value if it was modified
                if (modified) instance = (T)Enum.ToObject(typeof(T), intValue);
            }

            return instance;
        }
    }
}