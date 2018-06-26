using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.BitBucket
{
    public class Repository
    {
        public List<Link> Links { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Full_name { get; set; }
        public string Uuid { get; set; }
    }
}
