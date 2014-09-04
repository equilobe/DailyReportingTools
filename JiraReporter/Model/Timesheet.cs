using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AnotherJiraRestClient;

namespace JiraReporter.Model
{
    [XmlRoot("rawTimeSheetRepresentation")]
    public class Timesheet
    {
        [XmlElement("worklog", Type = typeof(WorkLog))]
        public WorkLog Worklog { get; set; }

        [XmlElement("startDate")]
        public DateTime StartDate { get; set; }

        [XmlElement("endDate")]
        public DateTime EndDate { get; set; }
    }
}
