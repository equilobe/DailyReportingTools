using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.Storage;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Equilobe.DailyReport.Models.Jira.Filters;

namespace Equilobe.DailyReport.BL.Jira
{
    public class SprintLoader
    {
        ProjectDateFilter Filter { get; set; }
        JiraClient Client { get; set; }

        public SprintLoader(ProjectDateFilter filter, JiraClient client)
        {
            Filter = filter;
            Client = client;
        }

        List<View> GetRapidViewsFromProject()
        {
            var views = Client.GetRapidViews(); 

            var rapidViews = views
                .Where(view => view.filter.query.ToLower().Contains(Filter.ProjectKey.ToLower())
                            || view.filter.query.ToLower().Contains(Filter.ProjectName.ToLower()))
                .Select(view => view)
                .ToList();

            return rapidViews;
        }

        View GetActiveView(List<View> views)
        {
            return views.Find(v => v.sprintSupportEnabled == true);
        }

        RapidView GetRapidView(string activeViewId)
        {
            return Client.GetRapidView(activeViewId);
        }

        public Sprint GetLatestSprint()
        {
            var projectViews = GetRapidViewsFromProject();
            var activeView = GetActiveView(projectViews);
            if (activeView == null)
                return null;

            var rapidView = GetRapidView(activeView.id.ToString());
            var rapidViewId = rapidView.rapidViewId.ToString();
            var sprints = Client.GetAllSprints(rapidViewId);

            if (sprints == null || sprints.Count == 0)
                return null;
            
            var sprint = GetCompleteSprint(sprints.Last().id.ToString(), rapidViewId);

            return sprint;
        }

        Sprint GetCompleteSprint(string sprintId, string rapidViewId)
        {
            var completedSprint = Client.GetSprintReport(rapidViewId, sprintId).sprint;

            return completedSprint;
        }
    }
}
