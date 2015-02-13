using System;
using Equilobe.DailyReport.Models.Storage;

namespace Equilobe.DailyReport.Models.ReportFrame
{
    public interface IContext
    {
        JiraOptions Options { get; }
        JiraPolicy Policy { get; }
        DateTime ReportDate { get; }
        TimeSpan OffsetFromUtc { get; set; }
    }
}
