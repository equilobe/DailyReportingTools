using Equilobe.DailyReport.Models.Interfaces;
using System;

namespace Equilobe.DailyReport.Models.Jira.Filters
{
    public class ProjectDateFilter
    {
        public IJiraRequestContext Context { get; set; }
        public string ProjectName { get; set; }
        public string ProjectKey { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Offset { get; set; }
    }
}
