using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.SL;
using RestSharp;
using SourceControlLogReporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    class SprintLoader
    {
        JiraPolicy Policy { get { return Report.Policy; } }
        JiraOptions JiraOptions { get { return Report.Options; } }
        JiraReport Report { get; set; }

        public SprintLoader(JiraReport report)
        {
            Report = report;
        }

        public List<View> GetRapidViewsFromProject()
        {
            var views = new JiraService().GetRapidViews(Report.Settings);
            var rapidViews = new List<View>();
            foreach (var view in views)
            {
                if (view.filter.query.ToLower().Contains(Policy.GeneratedProperties.ProjectKey.ToLower()) || view.filter.query.ToLower().Contains(Policy.GeneratedProperties.ProjectName.ToLower()))
                    rapidViews.Add(view);
            }
            return rapidViews;
        }

        public View GetActiveView(List<View> views)
        {
            return views.Find(v => v.sprintSupportEnabled == true);
        }

        public RapidView GetRapidView(string activeViewId)
        {
            return new JiraService().GetRapidView(Report.Settings, activeViewId);
        }

        public Sprint GetLatestSprint()
        {
            var projectViews = GetRapidViewsFromProject();
            var activeView = GetActiveView(projectViews);
            var rapidView = GetRapidView(activeView.id.ToString());
            var rapidViewId = rapidView.rapidViewId.ToString();
            var sprints = new JiraService().GetAllSprints(Report.Settings, rapidViewId);
            var sprint = GetCompleteSprint(sprints.Last().id.ToString(), rapidViewId);
            sprint = GetSprintFromReportDates(sprint);

            return sprint;
        }

        public Sprint GetSprintFromReportDates(Sprint sprint)
        {
            if (sprint.EndDate < JiraOptions.FromDate)
                return null;

            return sprint;
        }

        public Sprint GetCompleteSprint(string sprintId, string rapidViewId)
        {
            var completedSprint = new JiraService().GetSprintReport(Report.Settings, rapidViewId, sprintId).sprint; 

            return completedSprint;
        }
    }
}
