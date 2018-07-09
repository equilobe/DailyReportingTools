using System;
using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.BitBucket
{
    [DataContract]
    public class Contributor
    {
        [DataMember(Name = "username")]
        public string Username { get; set; }

        [DataMember(Name = "website")]
        public string Website { get; set; }

        [DataMember(Name = "display_name")]
        public string DisplayName { get; set; }

        [DataMember(Name = "account_id")]
        public string AccountId { get; set; }

        [DataMember(Name = "links")]
        public Links Links { get; set; }

        [DataMember(Name = "created_on")]
        public string CreatedOn { get; set; }

        [DataMember(Name = "is_staff")]
        public bool IsStaff { get; set; }

        [DataMember(Name = "location")]
        public string Location { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "uuid")]
        public string Uuid { get; set; }

        public DateTime? CreatedAt
        {
            get
            {
                try
                {
                    return Convert.ToDateTime(CreatedOn);
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
