﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SourceControlLogReporter.Model
{
    public class Override
    {
        public string Month { get; set; }
        public int AllocatedHoursPerDay { get; set; }
        public int AllocatedHoursPerMonth { get; set; }
        public string Days { get; set; }
        [XmlIgnore]
        public List<int> NonWorkingDays
        {
            get
            {
                var nonWorkingDays = new List<int>();
                var daysString = Days.Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var number in daysString)
                    nonWorkingDays.Add(Int32.Parse(number));
                return nonWorkingDays;
            }
        }
    }
}