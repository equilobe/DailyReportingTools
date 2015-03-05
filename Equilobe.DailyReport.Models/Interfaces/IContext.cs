using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using System;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IContext
    {
        JiraOptions Options { get; }
        JiraPolicy Policy { get; }
        DateTime ReportDate { get; }
        TimeSpan OffsetFromUtc { get; set; }
    }
}
