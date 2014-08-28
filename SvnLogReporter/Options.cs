using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using SvnLogReporter.Model;
using System.Globalization;

namespace SvnLogReporter
{
    class Options
    {
        [Option(null, "policy", Required=true)]
        public string PolicyPath { get; set; }
        [Option(null, "to", Required = false)]
        public string StringToDate { get; set; }
        [Option(null, "from", Required = false)]
        public string StringFromDate { get; set; }
        [Option(null, "noemail", Required = false, HelpText = "Don't email report")]
        public bool NoEmail { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public bool HasToDate
        {
            get
            {
                return StringToDate != null;
            }
        }

        public bool HasFromDate
        {
            get
            {
                return StringFromDate != null;
            }
        }

        public DateTime ReportDate
        {
            get
            {
                return ToDate.ToLocalTime().Hour < 12 ? FromDate.ToLocalTime() : ToDate.ToLocalTime();
            }
        }

        public static string DateToISO(DateTime date)
        {
            return date.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", DateTimeFormatInfo.InvariantInfo);
        }

        public static DateTime FloorToDay(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0);
        }

        public void  GetDates(List<DateTime> dates)
        {
            var startDate = FromDate;
            while (startDate.AddDays(1) <= ToDate)
            {
                startDate = FloorToDay(startDate);
                dates.Add(startDate);
                startDate = startDate.AddDays(1);
            }
        }


        [HelpOption]
        public string GetUsage()
        {
            var usage = new StringBuilder();
            usage.AppendLine("Svn Reporter");
            usage.AppendLine("usage svnreporter.exe -from [from date] -to [to date] --noemail");
            return usage.ToString();
        }


        public void LoadDates(Policy p)
        {
            this.Policy = p;
            if (HasToDate)
                ToDate = GetDate(StringToDate);

            if (HasFromDate)
                FromDate = GetDate(StringFromDate);

            SetDefaultDates();

            SwitchToUniversalTime();

            if (ToDate < FromDate)
                throw new ArgumentException("ToDate < FromDate");
        }

        private void SwitchToUniversalTime()
        {
            ToDate = ToDate.ToUniversalTime();
            FromDate = FromDate.ToUniversalTime();
        }

        private void SetDefaultDates()
        {
            if (!HasToDate && !HasFromDate)
            {
                var now = DateTime.Now;
                ToDate = now.Hour < StartHour ? now.Date.AddDays(-1) : now.Date;                               
                ToDate = ToDate.AddHours(StartHour);
                FromDate = ToDate.AddDays(-1);              
            }
            else if (!HasToDate)
            {
                ToDate = FromDate.AddDays(1);
            }

            else if (!HasFromDate)
            {
                FromDate = ToDate.AddDays(-1);
            }
        }

        private DateTime GetDate(string dateString)
        {
            DateTime date;
            if (DateTime.TryParse(dateString, out date))
            {
                if (dateString.Length <= 10)
                    return date.AddHours(StartHour);
                return date;
            }

            throw new ArgumentException("Date is not in the correct format");
        }

        Policy Policy { get; set; }

        private int StartHour
        {
            get
            {
                int defaultTime = Policy.DayStartHour;

                if (defaultTime < 0 || defaultTime > 23)
                    defaultTime = 5;
                return defaultTime;
            }
        }
    }
}
