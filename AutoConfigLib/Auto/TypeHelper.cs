using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AutoConfigLib.Auto
{
    public static class TypeHelper
    {
        public static bool IsBackingField(MemberInfo field)
        {
            // Backing field name pattern: "<PropertyName>k__BackingField"
            return field.Name.StartsWith('<') && field.Name.Contains("k__BackingField");
        }
    }
}
