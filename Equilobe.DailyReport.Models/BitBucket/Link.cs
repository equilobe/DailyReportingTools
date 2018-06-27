using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.BitBucket
{
    [DataContract]
    public class Links
    {
        [DataMember(Name = "decline")]
        public Link Decline { get; set; }

        [DataMember(Name = "commits")]
        public Link Commits { get; set; }

        [DataMember(Name = "self")]
        public Link Self { get; set; }

        [DataMember(Name = "comments")]
        public Link Comments { get; set; }

        [DataMember(Name = "merge")]
        public Link Merge { get; set; }

        [DataMember(Name = "html")]
        public Link Html { get; set; }

        [DataMember(Name = "activity")]
        public Link Activity { get; set; }

        [DataMember(Name = "diff")]
        public Link Diff { get; set; }

        [DataMember(Name = "approve")]
        public Link approve { get; set; }

        [DataMember(Name = "statuses")]
        public Link Statuses { get; set; }

        [DataMember(Name = "avatar")]
        public Link Avatar { get; set; }
    }

    [DataContract]
    public class Link
    {
        [DataMember(Name = "href")]
        public string Href { get; set; }
    }
}
