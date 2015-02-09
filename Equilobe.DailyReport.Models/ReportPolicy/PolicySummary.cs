using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Equilobe.DailyReport.Models.ReportPolicy
{
    [DataContract]
    public class ProjectInfo
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }
        [DataMember(Name = "name")]
        public string Name { get; set; }
    }

    public class PolicySummary
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Time { get; set; }
        public string BaseUrl { get; set; }
        public string SharedSecret { get; set; }
    }
}