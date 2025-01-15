using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AutoConfigLib.Auto.Rendering.Attributes.Providers.Custom
{
    public class ReadOnlyComplexAttributeProvider<C> : IAttributeProvider
    {
        public virtual T GetAttribute<T>(string memberName) where T : Attribute
        {
            if(typeof(T) == typeof(ReadOnlyAttribute))
            {
                var members = typeof(C).GetMember(memberName);
                if(members.Length == 1)
                {
                    if (members[0] is FieldInfo field && Renderer.GetOrCreateRenderForType(field.FieldType).IsComplex) return null;
                    if (members[0] is PropertyInfo property && Renderer.GetOrCreateRenderForType(property.PropertyType).IsComplex) return null;
                }

                return (T)(object)AttributeHelper.ReadOnly;
            }
            return null;
        }
    }
}
