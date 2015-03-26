using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.TimeZone
{ 
    public class TimeZone
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double UtcOffset { get; set; }
    }
}
