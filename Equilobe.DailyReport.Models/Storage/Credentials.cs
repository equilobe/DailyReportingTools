using Equilobe.DailyReport.Models.ReportFrame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Storage
{
    public class Credentials : IRequestContext
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
