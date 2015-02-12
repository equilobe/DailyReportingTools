using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.JiraOriginals
{
    public class SprintsData
    {
        [DataMember]
        public int rapidViewId { get; set; }
        [DataMember]
        public List<Sprint> sprints { get; set; }
        [DataMember]
        public bool canManageSprints { get; set; }
    }
}
