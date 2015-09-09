using Equilobe.DailyReport.Models.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Web
{
    public class EmailContext
    {
        public Email Email { get; set; }
        public string Subject { get; set; }
        public string ViewPath { get; set; }
        public string UserEmail { get; set; }
    }
}
