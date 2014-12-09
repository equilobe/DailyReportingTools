﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Model
{
    public class Report
    {
        public SourceControlLogReporter.Model.Policy Policy;
        public SourceControlLogReporter.Options Options;

        public Report(SourceControlLogReporter.Model.Policy p, SourceControlLogReporter.Options o)
        {
            Policy = p;
            Options = o;
        }
        public string Title { get; set; }
        public List<Author> Authors { get; set; }
        public DateTime Date { get; set; }
        public Summary Summary { get; set; }
        public SprintTasks SprintTasks { get; set; }
        public List<PullRequest> PullRequests { get; set; }
        public List<PullRequest> UnrelatedPullRequests
        {
            get
            {
                return PullRequests.FindAll(p => p.TaskSynced == false);
            }
        }
    }
}
