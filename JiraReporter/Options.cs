using CommandLine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    public class Options
    {
        [Option(null, "policy", Required = true)]
        public string PolicyPath { get; set; }
        [Option(null, "from", Required = false)]
        public string StringFromDate { get; set; }
        [Option(null, "to", Required = false)]
        public string StringToDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public void LoadDates()
        {
            if (StringFromDate != null)
                FromDate = GetDate(StringFromDate);

            if (StringToDate != null)
                ToDate = GetDate(StringToDate);

            if (StringFromDate == null && StringToDate == null)
            {
                var now = DateTime.Now;
                FromDate = now.AddDays(-1);
                ToDate = FromDate;
            }
            else if (StringFromDate == null)
                FromDate = ToDate;
            else if (StringToDate == null)
                ToDate = FromDate;           
        }

        private DateTime GetDate(string dateString)
        {
            DateTime date = Convert.ToDateTime(dateString);
            return date;
        }

        public static string DateToString(DateTime date)
        {
            return date.ToString("dd/MMM/yyyy", DateTimeFormatInfo.InvariantInfo);
        }
    }
}
