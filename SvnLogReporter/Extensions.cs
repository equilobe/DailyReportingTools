using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceControlLogReporter
{
    static class Extensions
    {
        public static string GetCollectionString(this IEnumerable<string> sequence, string elementSeparator)
        {
            return String.Join(elementSeparator, sequence);
        }

        public static string RemoveSpace(this string line)
        {
            return line.Trim();
        }

    }
}
