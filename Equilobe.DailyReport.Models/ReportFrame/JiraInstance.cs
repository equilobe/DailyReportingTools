using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.ReportFrame
{
    public class JiraInstance
    {
        public long Id { get; set; }
        public string BaseUrl { get; set; }
        public bool Active { get; set; }
        public List<BasicReportSettings> Projects { get; set; }
    }
}
