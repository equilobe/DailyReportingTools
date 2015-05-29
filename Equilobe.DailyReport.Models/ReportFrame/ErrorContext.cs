using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.ReportFrame
{
    public class ErrorContext
    {
        public List<Error> Errors { get; set; }
        public string Assignee { get; set; }

        public ErrorContext(List<Error> errors, string assignee) 
        {
            Errors = errors;
            Assignee = assignee;
        }
    }
}
