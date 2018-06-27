using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.SourceControl;
using SourceControlLogReporter;

namespace JiraReporter.SourceControl
{
    public class BitBucketSourceControl : BitBucketReport
    {
        JiraPolicy Policy { get { return Context.Policy; } }
        JiraOptions Options { get { return Context.Options; } }
        JiraReport Context { get; set; }

        public BitBucketSourceControl(JiraReport context)
        {
            Context = context;
        }

        public BitBucketSourceControl()
        {

        }

        public override Log CreateLog()
        {
            var context = new SourceControlContext { SourceControlOptions = Policy.SourceControlOptions, FromDate = Options.FromDate, ToDate = Options.ToDate };
            var log = BitBucketService.GetLog(context);

            return log;
        }
    }
}
