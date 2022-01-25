using System.Collections.Generic;
using System.Linq;

namespace Constellation.Application.Extensions
{
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
    }
}
