using Equilobe.DailyReport.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Report
{
    public class Error
    {
        public ErrorType Type { get; set; }

        public Error()
        {

        }
        
        public Error(ErrorType type)
        {
            Type = type;
        }
    }
}
