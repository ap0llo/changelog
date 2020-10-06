using System.Collections.Generic;
using System.Linq;

namespace Grynwald.ChangeLog
{
    /// <summary>
    /// Extension methods for <see cref="IGrouping{TKey, TElement}"/>
    /// </summary>
    internal static class GroupingExtensions
    {
        public static void Deconstruct<TKey, TElement>(this IGrouping<TKey, TElement> grouping, out TKey key, out IEnumerable<TElement> elements)
        {
            key = grouping.Key;
            elements = grouping;
        }
    }
}
