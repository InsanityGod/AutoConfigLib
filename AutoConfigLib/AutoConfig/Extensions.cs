using System;
using System.Reflection;

namespace AutoConfigLib.AutoConfig
{
    public static class Extensions
    {
        public static T GetValue<T>(this MemberInfo memberInfo, object instance)
        {
            if (memberInfo is PropertyInfo property)
            {
                return (T)property.GetValue(instance);
            }

            if (memberInfo is FieldInfo field)
            {
                return (T)field.GetValue(instance);
            }

            return default;
        }

        public static void SetValue<T>(this MemberInfo memberInfo, object instance, T value)
        {
            if (memberInfo is PropertyInfo property)
            {
                property.SetValue(instance, value);
            }

            if (memberInfo is FieldInfo field)
            {
                field.SetValue(instance, value);
            }
        }
    }
}