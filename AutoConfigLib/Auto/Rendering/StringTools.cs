using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AutoConfigLib.Auto.Rendering
{
    public static class StringTools
    {
        public static string CleanWhiteSpaces(string input) => Regex.Replace(input, @"[^\S\r\n]+", " ").Replace("\n ", "\n").Trim();

        public static string Capitalize(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            StringBuilder result = new(input.Length);

            // Track whether the next character should be capitalized
            bool capitalizeNext = true;

            foreach (char c in input)
            {
                if (char.IsWhiteSpace(c))
                {
                    capitalizeNext = true;
                    result.Append(c);
                    continue;
                }

                if (capitalizeNext)
                {
                    result.Append(char.ToUpper(c));
                    capitalizeNext = false;
                }
                else result.Append(c);
            }

            return result.ToString();
        }
    }
}
