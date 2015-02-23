﻿namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IPolicySummary
    {
        string BaseUrl { get; set; }
        long ProjectId { get; set; }
        string ProjectName { get; set; }
        string ReportTime { get; set; }
    }
}
