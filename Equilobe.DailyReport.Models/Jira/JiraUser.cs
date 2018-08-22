using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.Jira
{
    [DataContract]
    public class JiraUser
    {
        [DataMember(Name = "self")]
        public string Self { get; set; }

        [DataMember(Name = "key")]
        public string Key { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "emailAddress")]
        public string EmailAddress { get; set; }

        [DataMember(Name = "displayName")]
        public string DisplayName { get; set; }

        [DataMember(Name = "active")]
        public bool IsActive { get; set; }

        [DataMember(Name = "avatarUrls")]
        public Avatar AvatarUrls { get; set; }
    }
}
