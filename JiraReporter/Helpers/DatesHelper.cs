using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;

namespace JiraReporter.Helpers
{
    class DatesHelper
    {
        JiraPolicy Policy { get { return Context.Policy; } }
        JiraOptions Options { get { return Context.Options; } }
        JiraReport Context { get; set; }

        public DatesHelper(JiraReport context)
        {
            Context = context;
        }

        public void LoadDates()
        {
            if (Options.HasToDate)
                Options.ToDate = Options.GetDate(Options.StringToDate).Date;

            if (Options.HasFromDate)
                Options.FromDate = Options.GetDate(Options.StringFromDate).Date;

            SetDefaultDates();

            if (Options.ToDate < Options.FromDate)
                throw new ArgumentException("ToDate < FromDate");
            if (Options.ToDate > DateTime.Today.AddDays(1))
                Options.ToDate = DateTime.Today.AddDays(1);

            Options.ReportDates = Options.GetDates();
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
            if (Context.LastReportSentDate == new DateTime())
            {
                Options.FromDate = DateTime.Now.ToOriginalTimeZone(Context.OffsetFromUtc).AddDays(-1).Date;
                Options.ToDate = DateTime.Now.ToOriginalTimeZone(Context.OffsetFromUtc).Date;
            }
            else
                SetDatesFromLastSentReport();
        }

        private void SetDatesFromLastSentReport()
        {
            Options.FromDate = Context.LastReportSentDate.Date;
            Options.ToDate = DateTime.Now.ToOriginalTimeZone(Context.OffsetFromUtc).Date;
        }

        public static bool IsWeekend(JiraReport context)
        {
            var today = DateTime.Now.ToOriginalTimeZone(context.OffsetFromUtc).DayOfWeek;
            return context.Policy.AdvancedOptions.WeekendDaysList.Exists(d => d == today);
        }
    }
}
