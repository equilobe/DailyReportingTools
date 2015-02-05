using Equilobe.DailyReport.Models.ReportPolicy;
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
        public JiraPolicy Policy;
        public JiraOptions JiraOptions;
        public Summary Summary { get; set; }

        public IndividualReport(JiraPolicy policy, JiraOptions options)
            : base(policy, options)
        {
            Policy = policy;
            JiraOptions = options;
        }

        public override string GetReportTitle()
        {
            var reportDate = SourceControlLogReporter.ReportDateFormatter.GetReportDate(JiraOptions.FromDate, JiraOptions.ToDate);

            return "DRAFT | " + Author.Name + " | " + Policy.GeneratedProperties.ProjectName + " Daily Report | " + reportDate;
        }
    }
}
