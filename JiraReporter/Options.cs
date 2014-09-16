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
                FromDate = DateFromString(StringFromDate);

            if (StringToDate != null)
                ToDate = DateFromString(StringToDate);

            if (StringFromDate == null && StringToDate == null)
            {
                var now = DateTime.Now;
                FromDate = now.AddDays(-1);
                FromDate = new DateTime(FromDate.Year, FromDate.Month, FromDate.Day, 0, 0, 0);
                ToDate = FromDate;
            }
            else if (StringFromDate == null)
                FromDate = ToDate;
            else if (StringToDate == null)
                ToDate = FromDate;           
        }

        private DateTime DateFromString(string dateString)
        {
            DateTime date = Convert.ToDateTime(dateString);
            date = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
            return date;
        }

        public static string DateToString(DateTime date)
        {
            return date.ToString("dd/MMM/yyyy", DateTimeFormatInfo.InvariantInfo);
        }

        public static string DateToISO(DateTime date)
        {
            return date.ToString("yyyy'-'MM'-'dd' 'HH':'mm", DateTimeFormatInfo.InvariantInfo);
        }
    }
}
