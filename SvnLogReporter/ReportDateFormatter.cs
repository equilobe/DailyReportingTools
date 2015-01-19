using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceControlLogReporter
{
    public class ReportDateFormatter
    {

        public static string GetReportDate(DateTime fromDate, DateTime toDate)
        {
            if ((toDate - fromDate).Days > 1)
                return fromDate.ToString("ddd, dd MMM yyyy");

            return fromDate.ToString("ddd, dd MMM yyyy") + " - " + toDate.AddDays(-1).ToString("ddd, dd MMM yyyy");
        }
    }
}
