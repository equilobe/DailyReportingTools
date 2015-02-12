using System;
using Equilobe.DailyReport.Models.ReportPolicy;

namespace Equilobe.DailyReport.Models.Report
{
    public interface IContext
    {
        JiraOptions Options { get; }
        JiraPolicy Policy { get; }
        DateTime ReportDate { get; }
        TimeSpan OffsetFromUtc { get; set; }
    }
}
