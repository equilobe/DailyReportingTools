using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Equilobe.DailyReport.Models.Policy
{
    public class Month
    {
        [XmlAttribute]
        public string MonthName { get; set; }

        [DefaultValue(0)]
        public int AllocatedHoursPerDay { get; set; }

        [DefaultValue(0)]
        public int AllocatedHoursPerMonth { get; set; }

        public string NonWorkingDays { get; set; }

        [XmlIgnore]
        public List<int> NonWorkingDaysList { get; set; }
    }
}
