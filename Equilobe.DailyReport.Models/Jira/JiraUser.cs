using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;

namespace Equilobe.DailyReport.Models.Jira
{
    [DataContract]
    public class JiraUser
    {
        [DataMember]
        public string self { get; set; }
        [DataMember]
        public string key { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string emailAddress { get; set; }
        [DataMember]
        public string displayName { get; set; }
        [DataMember]
        public bool active { get; set; }
        [DataMember]
        public Avatar avatarUrls { get; set; }
    }
}
