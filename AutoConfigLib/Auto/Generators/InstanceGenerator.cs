using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoConfigLib.Auto.Generators
{
    public static class InstanceGenerator
    {
        public static T Generate<T>()
        {
            if(typeof(T) == typeof(string)) return (T)(object)string.Empty;
            if(typeof(T).IsArray) return (T)(object)Array.CreateInstance(typeof(T).GetElementType(), 0);

            return Activator.CreateInstance<T>();
        }

        public static bool CanGenerate<T>()
        {
            try
            {
                Generate<T>();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
