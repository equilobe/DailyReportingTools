using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.SourceControl;

namespace SourceControlLogReporter
{
    public class BitBucketReport : ReportBase
    {
        public IBitBucketService BitBucketService { get; set; }

        public BitBucketReport(Policy p, Options o)
            : base(p, o)
        {

        }

        public BitBucketReport()
        {

        }

        public override Log CreateLog()
        {
            var context = new SourceControlContext { SourceControlOptions = Policy.SourceControlOptions, FromDate = Options.FromDate, ToDate = Options.ToDate };

            return BitBucketService.GetLog(context);
        }

        protected override void AddPullRequests(Report report, Log log)
        {
            report.PullRequests = log.PullRequests;
        }
    }
}
