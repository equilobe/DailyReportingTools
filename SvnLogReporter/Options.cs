using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using SourceControlLogReporter.Model;
using System.Globalization;

namespace SourceControlLogReporter
{
    public class Options
    {
        [Option(null, "policy", Required=true)]
        public string PolicyPath { get; set; }
        [Option(null, "to", Required = false)]
        public string StringToDate { get; set; }
        [Option(null, "from", Required = false)]
        public string StringFromDate { get; set; }
        [Option(null, "noemail", Required = false, HelpText = "Don't email report")]
        public bool NoEmail { get; set; }
        [Option(null, "draft", Required = false)]
        public bool IsDraft { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public List<DateTime> ReportDates { get; set; }

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

        public List<DateTime>  GetDates()
        {
            var dates = new List<DateTime>();
            var startDate = FromDate;
            while (startDate.AddDays(1) <= ToDate)
            {
                startDate = startDate.Date;
                dates.Add(startDate);
                startDate = startDate.AddDays(1);
            }
            return dates;
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
                ToDate = GetDate(StringToDate).Date;

            if (HasFromDate)
                FromDate = GetDate(StringFromDate).Date;

            SetDefaultDates();

            SetDates();

           // SwitchToUniversalTime();

            if (ToDate < FromDate)
                throw new ArgumentException("ToDate < FromDate");
            if (ToDate > DateTime.Today.AddDays(1))
                ToDate = DateTime.Today.AddDays(1);

            ReportDates = GetDates();
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
                ToDate = DateTime.Now.ToOriginalTimeZone().Date;
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

        private void SetDates()
        {
            if(Policy.IsWeekendReportActive==true)
                if (DateTime.Now.ToOriginalTimeZone().DayOfWeek == DayOfWeek.Monday)
                {
                    FromDate = DateTime.Now.ToOriginalTimeZone().AddDays(-3).Date;
                    ToDate = DateTime.Now.ToOriginalTimeZone().Date;
                }
        }

        private DateTime GetDate(string dateString)
        {
            DateTime date;
            if (DateTime.TryParse(dateString, out date))
                return date;

            throw new ArgumentException("Date is not in the correct format");
        }

        public bool IsWeekend()
        {
            if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday)
                return true;
            return false;
        }

        Policy Policy { get; set; }
    }
}
