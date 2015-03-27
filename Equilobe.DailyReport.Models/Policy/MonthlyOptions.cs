using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Policy
{
    public class MonthlyOptions
    {
        public List<Month> Months { get; set; }

        public MonthlyOptions()
        {
            Months = new List<Month>();
            DateTimeFormatInfo.InvariantInfo.MonthNames.ToList()
                                                       .Where(monthName => !string.IsNullOrEmpty(monthName))
                                                       .ToList()
                                                       .ForEach(monthName => Months.Add(new Month
                                                       {
                                                           MonthName = monthName
                                                       }));
        }
    }
}
