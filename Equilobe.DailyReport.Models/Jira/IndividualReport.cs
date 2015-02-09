using Equilobe.DailyReport.Models.ReportPolicy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Jira
{
    public class IndividualReport : JiraReport
    {
        public JiraAuthor Author { get; set; }

        public IndividualReport(JiraPolicy policy)
            : base(policy)
        {
            Policy = policy;
        }
    }
}
