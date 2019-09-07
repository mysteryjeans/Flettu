using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Flettu.Extensions
{
    public static class CollectionExtension
    {
        public static void AddRange<T>(this Collection<T> collection, IEnumerable<T> items)
        {
            items.ForEach(item => collection.Add(item));
        }
    }
}
