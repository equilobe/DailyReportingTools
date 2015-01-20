using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SourceControlLogReporter.Model
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
        public List<int> NonWorkingDaysList
        {
            get
            {
                var nonWorkingDays = new List<int>();
                var daysString = NonWorkingDays.Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var number in daysString)
                    nonWorkingDays.Add(Int32.Parse(number));
                return nonWorkingDays;
            }
        }

        public static Month SearchOverride(List<Month> overrides, DateTime day)
        {
            if (overrides == null)
                return null;
            else
                return (overrides.Find(o => o.MonthName.ToLower() == day.CurrentMonth().ToLower()));
        }

        public static bool SearchDateInOverrides(List<Month> overrides, DateTime date)
        {
            var currentOverride = new Month();
            if(overrides!=null)
                 currentOverride = SearchOverride(overrides, date);
            if (currentOverride == null || currentOverride.NonWorkingDaysList == null)
                return false;
            else
                return currentOverride.NonWorkingDaysList.Exists(d => d == date.Day);
        }
    }
}
