using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Equilobe.DailyReport.Models.ReportFrame;

namespace Equilobe.DailyReport.Models.Jira.Filters
{
    public class ProjectDateFilter
    {
        public IJiraRequestContext Context { get; set; }
        public string ProjectName { get; set; }
        public string ProjectKey { get; set; }
        public DateTime Date { get; set; }
    }
}
