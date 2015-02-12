using System;
using Equilobe.DailyReport.Models.ReportPolicy;

namespace Equilobe.DailyReport.Models.Jira
{
    public interface IContext
    {
        JiraOptions Options { get; }
        JiraPolicy Policy { get; }
        DateTime ReportDate { get; }
        TimeSpan OffsetFromUtc { get; set; }
    }
}
