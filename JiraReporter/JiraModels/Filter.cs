using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    public class Filter
    {
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string query { get; set; }
        [DataMember]
        public bool canEdit { get; set; }
        [DataMember]
        public bool isOrderedByRank { get; set; }
    }
}
