using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DailyReportWeb.Models
{
    public class PolicySummary
    {
        public PolicySummary()
        {
            ReportTime = "09:00";
        }
        
        public string Id { get; set; }
        public string ProjectName { get; set; }
        public string ReportTime { get; set; } 
    }
}