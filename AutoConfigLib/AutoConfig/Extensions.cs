using System;
using System.Collections.Generic;
using System.Reflection;

namespace AutoConfigLib.AutoConfig
{
    public static class Extensions
    {
        public static T GetValue<T>(this MemberInfo memberInfo, object instance)
        {
            try
            {
                if (memberInfo is PropertyInfo property)
                {
                    return (T)property.GetValue(instance);
                }

                if (memberInfo is FieldInfo field)
                {
                    return (T)field.GetValue(instance);
                }
            }
            catch
            {
                //Something is going wrong here
            }

            return default;
        }

        public static void SetValue<T>(this MemberInfo memberInfo, object instance, T value)
        {
            try
            {
                if (memberInfo is PropertyInfo property && property.CanWrite)
                {
                    property.SetValue(instance, value);
                }

                if (memberInfo is FieldInfo field && !field.IsLiteral)
                {
                    field.SetValue(instance, value);
                }
            }
            catch
            {
                //Something is going wrong here
            }
        }

        public static bool IsICollection(this Type type)
        {
            // Check if the type implements any generic interface of ISet<T>
            foreach (var interfaceType in type.GetInterfaces())
            {
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(ICollection<>))
                {
                    return true;
                }
            }
            return false;
        }
    }
}