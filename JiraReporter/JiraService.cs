﻿using Equilobe.DailyReport.Models.ReportPolicy;
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
        public JiraOptions JiraOptions { get; set; }

        public JiraService(Policy policy, JiraOptions options)
        {
            Policy = policy;
            JiraOptions = options;
        }

        public List<View> GetRapidViewsFromProject()
        {
            var views = RestApiRequests.GetRapidViews(Policy);
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
            var sprint = GetCompleteSprint(sprints.Last(), rapidViewId);
            sprint = GetSprintFromReportDates(sprint);

            return sprint;
        }

        public Sprint GetSprintFromReportDates(Sprint sprint)
        {
            if (sprint.EndDate < JiraOptions.FromDate)
                return null;

            return sprint;
        }

        public Sprint GetCompleteSprint(Sprint sprint, string rapidViewId)
        {
            var completedSprint = RestApiRequests.GetSprintReport(rapidViewId, sprint.id.ToString(), Policy).sprint;

            return completedSprint;
        }
    }
}
