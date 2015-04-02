using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.SourceControl;
using SourceControlLogReporter;

namespace JiraReporter.SourceControl
{
    class SvnReportSourceControl : SvnReport
    {
        JiraReport Context { get; set; }

        public SvnReportSourceControl(JiraReport context)
        {
            Context = context;
        }

        public override Log CreateLog()
        {
            var context = new SourceControlContext
            {
                SourceControlOptions = Context.Policy.SourceControlOptions,
                FromDate = Context.Options.FromDate,
                ToDate = Context.Options.ToDate
            };
            return SvnService.GetLogWithCommitLinks(context);
        }
    }
}
