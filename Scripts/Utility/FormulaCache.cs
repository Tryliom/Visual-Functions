using System.Collections.Generic;
using System.Linq;

namespace TryliomFunctions
{
    public static class FormulaCache
    {
        private static readonly Dictionary<string, List<object>> Cache = new();

        public static List<object> Get(string uid, string formula, List<ExpressionVariable> variables)
        {
            var key = GenerateCacheKey(uid, formula, variables);

            return Cache.GetValueOrDefault(key);
        }

        public static void Add(string uid, string formula, List<ExpressionVariable> variables, List<object> result)
        {
            var key = GenerateCacheKey(uid, formula, variables);

            Cache.TryAdd(key, result);
        }

        public static void Clear()
        {
            Cache.Clear();
        }

        private static string GenerateCacheKey(string uid, string formula, List<ExpressionVariable> variables)
        {
            var variableDetails = string.Join(",", variables.FindAll(v => v.Value != null)
                .Select(v => $"{v.Name}:{v.Value.Type.Name}"));
            return $"{uid}:{formula}:{variableDetails}";
        }
    }
}