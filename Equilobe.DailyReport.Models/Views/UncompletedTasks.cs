using Equilobe.DailyReport.Models.ReportFrame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Views
{
    public class UncompletedTasks
    {
        public List<IssueDetailed> Issues { get; set; }
        public string AuthorName { get; set; }
    }
}
