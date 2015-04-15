using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.ReportFrame
{
    public class AuthorizationContext 
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string SharedSecret { get; set; }
        public string AddonKey { get; set; }
    }
}
