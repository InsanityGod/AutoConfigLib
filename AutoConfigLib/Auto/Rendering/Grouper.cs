using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoConfigLib.Auto.Rendering
{
    public static class Grouper
    {
        private static HashSet<string> TermBlacklist = new(StringComparer.OrdinalIgnoreCase)
        {
            "yield",
            "min",
            "max",
            "get",
            "set",
            "in",
            "ms",
            "days",
            "hours",
            "at"
        };

        //TODO: see if we can implement multi term matching
        public static Dictionary<string, List<FieldRenderDefinition>> CategorizeByName(List<FieldRenderDefinition> fieldRenderDefinitions)
        {
            var termFrequency = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var definition in fieldRenderDefinitions)
            {
                if(!definition.IsVisible) continue;
                var terms = definition.Name.Split(' ');

                foreach (var term in terms)
                {
                    if(TermBlacklist.Contains(term)) continue;
                    if (termFrequency.ContainsKey(term))
                    {
                        termFrequency[term]++;
                    }
                    else termFrequency[term] = 1;
                }
            }

            // Identify frequent terms to use as categories
            var frequentTerms = termFrequency
                .Where(kv => kv.Value > 1) // Only consider terms that appear more than once
                .OrderByDescending(kv => kv.Value)
                .Select(kv => StringTools.Capitalize(kv.Key))
                .ToList();

            // Create categories dynamically
            var categories = new Dictionary<string, List<FieldRenderDefinition>>();

            foreach (var definition in fieldRenderDefinitions)
            {
                var terms = definition.Name.Split(' ');
                var matchingTerm = definition.IsVisible ? 
                    frequentTerms.FirstOrDefault(frequentTerm => Array.Exists(terms, term => string.Equals(term, frequentTerm, StringComparison.OrdinalIgnoreCase))):
                    string.Empty;

                definition.Category = matchingTerm ?? string.Empty;

                if (categories.TryGetValue(definition.Category, out List<FieldRenderDefinition> value))
                {
                    value.Add(definition);
                }
                else categories[definition.Category] = new() { definition }; 
            }

            //Remove single items
            foreach(var category in categories.Values.Where(category => category.Count == 1).ToList())
            {
                var def = category[0];
                categories.Remove(def.Category);
                def.Category = string.Empty;
                if (categories.TryGetValue(string.Empty, out List<FieldRenderDefinition> value))
                {
                    value.Add(def);
                }
                else categories[string.Empty] = new() { def };
            }

            //If we have only 1 category just remove categories altogether
            if(categories.Count(category => category.Value.Exists(def => def.IsVisible)) == 1)
            {
                var newList = categories.Values.SelectMany(val => val).ToList();
                foreach(var def in newList)
                {
                    def.Category = string.Empty;
                }

                return new Dictionary<string, List<FieldRenderDefinition>>
                {
                    { string.Empty, newList }
                };
            }

            return categories;

            //TODO: Find the longest shared substring in the list
            //var optimizedCategories = new Dictionary<string, List<FieldRenderDefinition>>();

            //foreach(var entry in categories)
            //{
            //    var largestCommonFactor = entry.Key == string.Empty ? string.Empty : StringTools.FindLargestCommonSubstring(entry.Value.Select(def => def.Name.ToLowerInvariant()).ToList());
            //    if(largestCommonFactor != string.Empty) largestCommonFactor = StringTools.CapitalizeAfterWhitespace(largestCommonFactor);
            //    foreach(var def in entry.Value)
            //    {
            //        def.Category = largestCommonFactor;
            //    }
            //    optimizedCategories[largestCommonFactor] = entry.Value;
            //}

            //return optimizedCategories;
        }
    }
}
