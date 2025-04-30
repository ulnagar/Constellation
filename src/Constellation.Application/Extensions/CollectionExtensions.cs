namespace Constellation.Application.Extensions;

using System.Collections.Generic;
using System.Linq;

public static class CollectionExtensions
{
    public static IEnumerable<List<T>> Partition<T>(this IEnumerable<T> values, int chunkSize)
    {
        return values
            .Select((x, i) => new { Index = i, Value = x })
            .GroupBy(x => x.Index / chunkSize)
            .Select(x => x.Select(v => v.Value).ToList())
            .ToList();
    }
}