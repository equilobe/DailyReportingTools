using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.ReportFrame
{
    public class JiraRequestContext : IJiraRequestContext
    {
        public string BaseUrl { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string SharedSecret { get; set; }
    }
}
