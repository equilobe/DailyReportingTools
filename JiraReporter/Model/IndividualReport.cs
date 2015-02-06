using Equilobe.DailyReport.Models.ReportPolicy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Model
{
    public class IndividualReport : JiraReport
    {
        public JiraAuthor Author { get; set; }

        public IndividualReport(JiraPolicy policy, JiraOptions options)
            : base(policy, options)
        {
            Policy = policy;
            Options = options;
        }
    }
}
