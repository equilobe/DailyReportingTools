using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Equilobe.DailyReport.Models.Web
{
    public class PolicySummary
    {
        public long ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ReportTime { get; set; }
        public string BaseUrl { get; set; }
        public string SharedSecret { get; set; }
    }
}