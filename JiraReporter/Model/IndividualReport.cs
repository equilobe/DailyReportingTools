using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Model
{
    public class IndividualReport : Report
    {
        public Author Author { get; set; }
        public SourceControlLogReporter.Model.Policy Policy;
        public SourceControlLogReporter.Options Options;
        public Summary Summary { get; set; }

        public IndividualReport(SourceControlLogReporter.Model.Policy policy, SourceControlLogReporter.Options options) : base(policy, options)
        {
            Policy = policy;
            Options = options;
        }

        public override string GetReportTitle()
        {
            var reportDate = SourceControlLogReporter.ReportDateFormatter.GetReportDate(Options.FromDate, Options.ToDate);

            return "DRAFT | " + Author.Name + " | " + Policy.GeneratedProperties.ProjectName + " Daily Report | " + reportDate;
        }
    }
}
