using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.SourceControl;

namespace SourceControlLogReporter
{
    public class SvnReport : ReportBase
    {
        public ISvnService SvnService { get; set; }

        public SvnReport(Policy p, Options o)
            : base(p, o)
        {

        }

        public SvnReport()
        {

        }

        public override Log CreateLog()
        {
            var context = new SourceControlContext
            {
                SourceControlOptions = Policy.SourceControlOptions,
                FromDate = Options.FromDate,
                ToDate = Options.ToDate
            };
            return SvnService.GetLogWithCommitLinks(context);
        }
    }
}
