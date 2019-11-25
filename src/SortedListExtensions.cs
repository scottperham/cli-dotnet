using System.Collections.Generic;

namespace cli_dotnet
{
    public static class SortedListExtensions
    {
        public static void AddOrUpdate<TKey, TValue>(this SortedList<TKey, TValue> list, TKey key, TValue value)
        {
            if (list.ContainsKey(key))
            {
                list[key] = value;
            }
            else
            {
                list.Add(key, value);
            }
        }

        public static TValue GetOrDefault<TKey, TValue>(this SortedList<TKey, TValue> list, TKey key)
        {
            if (list.TryGetValue(key, out var value))
            {
                return value;
            }

            return default;
        }
    }
}
