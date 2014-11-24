using RestSharp;
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

        public JiraService(Policy policy)
        {
            Policy = policy;
        }

        public List<View> GetRapidViewsFromProject()
        {
            var views = RestApiRequests.GetRapidViews(Policy);
            var rapidViews = new List<View>();
            foreach(var view in views)
            {
                if (view.filter.query.ToLower().Contains(Policy.ProjectKey.ToLower()) || view.filter.query.ToLower().Contains(Policy.ProjectName.ToLower()))
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
            if (rapidView.sprintsData.sprints.Count > 0)
                return rapidView.sprintsData.sprints.Last();
            else
                return null;
        }
    }
}
