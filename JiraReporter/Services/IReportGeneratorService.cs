using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.ReportFrame;
using System;
namespace JiraReporter.Services
{
    interface IReportGeneratorService : IService
    {
        JiraReport GenerateReport(JiraReport report);
        JiraReport GetIndividualReport(JiraReport report, JiraAuthor author);
    }
}
