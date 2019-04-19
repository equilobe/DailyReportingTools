using System;
using System.Collections.Generic;

namespace Equilobe.DailyReport.Utils
{
    public static class LinqExtensions
    {
        public static IEnumerable<T> RemoveWhere<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            var list = source as List<T>;

            if (list != null)
                list.RemoveAll(new Predicate<T>(predicate));

            return list;
        }
    }
}
