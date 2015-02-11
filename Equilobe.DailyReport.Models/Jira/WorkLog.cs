﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Equilobe.DailyReport.Models.Jira
{
    [Serializable]
    public class WorkLog
    {
        [XmlElement("item", Type = typeof(Issue))]
        public List<Issue> Issues { get; set; }
    }
}