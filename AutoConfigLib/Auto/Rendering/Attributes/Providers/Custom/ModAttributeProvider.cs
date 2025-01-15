using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace AutoConfigLib.Auto.Rendering.Attributes.Providers.Custom
{
    public class ModAttributeProvider : ReadOnlyComplexAttributeProvider<Mod>
    {
        protected Dictionary<string, List<Attribute>> Attributes { get; } = new Dictionary<string, List<Attribute>>()
        {
            {
                nameof(Mod.Icon),
                new()
                {
                    AttributeHelper.Hidden
                }
            },
            {
                nameof(Mod.Systems),
                new()
                {
                    AttributeHelper.Hidden
                }
            },
        };

        public override T GetAttribute<T>(string memberName) => base.GetAttribute<T>(memberName) ?? (Attributes.TryGetValue(memberName, out var attributes) ? attributes.OfType<T>().FirstOrDefault() : null);
    }
}
