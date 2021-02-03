using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SavescumBuddy.Wpf.Extensions
{
    public static class CollectionExtensions
    {
        public static Collection<T> ReplaceRange<T>(this Collection<T> collection, IEnumerable<T> items)
        {
            if (collection.Any())
                collection.Clear();

            collection.AddRange(items);
            return collection;
        }
    }
}
