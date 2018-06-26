using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.BitBucket
{
    public class Commit
    {
        public string Hash { get; set; }
        public List<Link> Link { get; set; }
    }
}
