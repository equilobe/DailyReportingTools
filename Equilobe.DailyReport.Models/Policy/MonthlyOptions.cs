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
            var month = 1;
            while (month <= 12)
            {
                Months.Add(new Month { MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month) });
                month++;
            }
        }
    }
}
