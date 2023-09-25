namespace Constellation.Application.Extensions;

using System.Collections.Generic;
using System.Linq;

public static class CollectionExtensions
{
    public static IEnumerable<T> RemoveNulls<T>(this IEnumerable<T> list)
    {
        if (typeof(T) == typeof(string))
        {
            return list.Where(item => !string.IsNullOrWhiteSpace(item.ToString()));
        }
        else
        {
            return list.Where(l => l != null);
        }
    }

    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> newItems)
    {
        foreach (T item in newItems)
        {
            collection.Add(item);
        }
    }

    public static IEnumerable<List<T>> Partition<T>(this IEnumerable<T> values, int chunkSize)
    {
        return values
            .Select((x, i) => new { Index = i, Value = x })
            .GroupBy(x => x.Index / chunkSize)
            .Select(x => x.Select(v => v.Value).ToList())
            .ToList();
    }
}