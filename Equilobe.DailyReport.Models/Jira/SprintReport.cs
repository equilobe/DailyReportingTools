using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Jira
{
    public class SprintReport
    {
        [DataMember]
        public Sprint sprint { get; set; }
        [DataMember]
        public bool supportsPages { get; set; }
    }
}
