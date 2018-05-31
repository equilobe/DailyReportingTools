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
                .Where(view => view.filter.name.ToLower().Contains(Filter.ProjectKey.ToLower())
                            || view.filter.name.ToLower().Contains(Filter.ProjectName.ToLower()))
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

        public SprintContext GetSprintDetails()
        {
            var boardId = Client.Board(Filter.ProjectKey).id;

            if (string.IsNullOrEmpty(boardId))
                return null;

            var sprints = Client.GetAllSprints(boardId);

            return null;
                
            //var rapidViewId = GetRapidViewId();
            //if (rapidViewId == null)
            //    return null;

            //var sprints = Client.GetAllSprints(rapidViewId, Filter.ProjectKey);

            //if (sprints == null || sprints.Count == 0)
            //    return null;

            //return GetSprintContext(rapidViewId, sprints);
        }

        private SprintContext GetSprintContext(string rapidViewId, List<Sprint> sprints)
        {
            var sprintContext = new SprintContext();
            var futureSprints = new List<Sprint>();

            while (sprints.Count > 0)
            {
                var sprint = sprints.Last();
                sprint = GetCompleteSprint(sprint.id.ToString(), rapidViewId);
                if (sprint.StartDate != null && sprint.StartDate.Value.Date.ToOriginalTimeZone(Filter.Offset) <= Filter.Date)
                {
                    if (sprint.CompletedDate.ToOriginalTimeZone(Filter.Offset) > Filter.Date)
                        sprint.state = "ACTIVE";

                    sprintContext.ReportSprint = sprint;

                    SetFutureSprint(sprintContext, futureSprints);
                    SetPastSprint(sprints, sprintContext, rapidViewId);

                    return sprintContext;
                }
                futureSprints.Add(sprint);
                sprints.Remove(sprints.Last());
            }

            return sprintContext;
        }

        private void SetFutureSprint(SprintContext sprintContext, List<Sprint> futureSprints)
        {
            if (futureSprints.IsEmpty())
                return;

            sprintContext.FutureSprint = futureSprints.Last();
        }

        private void SetPastSprint(List<Sprint> sprints, SprintContext sprintContext, string rapidViewId)
        {
            if (sprints.Count < 2)
                return;

            var pastSprint = sprints[sprints.Count - 2];
            sprintContext.PastSprint = GetCompleteSprint(pastSprint.id.ToString(), rapidViewId);
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
