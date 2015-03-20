using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.ReportFrame
{
    public class IssuesContext
    {
        public JiraRequestContext RequestContext { get; set; }
        public string ProjectKey { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
