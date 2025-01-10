using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoConfigLib.Auto.Generators
{
    public static class UniqueGenerator
    {
        public static T GenerateUnique<T>(IEnumerable<T> existing, out bool success)
        {
            success = true;
            if (typeof(T) != typeof(string))
            {
                if (!typeof(T).IsValueType) return Activator.CreateInstance<T>();

                T result = default;
                if (!existing.Contains(result)) return result;
            }

            IEnumerable<object> possibilities = null;
            if (typeof(T) == typeof(bool))
            {
                possibilities = new object[] { true, false };
            }
            else if (typeof(T).BaseType == typeof(Enum))
            {
                possibilities = ((T[])Enum.GetValues(typeof(T))).Select(x => (object)x);
            }
            else if (typeof(T).IsNumber())
            {
                var num = 0;
                while (existing.Contains((T)(object)num)) num++;
                return (T)(object)num;
            }
            else if (typeof(T) == typeof(string))
            {
                var baseStr = "str-";

                var num = 0;
                while (existing.Contains((T)(object)$"{baseStr}{num}")) num++;
                return (T)(object)$"{baseStr}{num}";
            }

            if (possibilities != null)
            {
                foreach (var possibility in possibilities)
                {
                    if (!existing.Contains((T)possibility))
                    {
                        return (T)possibility;
                    }
                }
                success = false;
                return default;
            }

            throw new InvalidOperationException($"Cannot create unique instance of type '{typeof(T)}'");
        }
        
        public static bool CanGenerateUnique<T>()
        {
            if (typeof(T).IsInterface || typeof(T).IsAbstract) return false;
            if (typeof(T) == typeof(string)) return true;
            if (typeof(T) == typeof(bool)) return true;
            if (typeof(T).IsNumber()) return true;
            if (typeof(T).BaseType == typeof(Enum)) return true;

            try
            {
                Activator.CreateInstance<T>();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}