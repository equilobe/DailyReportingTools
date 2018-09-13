using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.Dashboard
{
    [DataContract]
    public class DashboardFilter
    {
        [DataMember]
        public long InstanceId { get; set; }
        
        [DataMember]
        public string Hash { get; set; }
    }
}
