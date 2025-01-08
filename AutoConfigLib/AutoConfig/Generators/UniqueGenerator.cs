using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoConfigLib.AutoConfig.Generators
{
    public static class UniqueGenerator
    {
        public static T GenerateUnique<T>(IEnumerable<T> existing)
        {
            if(typeof(T) != typeof(string))
            {
                if (!typeof(T).IsValueType) return Activator.CreateInstance<T>();

                T result = default;
                if(!existing.Contains(result)) return result;
            }

            IEnumerable<object> possibilities = null;
            if (typeof(T) == typeof(bool))
            {
                possibilities = new object[] { true, false };
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
            else if(typeof(T).BaseType == typeof(Enum))
            {
                possibilities = (object[])Enum.GetValues(typeof(T));
            }

            if(possibilities != null)
            {
                foreach(var possibility in possibilities)
                {
                    if (!existing.Contains((T)possibility))
                    {
                        return (T)possibility;
                    }
                }
            }

            throw new InvalidOperationException($"Cannot create unique instance of type '{typeof(T)}'");
        }
    }
}
