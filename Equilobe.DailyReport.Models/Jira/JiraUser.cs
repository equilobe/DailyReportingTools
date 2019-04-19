using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.Jira
{
    [DataContract]
    public class JiraUser : JiraIdentifiableResponse
    {
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
