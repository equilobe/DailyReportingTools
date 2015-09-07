using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Jira
{
    public class JiraInstance
    {
        public long Id { get; set; }
        public string BaseUrl { get; set; }
        public DateTime ExpirationDate { get; set; }
        public List<BasicReportSettings> Projects { get; set; }
        public bool IsActive
        {
            get
            {
                return ExpirationDate > DateTime.Now;
            }
        }
    }
}
