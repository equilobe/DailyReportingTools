using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    class RandomGenerator
    {
        public static string GetRandomString()
        {
            return Path.GetRandomFileName().Replace(".", string.Empty);
        }
    }
}
