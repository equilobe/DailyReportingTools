using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.BitBucket
{
    [DataContract]
    public class Author
    {
        [DataMember(Name = "username")]
        public string Username { get; set; }

        [DataMember(Name = "display_name")]
        public string DisplayName { get; set; }

        [DataMember(Name = "account_id")]
        public string AccountId { get; set; }

        [DataMember(Name = "links")]
        public Links Links { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "uuid")]
        public string Uuid { get; set; }
    }
}
