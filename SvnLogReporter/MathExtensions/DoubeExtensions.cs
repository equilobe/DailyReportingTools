using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class DoubeExtensions
    {
        public static string RoundDoubleDecimals(this double number)
        {
            if (number == (int)number)
                return string.Format("{0}", number);
            else
                return string.Format("{0:0.00}", number);
        }
    }
}
