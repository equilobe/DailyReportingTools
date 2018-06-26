using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.BitBucket
{
    public class Author
    {
        public string Username { get; set; }
        public string Display_name { get; set; }
        public string Account_id { get; set; }
        public List<Link> Links { get; set; }
        public string Type { get; set; }
        public string Uuid { get; set; }
    }
}
