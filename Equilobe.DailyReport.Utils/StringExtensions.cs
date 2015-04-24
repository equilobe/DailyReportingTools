using System;
using System.Collections.Generic;
using System.Linq;

namespace Equilobe.DailyReport.Utils
{
    public static class StringExtensions
    {
        public static string GetNaturalLanguage<T>(List<T> items)
        {
            if (!items.Any())
                return string.Empty;

            if (items.Count == 1)
                return items.First().ToString();

            var firstElements = items.Take(items.Count - 1);
            var lastElement = items.Last();
            return String.Format("{0} and {1}", string.Join(", ", firstElements), lastElement);
        }
    }
}
