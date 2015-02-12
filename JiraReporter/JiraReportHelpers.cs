﻿using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.ReportPolicy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    public static class JiraReportHelpers
    {
        public static string GetReportTitle(JiraReport report)
        {
            var title = string.Empty;
            var reportDate = SourceControlLogReporter.ReportDateFormatter.GetReportDate(report.Options.FromDate, report.Options.ToDate);
            if (report.Policy.GeneratedProperties.IsFinalDraft || report.Policy.GeneratedProperties.IsIndividualDraft)
                title += "DRAFT | ";
            if (report.Policy.GeneratedProperties.IsIndividualDraft)
                title += report.Author.Name + " | ";
            title += report.Policy.GeneratedProperties.ProjectName + " Daily Report | " + reportDate;

            return title;
        }
    }
}
