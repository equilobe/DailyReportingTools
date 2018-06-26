using System;
using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.BitBucket
{
    public class PullRequest
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public State State { get; set; }
        public DateTime Created_on { get; set; }
        public DateTime Updated_on { get; set; }
        public int Comment_count { get; set; }
        public int Task_count { get; set; }
        public bool Close_source_branch { get; set; }
        public string Reason { get; set; }
        public List<Link> Links { get; set; }
        public Summary Summary { get; set; }
        public SourceOrDestination Destination { get; set; }
        public SourceOrDestination Source { get; set; }
        public Author Author { get; set; }
    }
}
