using System.Collections.Generic;

namespace VisualFunctions
{
    public static class FormulaCache
    {
        private static readonly Dictionary<string, List<object>> Cache = new();

        public static List<object> Get(string uid, string formula)
        {
            var key = GenerateCacheKey(uid, formula);

            return Cache.GetValueOrDefault(key);
        }

        public static void Add(string uid, string formula, List<object> result)
        {
            var key = GenerateCacheKey(uid, formula);

            Cache.TryAdd(key, result);
        }

        public static void Clear()
        {
            Cache.Clear();
        }

        private static string GenerateCacheKey(string uid, string formula)
        {
            return $"{uid}:{formula}";
        }
    }
}