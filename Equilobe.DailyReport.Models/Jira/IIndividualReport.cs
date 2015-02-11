using System;
namespace Equilobe.DailyReport.Models.Jira
{
    public interface IIndividualReport : IJiraReport
    {
        JiraAuthor Author { get; set; }
    }
}
