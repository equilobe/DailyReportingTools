using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    public class SprintReport
    {
        [DataMember]
        public Sprint sprint { get; set; }
        [DataMember]
        public bool supportsPages { get; set; }
    }
}
