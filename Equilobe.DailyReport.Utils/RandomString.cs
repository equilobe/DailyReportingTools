using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public class RandomString
    {
        public static string Get()
        {
            return Path.GetRandomFileName().Replace(".",string.Empty);
        }

        public static string Get(string prefix)
        {
            return prefix + Get();
        }
    }
}
