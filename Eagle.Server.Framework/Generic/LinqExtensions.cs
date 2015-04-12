using System;
using System.Collections.Generic;
using System.Linq;

namespace Eagle.Server.Framework.Generic
{
    public static class LinqExtensions
    {
        public static TSource MaxOrDefault<TSource>(this IEnumerable<TSource> source, TSource fallback = default(TSource))
        {
            if (source == null)
                throw new ArgumentNullException("source");
            Comparer<TSource> @default = Comparer<TSource>.Default;
            TSource y = default(TSource);
            if ((object)y == null)
            {
                foreach (TSource x in source)
                {
                    if ((object)x != null && ((object)y == null || @default.Compare(x, y) > 0))
                        y = x;
                }
                return y;
            }
            bool flag = false;
            foreach (TSource x in source)
            {
                if (flag)
                {
                    if (@default.Compare(x, y) > 0)
                        y = x;
                }
                else
                {
                    y = x;
                    flag = true;
                }
            }
            if (flag)
                return y;
            return fallback;
        }

        public static TResult MaxOrDefault<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector, TResult fallback = default(TResult))
        {
            return source.Select(selector).MaxOrDefault(fallback);
        }
    }
}
