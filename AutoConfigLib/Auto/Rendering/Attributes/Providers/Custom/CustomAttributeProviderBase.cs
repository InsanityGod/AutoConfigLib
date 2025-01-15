using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoConfigLib.Auto.Rendering.Attributes.Providers.Custom
{
    public abstract class CustomAttributeProviderBase : IAttributeProvider
    {
        protected abstract Dictionary<string, List<Attribute>> Attributes { get; }

        public T GetAttribute<T>(string memberName) where T : Attribute => Attributes.TryGetValue(memberName, out var attributes) ? attributes.OfType<T>().FirstOrDefault() : null;
    }
}
