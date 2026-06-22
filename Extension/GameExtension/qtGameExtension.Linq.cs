using System;
using System.Collections.Generic;

namespace qtLib.Extension
{
    public partial class qtGameExtension
    {
        public static IEnumerable<TSource> IntersectBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            IEnumerable<TKey> keys,
            Func<TSource, TKey> keySelector)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (keys == null)
            {
                foreach (var item in source)
                {
                    yield return item;
                }
                yield break;
            }

            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }

            HashSet<TKey> keySet = new HashSet<TKey>(keys);
            HashSet<TKey> seen = new HashSet<TKey>(); // To avoid duplicates in result

            foreach (var item in source)
            {
                var key = keySelector(item);
                if (keySet.Contains(key) && seen.Add(key))
                {
                    yield return item;
                }
            }
        }
    }
}