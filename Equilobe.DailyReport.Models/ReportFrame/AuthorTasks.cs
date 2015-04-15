using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.ReportFrame
{
    public class AuthorTasks
    {
        public List<IssueDetailed> Issues { get; set; }
        public string AuthorName { get; set; }
    }
}
