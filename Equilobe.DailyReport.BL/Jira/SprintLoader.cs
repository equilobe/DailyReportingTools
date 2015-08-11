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

        public SprintContext GetSprintDetails(DateTime date)
        {
            var rapidViewId = GetRapidViewId();
            if (rapidViewId == null)
                return null;

            var sprints = Client.GetAllSprints(rapidViewId);

            if (sprints == null || sprints.Count == 0)
                return null;

            return GetSprintContext(date, rapidViewId, sprints);
        }

        private SprintContext GetSprintContext(DateTime date, string rapidViewId, List<Sprint> sprints)
        {
            var sprintContext = new SprintContext();
            var futureSprints = new List<Sprint>();

            while (sprints.Count > 0)
            {
                var sprint = sprints.Last();
                sprint = GetCompleteSprint(sprint.id.ToString(), rapidViewId);
                if (sprint.StartDate != null && sprint.StartDate.Value.Date <= date)
                {
                    if (sprint.CompletedDate > date)
                        sprint.state = "ACTIVE";

                    sprintContext.ReportSprint = sprint;
                    if (!futureSprints.IsEmpty())
                        sprintContext.FutureSprint = futureSprints.Last();

                    return sprintContext;
                }
                futureSprints.Add(sprint);
                sprints.Remove(sprints.Last());
            }

            return sprintContext;
        }

        private string GetRapidViewId()
        {
            var projectViews = GetRapidViewsFromProject();
            var activeView = GetActiveView(projectViews);

            if (activeView == null)
                return null;

            var rapidView = GetRapidView(activeView.id.ToString());
            var rapidViewId = rapidView.rapidViewId.ToString();
            return rapidViewId;
        }

        Sprint GetCompleteSprint(string sprintId, string rapidViewId)
        {
            var completedSprint = Client.GetSprintReport(rapidViewId, sprintId).sprint;

            return completedSprint;
        }
    }
}
