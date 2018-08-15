using System;
using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.Jira
{
    [DataContract]
    public class Avatar
    {
        [DataMember(Name = "48x48")]
        public Uri Big { get; set; }

        [DataMember(Name = "24x24")]
        public Uri Small { get; set; }

        [DataMember(Name = "16x16")]
        public Uri VerySmall { get; set; }

        [DataMember(Name = "32x32")]
        public Uri Med { get; set; }
    }
}
