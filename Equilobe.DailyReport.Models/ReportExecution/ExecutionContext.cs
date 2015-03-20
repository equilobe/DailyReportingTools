using Equilobe.DailyReport.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.ReportExecution
{
    public class ExecutionContext
    {
        public DateTime Date { get; set; }

        /// <summary>
        /// Unique project key
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Unique user key
        /// </summary>
        public string DraftKey { get; set; }

        public SendScope Scope { get; set; }
    }
}
