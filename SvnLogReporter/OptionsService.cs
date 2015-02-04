using Equilobe.DailyReport.Models.ReportPolicy;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceControlLogReporter
{
    class OptionsService
    {
        Policy Policy { get; set; }
        Options Options { get; set; }

        public OptionsService(Policy policy, Options options)
        {
            Policy = policy;
            Options = options;
        }

        public void SetOptions()
        {
            Options.HasToDate = HasToDate();
            Options.HasFromDate = HasFromDate();
            LoadDates();
        }

        public bool HasToDate()
        {
            return Options.StringToDate != null;
        }

        public bool HasFromDate()
        {
            return Options.StringFromDate != null;
        }

        public static string DateToISO(DateTime date)
        {
            return date.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", DateTimeFormatInfo.InvariantInfo);
        }

        public List<DateTime> GetDates()
        {
            var dates = new List<DateTime>();
            var startDate = Options.FromDate;
            while (startDate.AddDays(1) <= Options.ToDate)
            {
                startDate = startDate.Date;
                dates.Add(startDate);
                startDate = startDate.AddDays(1);
            }
            return dates;
        }

        public void LoadDates()
        {
            if (Options.HasToDate)
                Options.ToDate = GetDate(Options.StringToDate).Date;

            if (Options.HasFromDate)
                Options.FromDate = GetDate(Options.StringFromDate).Date;

            SetDefaultDates();

            if (Options.ToDate < Options.FromDate)
                throw new ArgumentException("ToDate < FromDate");

            if (Options.ToDate > DateTime.Today.AddDays(1))
                Options.ToDate = DateTime.Today.AddDays(1);

            Options.ReportDates = GetDates();
        }

        private void SetDefaultDates()
        {
            if (!Options.HasToDate && !Options.HasFromDate)
            {
                SetDates();
            }
            else if (!Options.HasToDate)
            {
                Options.ToDate = Options.FromDate.AddDays(1);
            }

            else if (!Options.HasFromDate)
            {
                Options.FromDate = Options.ToDate.AddDays(-1);
            }
        }

        private void SetDates()
        {
            if (Policy.GeneratedProperties.LastReportSentDate == new DateTime())
            {
                Options.FromDate = DateTime.Now.ToOriginalTimeZone().AddDays(-1).Date;
                Options.ToDate = DateTime.Now.ToOriginalTimeZone().Date;
            }
            else
                SetDatesFromLastSentReport();
        }

        private void SetDatesFromLastSentReport()
        {
            Options.FromDate = Policy.GeneratedProperties.LastReportSentDate.Date;
            Options.ToDate = DateTime.Now.ToOriginalTimeZone().Date;
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
            var today = DateTime.Now.ToOriginalTimeZone().DayOfWeek;
            if (Policy.AdvancedOptions.WeekendDaysList.Exists(d => d == today))
                return true;
            return false;
        }
    }
}
