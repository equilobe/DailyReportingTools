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

        public SprintContext GetSprintDetails()
        {
            var boardId = Client.Board(Filter.ProjectKey).id;

            if (boardId == 0)
                return null;

            var minimumSprints = 3;
            var lastSprints = GetLastSprints(boardId, minimumSprints);

            return GetSprintContext(lastSprints);
        }

        private List<Sprint> GetLastSprints(long boardId, long minimumSprints)
        {
            var startAt = (long)0;
            var total = (long)0;

            while (true)
            {
                var sprintsResponse = Client.GetAllSprints(boardId, startAt.ToString());
                
                if (sprintsResponse.Values.Count < minimumSprints)
                {
                    if (total == 0)
                        return sprintsResponse.Values;

                    if (sprintsResponse.IsLast)
                        startAt -= minimumSprints - sprintsResponse.Values.Count;
                }
                else
                {
                    if (sprintsResponse.IsLast)
                        return sprintsResponse.Values;

                    startAt += sprintsResponse.MaxResults;
                    total += sprintsResponse.MaxResults;
                }
            }
        }

        private SprintContext GetSprintContext(List<Sprint> sprints)
        {
            var sprintContext = new SprintContext();
            var futureSprints = new List<Sprint>();

            while (sprints.Count > 0)
            {
                var sprint = sprints.Last();

                if (sprint.StartDate != null && sprint.StartedAt.Value.Date.ToOriginalTimeZone(Filter.Offset) <= Filter.Date)
                {
                    if (sprint.CompletedAt.ToOriginalTimeZone(Filter.Offset) > Filter.Date)
                        sprint.State = "ACTIVE";

                    sprintContext.ReportSprint = sprint;

                    SetFutureSprint(sprintContext, futureSprints);
                    SetPastSprint(sprints, sprintContext);

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

        private void SetPastSprint(List<Sprint> sprints, SprintContext sprintContext)
        {
            if (sprints.Count < 2)
                return;

            sprintContext.PastSprint = sprints[sprints.Count - 2];
        }
    }
}
