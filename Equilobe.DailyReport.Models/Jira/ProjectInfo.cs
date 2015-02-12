using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Jira
{
    [DataContract]
    public class ProjectInfo
    {
        [DataMember(Name = "id")]
        public long ProjectId { get; set; }
        [DataMember(Name = "name")]
        public string ProjectName { get; set; }
    }
}
