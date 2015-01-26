using RestSharp;
using SourceControlLogReporter;
using SourceControlLogReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    class JiraService
    {
        public Policy Policy { get; set; }
        public Options Options { get; set; }

        public JiraService(Policy policy, Options options)
        {
            Policy = policy;
            Options = options;
        }

        public List<View> GetRapidViewsFromProject()
        {
            var views = RestApiRequests.GetRapidViews(Policy);
            var rapidViews = new List<View>();
            foreach(var view in views)
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

        public RapidView GetRapidView(View activeView)
        {
            return RestApiRequests.GetRapidView(activeView.id.ToString(), Policy);
        }

        public Sprint GetLatestSprint()
        {
            var projectViews = GetRapidViewsFromProject();
            var activeView = GetActiveView(projectViews);
            var rapidView = GetRapidView(activeView);
            var rapidViewId = rapidView.rapidViewId.ToString();
            var sprints = RestApiRequests.GetAllSprints(rapidViewId, Policy);
            sprints = GetCompleteSprints(sprints, rapidViewId);
            if (sprints.Count > 0)
                return GetSprintFromReportDates(sprints);
            else
                return null;
        }

        public Sprint GetSprintFromReportDates(List<Sprint> sprints)
        {
            var sprintList = new List<Sprint>();
            sprintList = sprints.FindAll(s => s.StartDate >= Options.FromDate || s.EndDate >= Options.FromDate);
            return sprintList.Last();
        }

        public List<Sprint> GetCompleteSprints(List<Sprint> sprints, string rapidViewId)
        {
            var completeSprints = new List<Sprint>();
            foreach(var sprint in sprints)
            {
                var completeSprint = RestApiRequests.GetSprintReport(rapidViewId, sprint.id.ToString(), Policy).sprint;
                completeSprints.Add(completeSprint);
            }

            return completeSprints;
        }
    }
}
