using CommandLine;
using Equilobe.DailyReport.Models.ReportPolicy;
using SourceControlLogReporter;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    public class JiraOptions : Options
    {
        [Option(null, "draftKey", Required = false)]
        public string DraftKey { get; set; }
        [Option(null, "triggerKey", Required = false)]
        public string TriggerKey { get; set; }

        public JiraPolicy Policy { get; set; }

        public List<DateTime> ReportDates { get; set; }

        public static string DateToISO(DateTime date)
        {
            return date.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", DateTimeFormatInfo.InvariantInfo);
        }

        public void LoadDates(JiraPolicy policy)
        {
            Policy = policy;

            if (HasToDate)
                ToDate = GetDate(StringToDate).Date;

            if (HasFromDate)
                FromDate = GetDate(StringFromDate).Date;

            SetDefaultDates();

            if (ToDate < FromDate)
                throw new ArgumentException("ToDate < FromDate");
            if (ToDate > DateTime.Today.AddDays(1))
                ToDate = DateTime.Today.AddDays(1);

            ReportDates = GetDates();
        }


        private void SetDefaultDates()
        {
            if (!HasToDate && !HasFromDate)
            {
                SetDates();
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
            if (Policy.GeneratedProperties.LastReportSentDate == new DateTime())
            {
                FromDate = DateTime.Now.ToOriginalTimeZone().AddDays(-1).Date;
                ToDate = DateTime.Now.ToOriginalTimeZone().Date;
            }
            else
                SetDatesFromLastSentReport();
        }

        private void SetDatesFromLastSentReport()
        {
            FromDate = Policy.GeneratedProperties.LastReportSentDate.Date;
            ToDate = DateTime.Now.ToOriginalTimeZone().Date;
        }

        public bool IsWeekend()
        {
            var today = DateTime.Now.ToOriginalTimeZone().DayOfWeek;
            if (Policy.AdvancedOptions.WeekendDaysList.Exists(d => d == today))
                return true;
            return false;
        }
    }
}
