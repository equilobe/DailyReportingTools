using JiraReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JiraReporter
{
    public class CompletedTasks
    {
        public List<Task> Tasks { get; set; }
        public string CompletedTimeAgo { get; set; }
    }
}
