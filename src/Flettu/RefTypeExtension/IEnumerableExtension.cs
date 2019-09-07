using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Flettu.ValueTypeExtension;

namespace Flettu.RefTypeExtension
{
    /// <summary>
    /// Extension methods for IEnumerable<T> interface
    /// </summary>
    public static class IEnumerableExtension
    {
        /// <summary>
        /// Enumerate each element in the enumeration and execute specified action
        /// </summary>
        /// <typeparam name="T">Type of enumeration</typeparam>
        /// <param name="enumerable">Enumerable collection</param>
        /// <param name="action">Action to perform</param>
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (T item in enumerable)
                action(item);
        }

        /// <summary>
        /// Read only collection of any enumeration
        /// </summary>
        /// <typeparam name="T">Type of enumeration</typeparam>
        /// <param name="collection">Enumerable collection</param>
        /// <returns>ReadOnlyCollection of the collection</returns>
        public static ReadOnlyCollection<T> ToReadOnly<T>(this IEnumerable<T> collection)
        {
            return (new List<T>(collection)).AsReadOnly();
        }

        /// <summary>
        /// Converts bytes collection to hexadecimal representation
        /// </summary>
        /// <param name="bytes">Bytes to convert</param>
        /// <returns>Hexadecimal representation string</returns>
        public static string ToHexString(this IEnumerable<byte> bytes)
        {
            return string.Join("", bytes.Select(b => ("0" + b.ToString("X")).Right(2)));
        }
    }
}
