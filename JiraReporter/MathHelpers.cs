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

        public static double RuleOfThree(double referenceNumber, double associatedValue, double evaluatedAmount)
        {
            return (referenceNumber * evaluatedAmount) / associatedValue;
        }

        public static int RoundToNextEvenInteger(double number)
        {
            var roundedNumber = Math.Ceiling(number);
            if (roundedNumber % 2 != 0)
                roundedNumber++;
            return (int)roundedNumber;
        }
    }
}
