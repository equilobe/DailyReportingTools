using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.ReportExecution
{
    public class ConfirmationContext
    {
        public ExecutionContext ExecutionContext { get; set; }
        public List<JiraUser> Users { get; set; }
        public TimeSpan OffsetFromUtc { get; set; }
        public List<IndividualDraftConfirmation> IndividualDrafts { get; set; }
    }
}
