using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    [DataContract]
    public class View
    {
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public bool canEdit { get; set; }
        [DataMember]
        public bool sprintSupportEnabled { get; set; }
        [DataMember]
        public Filter filter { get; set; }
    }
}
