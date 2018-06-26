using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.BitBucket
{
    public class PullRequests
    {
        public int Size { get; set; }
        public int Page { get; set; }
        public int Pagelen { get; set; }
        public string Next { get; set; }
        public string Previous { get; set; }
        public List<PullRequest> Values { get; set; }
    }
}
