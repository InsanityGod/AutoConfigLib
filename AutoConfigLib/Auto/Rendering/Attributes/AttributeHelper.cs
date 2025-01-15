using AutoConfigLib.Auto.Rendering.Attributes.Providers;
using AutoConfigLib.Auto.Rendering.Attributes.Providers.Custom;
using AutoConfigLib.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.Common;

namespace AutoConfigLib.Auto.Rendering.Attributes
{
    public static class AttributeHelper
    {
        public static Dictionary<Type, IAttributeProvider> CustomProviders { get; private set; } = new Dictionary<Type, IAttributeProvider>();

        public static void RegisterDefaultCustomProviders()
        {
            CustomProviders[typeof(ModInfo)] = new ReadOnlyComplexAttributeProvider<ModInfo>();
            CustomProviders[typeof(ModContainer)] = new ReadOnlyComplexAttributeProvider<ModContainer>();
            CustomProviders[typeof(Mod)] = new ModAttributeProvider();
        }

        public static Type GetFirstGenericInterface(this Type typeToCheck, Type genericInterfaceType)
        {
            if(typeToCheck.IsGenericType && typeToCheck.GetGenericTypeDefinition() == genericInterfaceType) return typeToCheck;

            return Array.Find(
                typeToCheck.GetInterfaces(),
                interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == genericInterfaceType
            );
        }

        public static T GetAttribute<T>(this Type type) where T : Attribute
        {
            return type.GetCustomAttribute<T>();
        }

        public static T GetAttribute<T>(this MemberInfo member) where T : Attribute
        {
            var custom = CustomProviders.TryGetValue(member.DeclaringType, out var provider) ? provider.GetAttribute<T>(member.Name) : null;
            if(custom != null) return custom;

            return member.GetCustomAttribute<T>();
        }

        #region StaticAttributes

        public static ReadOnlyAttribute ReadOnly {  get; private set; } = new ReadOnlyAttribute(true);
        public static BrowsableAttribute Hidden {  get; private set; } = new BrowsableAttribute(false);

        #endregion StaticAttributes
    }
}
