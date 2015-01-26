using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.JiraModels
{
    [DataContract]
    class Sprints
    {
        [DataMember]
        public List<Sprint> sprints { get; set; }
        [DataMember]
        public string rapidViewId { get; set; }
    }
}
