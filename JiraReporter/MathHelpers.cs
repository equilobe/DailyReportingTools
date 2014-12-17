using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    public static class MathHelpers
    {
        public static double GetPercentage(double value, double total)
        {
            var percentage = (value * 100) / total;
            return percentage;
        }

        public static double GetVariance(double allocatedTime, double workedTime)
        {
            return allocatedTime - workedTime;
        }
    }
}
