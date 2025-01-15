using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoConfigLib.Auto.Rendering.Attributes.Providers
{
    public interface IAttributeProvider
    {
        public T GetAttribute<T>(string memberName) where T : Attribute;
    }
}
